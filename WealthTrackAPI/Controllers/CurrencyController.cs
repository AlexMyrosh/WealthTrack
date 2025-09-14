using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WealthTrack.API.ApiModels.Currency;
using WealthTrack.Business.Services.Interfaces;

namespace WealthTrack.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController(ICurrencyService currencyService, IMapper mapper) : ControllerBase
    {
        // GET: api/currency
        [HttpGet]
        public async Task<ActionResult<List<CurrencyDetailsApiModel>>> GetAll()
        {
            var businessModels = await currencyService.GetAllAsync();
            var apiModels = mapper.Map<List<CurrencyDetailsApiModel>>(businessModels);
            return Ok(apiModels);
        }

        // GET api/currency/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CurrencyDetailsApiModel>> GetById(Guid id)
        {
            try
            {
                var businessModel = await currencyService.GetByIdAsync(id);
                if (businessModel is null)
                {
                    return NotFound();
                }

                var apiModel = mapper.Map<CurrencyDetailsApiModel>(businessModel);
                return Ok(apiModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}