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
            var businessModels = await transactionService.GetAllAsync(include);
            var apiModels = mapper.Map<List<TransactionDetailsApiModel>>(businessModels);
            return Ok(apiModels);
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
        public async Task<ActionResult> Create([FromBody] CreateTransactionApiModel model)
        {
            var businessModel = mapper.Map<CreateTransactionBusinessModel>(model);
            await transactionService.CreateAsync(businessModel);
            return Created();
        }

        // PUT api/transaction/update
        [HttpPut("update")]
        public async Task<ActionResult> Update([FromBody] UpdateTransactionApiModel model)
        {
            var businessModel = mapper.Map<UpdateTransactionBusinessModel>(model);
            await transactionService.UpdateAsync(businessModel);
            return Accepted();
        }

        // DELETE api/transaction/hard_delete/{id}
        [HttpDelete("hard_delete/{id}")]
        public async Task<ActionResult> HardDelete(Guid id)
        {
            var result = await transactionService.HardDeleteAsync(id);
            if (result)
            {
                return NoContent();
            }

            return BadRequest();
        }
    }
}