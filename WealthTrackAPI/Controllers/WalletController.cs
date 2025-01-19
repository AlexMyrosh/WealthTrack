using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WealthTrack.API.ApiModels.Wallet;
using WealthTrack.Business.BusinessModels.Wallet;
using WealthTrack.Business.Services.Interfaces;

namespace WealthTrack.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController(IWalletService walletService, IMapper mapper) : ControllerBase
    {
        // GET: api/wallet
        [HttpGet]
        public async Task<ActionResult<List<WalletDetailsApiModel>>> GetAll([FromQuery] string include = "")
        {
            var businessModels = await walletService.GetAllAsync(include);
            var apiModels = mapper.Map<List<WalletDetailsApiModel>>(businessModels);
            return Ok(apiModels);
        }

        // GET api/wallet/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<WalletDetailsApiModel>> GetById(Guid id, [FromQuery] string include = "")
        {
            var businessModel = await walletService.GetByIdAsync(id, include);
            if (businessModel is null)
            {
                return NotFound();
            }

            var apiModel = mapper.Map<WalletDetailsApiModel>(businessModel);
            return Ok(apiModel);
        }

        // POST api/wallet/create
        [HttpPost("create")]
        public async Task<ActionResult> Create([FromBody] CreateWalletApiModel model)
        {
            var businessModel = mapper.Map<CreateWalletBusinessModel>(model);
            await walletService.CreateAsync(businessModel);
            return Created();
        }

        // PUT api/wallet/update
        [HttpPut("update")]
        public async Task<ActionResult> Update([FromBody] UpdateWalletApiModel model)
        {
            var businessModel = mapper.Map<UpdateWalletBusinessModel>(model);
            await walletService.UpdateAsync(businessModel);
            return Accepted();
        }

        // DELETE api/wallet/hard_delete
        [HttpDelete("hard_delete/{id}")]
        public async Task<ActionResult> HardDelete(Guid id)
        {
            var result = await walletService.HardDeleteAsync(id);
            if (result)
            {
                return NoContent();
            }

            return BadRequest();
        }
    }
}