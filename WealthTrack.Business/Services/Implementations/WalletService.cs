using AutoMapper;
using WealthTrack.Business.BusinessModels.Wallet;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Services.Implementations
{
    public class WalletService(IUnitOfWork unitOfWork, IMapper mapper) : IWalletService
    {
        public async Task CreateAsync(CreateWalletBusinessModel model)
        {
            var domainModel = mapper.Map<Wallet>(model);
            domainModel.CreatedDate = DateTimeOffset.Now;
            domainModel.ModifiedDate = DateTimeOffset.Now;
            domainModel.Status = WalletStatus.Active;
            await unitOfWork.WalletRepository.CreateAsync(domainModel);
            await unitOfWork.SaveAsync();
        }

        public async Task<WalletDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "")
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var domainModel = await unitOfWork.WalletRepository.GetByIdAsync(id, include);
            var result = mapper.Map<WalletDetailsBusinessModel>(domainModel);
            return result;
        }

        public async Task<List<WalletDetailsBusinessModel>> GetAllAsync(string include = "")
        {
            var domainModels = await unitOfWork.WalletRepository.GetAllAsync(include);
            var result = mapper.Map<List<WalletDetailsBusinessModel>>(domainModels);
            return result;
        }

        public async Task UpdateAsync(UpdateWalletBusinessModel model)
        {
            var originalModel = await unitOfWork.WalletRepository.GetByIdAsync(model.Id);
            mapper.Map(model, originalModel);
            if (originalModel is null)
            {
                throw new AutoMapperMappingException("Entity is null after mapping");
            }

            originalModel.ModifiedDate = DateTimeOffset.Now;
            unitOfWork.WalletRepository.Update(originalModel);
            await unitOfWork.SaveAsync();
        }

        public async Task<bool> HardDeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var deletedDomainModel = await unitOfWork.WalletRepository.HardDeleteAsync(id);
            if (deletedDomainModel is null)
            {
                return false;
            }

            await unitOfWork.SaveAsync();
            return true;
        }
    }
}
