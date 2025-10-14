using Ecommerce.Application.Features.Categories.Queries.GetCategoryList;
using Ecommerce.Application.Features.Categories.Vms;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Ecommerce.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        ///  Método publico que me permite listar los Categorias
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("list",Name = "GetCategories")]
        [ProducesResponseType(typeof(IReadOnlyList<CategoryVm>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IReadOnlyList<CategoryVm>>> GetCategories()
        {
            var query = new GetCategoryListQuery();
            return Ok(await _mediator.Send(query));
        }
    }
}
