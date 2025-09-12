using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WealthTrack.API.ApiModels.Category;
using WealthTrack.Business.BusinessModels.Category;
using WealthTrack.Business.Services.Interfaces;

namespace WealthTrack.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(ICategoryService categoryService, IMapper mapper) : ControllerBase
{
    // GET: api/category
    [HttpGet]
    public async Task<ActionResult<List<CategoryDetailsApiModel>>> GetAll()
    {
        try
        {
            var businessModels = await categoryService.GetAllAsync();
            var apiModels = mapper.Map<List<CategoryDetailsApiModel>>(businessModels);
            return Ok(apiModels);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET api/category/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDetailsApiModel>> GetById(Guid id, [FromQuery] string include = "")
    {
        try
        {
            var businessModel = await categoryService.GetByIdAsync(id, include);
            if (businessModel is null)
            {
                return NotFound();
            }

            var apiModel = mapper.Map<CategoryDetailsApiModel>(businessModel);
            return Ok(apiModel);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST api/category/create
    [HttpPost("create")]
    public async Task<ActionResult> Create([FromBody] CategoryUpsertApiModel model)
    {
        try
        {
            var businessModel = mapper.Map<CategoryUpsertBusinessModel>(model);
            var createdEntityId = await categoryService.CreateAsync(businessModel);
            return Ok(createdEntityId);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT api/category/update/{id}
    [HttpPut("update/{id}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] CategoryUpsertApiModel model)
    {
        try
        {
            var businessModel = mapper.Map<CategoryUpsertBusinessModel>(model);
            await categoryService.UpdateAsync(id, businessModel);
            return Accepted();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE api/category/hard_delete/{id}
    [HttpDelete("hard_delete/{id}")]
    public async Task<ActionResult> HardDelete(Guid id)
    {
        try
        {
            await categoryService.HardDeleteAsync(id);
            return Accepted();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}