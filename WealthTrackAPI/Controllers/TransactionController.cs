using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WealthTrack.API.ApiModels.Transaction;
using WealthTrack.Business.BusinessModels.Transaction;
using WealthTrack.Business.Services.Interfaces;

namespace WealthTrack.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController(ITransactionService transactionService, IMapper mapper) : ControllerBase
    {
        // GET: api/transaction
        [HttpGet]
        public async Task<ActionResult<List<TransactionDetailsApiModel>>> GetAll([FromQuery] string include = "")
        {
            try
            {
                var businessModels = await transactionService.GetAllAsync(include);
                var apiModels = mapper.Map<List<TransactionDetailsApiModel>>(businessModels);
                return Ok(apiModels);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/transaction/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDetailsApiModel>> GetById(Guid id, [FromQuery] string include = "")
        {
            try
            {
                var businessModel = await transactionService.GetByIdAsync(id, include);
                if (businessModel is null)
                {
                    return NotFound();
                }

                var apiModel = mapper.Map<TransactionDetailsApiModel>(businessModel);
                return Ok(apiModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/transaction/create
        [HttpPost("create")]
        public async Task<ActionResult> Create([FromBody] TransactionUpsertApiModel model)
        {
            try
            {
                var businessModel = mapper.Map<TransactionUpsertBusinessModel>(model);
                var createdEntityId = await transactionService.CreateAsync(businessModel);
                return Ok(createdEntityId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/transaction/transfer/create
        [HttpPost("transfer/create")]
        public async Task<ActionResult> Create([FromBody] TransferTransactionUpsertApiModel model)
        {
            try
            {
                var businessModel = mapper.Map<TransferTransactionUpsertBusinessModel>(model);
                var createdEntityId = await transactionService.CreateAsync(businessModel);
                return Ok(createdEntityId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT api/transaction/update/{id}
        [HttpPut("update/{id}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] TransactionUpsertApiModel model)
        {
            try
            {
                var businessModel = mapper.Map<TransactionUpsertBusinessModel>(model);
                await transactionService.UpdateAsync(id, businessModel);
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

        // PUT api/transaction/transfer/update/{id}
        [HttpPut("transfer/update/{id}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] TransferTransactionUpsertApiModel model)
        {
            try
            {
                var businessModel = mapper.Map<TransferTransactionUpsertBusinessModel>(model);
                await transactionService.UpdateAsync(id, businessModel);
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
        
        // PUT api/transaction/unassign_category/{id}
        [HttpPut("unassign_category/{id}")]
        public async Task<ActionResult> UnassignCategory(Guid id)
        {
            try
            {
                await transactionService.UnassignCategoryAsync(id);
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

        // DELETE api/transaction/hard_delete/{id}
        [HttpDelete("hard_delete/{id}")]
        public async Task<ActionResult> HardDelete(Guid id)
        {
            try
            {
                await transactionService.HardDeleteAsync(id);
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
}