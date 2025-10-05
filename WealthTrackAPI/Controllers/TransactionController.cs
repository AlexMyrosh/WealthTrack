using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WealthTrack.API.ApiModels.General;
using WealthTrack.API.ApiModels.Transaction;
using WealthTrack.Business.BusinessModels.Transaction;
using WealthTrack.Business.Services.Interfaces;

namespace WealthTrack.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransactionController(ITransactionService transactionService, IMapper mapper) : ControllerBase
{
    // GET: api/transaction
    [HttpGet]
    public async Task<ActionResult<List<TransactionDetailsApiModel>>> GetAll(
        [FromQuery] string include = "", 
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20
    )
    {
        var totalCount = await transactionService.GetCountAsync();
        var businessModels = await transactionService.GetPageAsync(pageNumber, pageSize, include);
        var apiModels = mapper.Map<List<TransactionDetailsApiModel>>(businessModels);
        var result = new PaginatedApiModel<TransactionDetailsApiModel>
        {
            Items = apiModels,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(result);
    }

    // GET api/transaction/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionDetailsApiModel>> GetById(Guid id, [FromQuery] string include = "")
    {
        var businessModel = await transactionService.GetByIdAsync(id, include);
        if (businessModel is null)
        {
            return NotFound();
        }

        var apiModel = mapper.Map<TransactionDetailsApiModel>(businessModel);
        return Ok(apiModel);
    }

    // POST api/transaction/create
    [HttpPost("create")]
    public async Task<ActionResult> Create([FromBody] TransactionUpsertApiModel model)
    {
        var businessModel = mapper.Map<TransactionUpsertBusinessModel>(model);
        var createdEntityId = await transactionService.CreateAsync(businessModel);
        return Ok(createdEntityId);
    }

    // POST api/transaction/transfer/create
    [HttpPost("transfer/create")]
    public async Task<ActionResult> Create([FromBody] TransferTransactionUpsertApiModel model)
    {
        var businessModel = mapper.Map<TransferTransactionUpsertBusinessModel>(model);
        var createdEntityId = await transactionService.CreateAsync(businessModel);
        return Ok(createdEntityId);
    }

    // PUT api/transaction/update/{id}
    [HttpPut("update/{id}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] TransactionUpsertApiModel model)
    {
        var businessModel = mapper.Map<TransactionUpsertBusinessModel>(model);
        await transactionService.UpdateAsync(id, businessModel);
        return Accepted();
    }

    // PUT api/transaction/transfer/update/{id}
    [HttpPut("transfer/update/{id}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] TransferTransactionUpsertApiModel model)
    {
        var businessModel = mapper.Map<TransferTransactionUpsertBusinessModel>(model);
        await transactionService.UpdateAsync(id, businessModel);
        return Accepted();
    }
        
    // PUT api/transaction/unassign_category/{id}
    [HttpPut("unassign_category/{id}")]
    public async Task<ActionResult> UnassignCategory(Guid id)
    {
        await transactionService.UnassignCategoryAsync(id);
        return Accepted();
    }

    // DELETE api/transaction/hard_delete/{id}
    [HttpDelete("hard_delete/{id}")]
    public async Task<ActionResult> HardDelete(Guid id)
    {
        await transactionService.HardDeleteAsync(id);
        return Accepted();
    }
}