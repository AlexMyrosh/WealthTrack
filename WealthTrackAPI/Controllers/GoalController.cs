using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WealthTrack.API.ApiModels.Goal;
using WealthTrack.Business.BusinessModels.Goal;
using WealthTrack.Business.Services.Interfaces;

namespace WealthTrack.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoalController(IGoalService goalService, IMapper mapper) : ControllerBase
    {
        // GET: api/goal
        [HttpGet]
        public async Task<ActionResult<List<GoalDetailsApiModel>>> GetAll([FromQuery] string include = "")
        {
            var businessModels = await goalService.GetAllAsync(include);
            var apiModels = mapper.Map<List<GoalDetailsApiModel>>(businessModels);
            return Ok(apiModels);
        }

        // GET api/goal/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<GoalDetailsApiModel>> GetById(Guid id, [FromQuery] string include = "")
        {
            var businessModel = await goalService.GetByIdAsync(id, include);
            if (businessModel is null)
            {
                return NotFound();
            }

            var apiModel = mapper.Map<GoalDetailsApiModel>(businessModel);
            return Ok(apiModel);
        }

        // POST api/goal/create
        [HttpPost("create")]
        public async Task<ActionResult> Create([FromBody] GoalUpsertApiModel model)
        {
            var businessModel = mapper.Map<GoalUpsertBusinessModel>(model);
            var createdEntityId = await goalService.CreateAsync(businessModel);
            return Ok(createdEntityId);
        }

        // PUT api/goal/update/{id}
        [HttpPut("update/{id}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] GoalUpsertApiModel model)
        {
            try
            {
                var businessModel = mapper.Map<GoalUpsertBusinessModel>(model);
                await goalService.UpdateAsync(id, businessModel);
                return Accepted();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // DELETE api/goal/hard_delete
        [HttpDelete("hard_delete/{id}")]
        public async Task<ActionResult> HardDelete(Guid id)
        {
            await goalService.HardDeleteAsync(id);
            return Accepted();
        }
    }
}