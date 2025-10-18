using Ecommerce.Application.Contracts.Infrastructure;
using Ecommerce.Application.Features.Products.Commands.CreateProduct;
using Ecommerce.Application.Features.Products.Commands.DeleteProduct;
using Ecommerce.Application.Features.Products.Commands.UpdateProduct;
using Ecommerce.Application.Features.Products.Queries.GetProductById;
using Ecommerce.Application.Features.Products.Queries.GetProductList;
using Ecommerce.Application.Features.Products.Queries.PaginationProducts;
using Ecommerce.Application.Features.Products.Queries.Vms;
using Ecommerce.Application.Features.Shared.Queries;
using Ecommerce.Application.Models.Authorization;
using Ecommerce.Application.Models.ImageManagement;
using Ecommerce.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Ecommerce.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IManageImageService _manageImageService;

        public ProductController(IMediator mediator, IManageImageService manageImageService)
        {
            _mediator = mediator;
            _manageImageService = manageImageService;
        }

        [AllowAnonymous]
        [HttpGet("list", Name = "GetProductList")]
        [ProducesResponseType(typeof(IReadOnlyList<ProductVm>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IReadOnlyList<ProductVm>>> GetProductList()
        {
            var query = new GetProductListQuery();
            var productos = await _mediator.Send(query);
            return Ok(productos);
        }

        /// <summary>
        /// Devuelve una lista paginada de productos **activos** para consumo público.
        /// El filtro de estado se fuerza a <see cref="ProductStatus.Activo"/> sin requerirlo del cliente
        /// </summary>
        /// <param name="paginationProductsQuery"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("pagination", Name = "PaginationProduct")]
        [ProducesResponseType(typeof(PaginationVm<ProductVm>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginationVm<ProductVm>>> PaginationProduct([FromQuery] PaginationProductsQuery paginationProductsQuery)
        {
            paginationProductsQuery.Status = ProductStatus.Activo;
            var paginationProduct = await _mediator.Send(paginationProductsQuery);
            return Ok(paginationProduct);
        }

        /// <summary>
        /// Devuelve una lista paginada de productos para uso administrativo (incluye activos/inactivos).
        /// Solo accesible para usuarios con rol <c>ADMIN</c>. Permite aplicar filtros y ordenamiento globales.
        /// </summary>
        /// <param name="paginationProductsQuery"></param>
        /// <returns></returns>
        [Authorize(Roles = Role.ADMIN)]
        [HttpGet("paginationAdmin", Name = "PaginationProductAdmin")]
        [ProducesResponseType(typeof(PaginationVm<ProductVm>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginationVm<ProductVm>>> PaginationProductAdmin([FromQuery] PaginationProductsQuery paginationProductsQuery)
        {
            var paginationProduct = await _mediator.Send(paginationProductsQuery);
            return Ok(paginationProduct);
        }

        [AllowAnonymous]
        [HttpGet("{id}", Name = "GetProductById")]
        [ProducesResponseType(typeof(ProductVm), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginationVm<ProductVm>>> GetProductById(int id)
        {
            var query = new GetProductByIdQuery(id);
            return Ok(await _mediator.Send(query));
        }

        /// <summary>
        /// Crea un nuevo producto en el sistema, incluyendo la carga de sus imágenes
        /// al servicio de almacenamiento configurado.
        /// Solo los administradores pueden ejecutar este endpoint.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = Role.ADMIN)]
        [HttpPost("create", Name = "CreateProduct")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<ProductVm>> CreateProduct([FromForm] CreateProductCommand request)
        {
            var listFotoUrls = new List<CreateProductImageCommand>();

            if (request.Fotos is not null)
            {
                foreach (var foto in request.Fotos)
                {
                    var resultImage = await _manageImageService.UploadImageAsync(new ImageData
                    {
                        ImageStream = foto.OpenReadStream(),
                        Nombre = foto.Name
                    });

                    var fotoCommand = new CreateProductImageCommand
                    {
                        PublicCode = resultImage.PublicId,
                        Url = resultImage.Url
                    };

                    listFotoUrls.Add(fotoCommand);
                }

                request.ImageUrls = listFotoUrls;
            }

            return await _mediator.Send(request);
        }

        /// <summary>
        /// Actualiza la información de un producto existente, incluyendo sus imágenes.
        /// Solo los administradores pueden ejecutar este endpoint.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = Role.ADMIN)]
        [HttpPut("update", Name = "UpdateProduct")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<ProductVm>> UpdateProduct([FromForm] UpdateProductCommand request)
        {
            var listFotoUrls = new List<CreateProductImageCommand>();

            if (request.Fotos is not null)
            {
                foreach (var foto in request.Fotos)
                {
                    var resultImage = await _manageImageService.UploadImageAsync(new ImageData
                    {
                        ImageStream = foto.OpenReadStream(),
                        Nombre = foto.Name
                    });

                    var fotoCommand = new CreateProductImageCommand
                    {
                        PublicCode = resultImage.PublicId,
                        Url = resultImage.Url
                    };

                    listFotoUrls.Add(fotoCommand);
                }

                request.ImageUrls = listFotoUrls;
            }

            return await _mediator.Send(request);
        }

        /// <summary>
        /// Maneja el comando <see cref="DeleteProductCommand"/> alternando el estado del producto 
        /// entre activo e inactivo, sin realizar una eliminación física.
        /// Solo los administradores pueden ejecutar este endpoint.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = Role.ADMIN)]
        [HttpDelete("status/{id}", Name = "UpdateStatusProduct")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<ProductVm>> UpdateStatusProduct(int id)
        {
            var request = new DeleteProductCommand(id);
            return await _mediator.Send(request);
        }
    }
}
