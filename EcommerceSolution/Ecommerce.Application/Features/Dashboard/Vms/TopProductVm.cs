namespace Ecommerce.Application.Features.Dashboard.Vms
{
    public class TopProductVm
    {
        public int ProductId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
    }
}
