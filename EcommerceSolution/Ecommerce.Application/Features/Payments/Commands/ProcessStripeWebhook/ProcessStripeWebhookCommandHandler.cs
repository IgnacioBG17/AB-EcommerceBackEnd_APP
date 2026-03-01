using Ecommerce.Application.Contracts.Infrastructure;
using Ecommerce.Application.Models.Payment;
using Ecommerce.Application.Persistence;
using Ecommerce.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Stripe;
using System.Linq.Expressions;

namespace Ecommerce.Application.Features.Payments.Commands.ProcessStripeWebhook
{
    public class ProcessStripeWebhookCommandHandler : IRequestHandler<ProcessStripeWebhookCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly StripeSettings _stripeSettings;
        private readonly ILogger<ProcessStripeWebhookCommandHandler> _logger;

        public ProcessStripeWebhookCommandHandler(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IOptions<StripeSettings> stripeSettings,
            ILogger<ProcessStripeWebhookCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _stripeSettings = stripeSettings.Value;
            _logger = logger;
        }

        public async Task<MediatR.Unit> Handle(ProcessStripeWebhookCommand request, CancellationToken cancellationToken)
        {
            //var webhookSecret = _configuration["StripeSettings:WebhookSecret"];
            var webhookSecret = _stripeSettings.WebhookSecret;
            Event stripeEvent;

            try
            {
                // Validar firma de forma segura
                stripeEvent = EventUtility.ConstructEvent(request.Payload, request.SignatureHeader, webhookSecret, throwOnApiVersionMismatch: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo en la validación del Webhook de Stripe. Verifica el Secret.");
                return MediatR.Unit.Value;
            }

            // Cast seguro del objeto
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return MediatR.Unit.Value;

            switch (stripeEvent.Type)
            {
                case EventTypes.PaymentIntentSucceeded:
                    await ProcessSucceededPayment(paymentIntent);
                    break;

                case EventTypes.PaymentIntentPaymentFailed:
                    await ProcessFailedPayment(paymentIntent);
                    break;
            }

            return MediatR.Unit.Value;
        }

        private async Task ProcessSucceededPayment(PaymentIntent paymentIntent)
        {
            // Extraemos los IDs de la metadata que enviamos en el paso anterior
            paymentIntent.Metadata.TryGetValue("orderId", out var orderIdStr);
            paymentIntent.Metadata.TryGetValue("ShoppingCartMasterId", out var cartIdStr);

            // 1. Actualizar Orden a "Completed" (Ya no Pending)
            var orderToPay = await _unitOfWork.Repository<Order>().GetEntityAsync(x => x.PaymentIntentId == paymentIntent.Id);
            orderToPay.Status = OrderStatus.Completed;
            _unitOfWork.Repository<Order>().UpdateEntity(orderToPay);

            // 2. BORRAR EL CARRITO 
            if (Guid.TryParse(cartIdStr, out var cartId))
            {
                var items = await _unitOfWork.Repository<ShoppingCartItem>()
                                    .GetAsync(x => x.ShoppingCartMasterId == cartId);

                _unitOfWork.Repository<ShoppingCartItem>().DeleteRange(items);
            }

            await _unitOfWork.Complete();

            // 3. GENERAR Y ENVIAR FACTURA POR SENDGRID
            try
            {
                // Cargamos la orden con sus detalles (OrderItems) para que aparezcan en el PDF
                var orderWithItems = await _unitOfWork.Repository<Order>().GetEntityAsync(
                    x => x.Id == orderToPay.Id,
                    includes: new List<Expression<Func<Order, object>>>
                    {
                        x => x.OrderItems!,
                        x => x.OrderAddress 
                    },
                    disableTracking: true
                );

                if (orderWithItems != null)
                {
                    byte[] pdfInvoice = GenerateInvoiceData(orderWithItems);

                    var result = await _emailService.SendOrderInvoiceSengridAsync(orderWithItems, pdfInvoice);

                    if (result)
                        _logger.LogInformation("Factura enviada exitosamente para la orden {Id}", orderWithItems.Id);
                    else
                        _logger.LogWarning("SendGrid aceptó el correo pero reportó un fallo en el envío para la orden {Id}", orderWithItems.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "El pago fue exitoso pero falló el envío del correo de factura.");
            }
        }

        private async Task ProcessFailedPayment(PaymentIntent paymentIntent)
        {
            // Buscamos la orden asociada al PaymentIntent que falló
            var order = await _unitOfWork.Repository<Order>()
                .GetEntityAsync(
                    x => x.PaymentIntentId == paymentIntent.Id,
                    null,
                    false
                );

            if (order == null)
            {
                _logger.LogWarning("No se encontró la orden para el pago fallido. PaymentIntentId: {Id}", paymentIntent.Id);
                return;
            }

            // Solo actualizamos si no estaba ya en estado de error
            if (order.Status != OrderStatus.Error)
            {
                order.Status = OrderStatus.Error;

                // Opcional: Stripe suele enviar la razón del fallo en LastPaymentError
                var reason = paymentIntent.LastPaymentError?.Message ?? "Pago rechazado por Stripe";

                _logger.LogWarning("El pago para la Orden {OrderId} falló. Razón: {Reason}", order.Id, reason);

                _unitOfWork.Repository<Order>().UpdateEntity(order);
                await _unitOfWork.Complete();

                _logger.LogInformation("Orden {OrderId} marcada como Error correctamente.", order.Id);
            }
            else
            {
                _logger.LogInformation("La orden {OrderId} ya estaba marcada como Error.", order.Id);
            }
        }

        private byte[] GenerateInvoiceData(Order order)
        {
            // QuestPDF requiere configurar el tipo de licencia (Community es gratis para proyectos pequeños/personales)
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                    // --- ENCABEZADO ---
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("FACTURA DE COMPRA").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text($"Orden ID: #{order.Id}");
                            col.Item().Text($"Fecha: {order.CreateDate:dd/MM/yyyy HH:mm}");
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("TU ECOMMERCE S.A.").FontSize(14).Bold();
                            col.Item().Text("Managua, Nicaragua");
                            col.Item().Text("soporte@tudominio.com");
                        });
                    });

                    // --- INFORMACIÓN DEL CLIENTE Y ENVÍO ---
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Facturar a:").SemiBold();
                                c.Item().Text(order.CompradorNombre ?? "Cliente");
                                c.Item().Text(order.CompradorUserName ?? "");
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Enviar a:").SemiBold();
                                c.Item().Text($"{order.OrderAddress?.Direccion ?? "N/A"}, {order.OrderAddress?.Ciudad ?? ""}");
                                c.Item().Text($"{order.OrderAddress?.Departamento ?? ""}, {order.OrderAddress?.CodigoPostal ?? ""}");
                                c.Item().Text(order.OrderAddress?.Pais ?? "");
                            });
                        });

                        col.Item().PaddingTop(20);

                        // --- TABLA DE PRODUCTOS ---
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Producto
                                columns.RelativeColumn();  // Precio
                                columns.RelativeColumn();  // Cantidad
                                columns.RelativeColumn();  // Total
                            });

                            // Encabezado de la tabla
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Producto");
                                header.Cell().Element(CellStyle).AlignRight().Text("Precio");
                                header.Cell().Element(CellStyle).AlignCenter().Text("Cant.");
                                header.Cell().Element(CellStyle).AlignRight().Text("Total");

                                static IContainer CellStyle(IContainer container) =>
                                    container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            });

                            // Filas de productos
                            foreach (var item in order.OrderItems)
                            {
                                table.Cell().Element(ContentStyle).Text(item.ProductNombre);
                                table.Cell().Element(ContentStyle).AlignRight().Text($"{item.Precio:C}");
                                table.Cell().Element(ContentStyle).AlignCenter().Text(item.Cantidad.ToString());
                                table.Cell().Element(ContentStyle).AlignRight().Text($"{(item.Precio * item.Cantidad):C}");

                                static IContainer ContentStyle(IContainer container) =>
                                    container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                            }
                        });

                        // --- TOTALES ---
                        col.Item().AlignRight().PaddingTop(10).Column(c =>
                        {
                            c.Item().Text($"Subtotal: {order.SubTotal:C}");
                            c.Item().Text($"Impuestos: {order.Impuesto:C}");
                            c.Item().Text($"Envío: {order.PrecioEnvio:C}");
                            c.Item().PaddingTop(5).Text($"TOTAL: {order.Total:C}").FontSize(14).Bold().FontColor(Colors.Blue.Medium);
                        });
                    });

                    // --- PIE DE PÁGINA ---
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Gracias por confiar en nosotros. Página ");
                        x.CurrentPageNumber();
                    });
                });
            }).GeneratePdf();
        }
    }
}
