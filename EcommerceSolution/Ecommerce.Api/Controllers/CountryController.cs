using Ecommerce.Application.Features.Countries.Queries.GetCountryList;
using Ecommerce.Application.Features.Countries.Vms;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Ecommerce.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CountryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        ///  Método publico que me permite listar los pasises
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet(Name = "GetCountries")]
        [ProducesResponseType(typeof(IReadOnlyList<CountryVm>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IReadOnlyList<CountryVm>>> GetCountries()
        {
            var query = new GetCountryListQuery();
            return Ok(await _mediator.Send(query));
        }
    }
}
