using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WealthTrack.API.ApiModels.Budget;
using WealthTrack.Business.BusinessModels.Budget;
using WealthTrack.Business.Services.Interfaces;

namespace WealthTrack.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BudgetController(IBudgetService budgetService, IMapper mapper) : ControllerBase
    {
        // GET: api/budget
        [HttpGet]
        public async Task<ActionResult<List<BudgetDetailsApiModel>>> GetAll([FromQuery] string include = "")
        {
            var businessModels = await budgetService.GetAllAsync(include);
            var apiModels = mapper.Map<List<BudgetDetailsApiModel>>(businessModels);
            return Ok(apiModels);
        }

        // GET api/budget/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<BudgetDetailsApiModel>> GetById(Guid id, [FromQuery] string include = "")
        {
            var businessModel = await budgetService.GetByIdAsync(id, include);
            if (businessModel is null)
            {
                return NotFound();
            }

            var apiModel = mapper.Map<BudgetDetailsApiModel>(businessModel);
            return Ok(apiModel);
        }

        // POST api/budget/create
        [HttpPost("create")]
        public async Task<ActionResult> Create([FromBody] BudgetUpsertApiModel model)
        {
            try
            {
                var businessModel = mapper.Map<BudgetUpsertBusinessModel>(model);
                var createdEntityId = await budgetService.CreateAsync(businessModel);
                return Ok(createdEntityId);
            }
            catch (ArgumentNullException)
            {
                return BadRequest();
            }
        }

        // PUT api/budget/update/{id}
        [HttpPut("update/{id}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] BudgetUpsertApiModel model)
        {
            try
            {
                var businessModel = mapper.Map<BudgetUpsertBusinessModel>(model);
                await budgetService.UpdateAsync(id, businessModel);
                return Accepted();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // DELETE api/budget/hard_delete
        [HttpDelete("hard_delete/{id}")]
        public async Task<ActionResult> HardDelete(Guid id)
        {
            await budgetService.HardDeleteAsync(id);
            return Accepted();
        }
    }
}