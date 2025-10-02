using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WealthTrack.API.ApiModels.Wallet;
using WealthTrack.Business.BusinessModels.Wallet;
using WealthTrack.Business.Services.Interfaces;

namespace WealthTrack.API.Controllers;

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
    public async Task<ActionResult> Create([FromBody] WalletUpsertApiModel model)
    {
        var businessModel = mapper.Map<WalletUpsertBusinessModel>(model);
        var createdEntityId = await walletService.CreateAsync(businessModel);
        return Ok(createdEntityId);
    }

    // PUT api/wallet/update/{id}
    [HttpPut("update/{id}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] WalletUpsertApiModel model)
    {
        var businessModel = mapper.Map<WalletUpsertBusinessModel>(model);
        await walletService.UpdateAsync(id, businessModel);
        return Accepted();
    }

    // DELETE api/wallet/hard_delete/{id}
    [HttpDelete("hard_delete/{id}")]
    public async Task<ActionResult> HardDelete(Guid id)
    {
        await walletService.HardDeleteAsync(id);
        return Accepted();
    }
    
    // DELETE api/wallet/archive/{id}
    [HttpDelete("archive/{id}")]
    public async Task<ActionResult> Archive(Guid id)
    {
        await walletService.ArchiveAsync(id);
        return Accepted();
    }
}