using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WealthTrack.API.ApiModels.Category;
using WealthTrack.Business.BusinessModels.Category;
using WealthTrack.Business.Services.Interfaces;

namespace WealthTrack.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(ICategoryService categoryService, IMapper mapper) : ControllerBase
    {
        // GET: api/category
        [HttpGet]
        public async Task<ActionResult<List<CategoryDetailsApiModel>>> GetAll([FromQuery] string include = "")
        {
            var businessModels = await categoryService.GetAllAsync(include);
            var apiModels = mapper.Map<List<CategoryDetailsApiModel>>(businessModels);
            return Ok(apiModels);
        }

        // GET api/category/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDetailsApiModel>> GetById(Guid id, [FromQuery] string include = "")
        {
            var businessModel = await categoryService.GetByIdAsync(id, include);
            if (businessModel is null)
            {
                return NotFound();
            }

            var apiModel = mapper.Map<CategoryDetailsApiModel>(businessModel);
            return Ok(apiModel);
        }

        // POST api/category/create
        [HttpPost("create")]
        public async Task<ActionResult> Create([FromBody] CategoryUpsertApiModel model)
        {
            var businessModel = mapper.Map<CategoryUpsertBusinessModel>(model);
            var createdEntityId = await categoryService.CreateAsync(businessModel);
            return Ok(createdEntityId);
        }

        // PUT api/category/update/{id}
        [HttpPut("update/{id}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] CategoryUpsertApiModel model)
        {
            var businessModel = mapper.Map<CategoryUpsertBusinessModel>(model);
            await categoryService.UpdateAsync(id, businessModel);
            return Accepted();
        }

        // DELETE api/category/hard_delete/{id}
        [HttpDelete("hard_delete/{id}")]
        public async Task<ActionResult> HardDelete(Guid id)
        {
            var result = await categoryService.HardDeleteAsync(id);
            if (result)
            {
                return NoContent();
            }

            return BadRequest();
        }

        // DELETE api/category/soft_delete/{id}
        [HttpDelete("soft_delete/{id}")]
        public async Task<ActionResult> SoftDelete(Guid id)
        {
            var result = await categoryService.SoftDeleteAsync(id);
            if (result)
            {
                return NoContent();
            }

            return BadRequest();
        }
    }
}
