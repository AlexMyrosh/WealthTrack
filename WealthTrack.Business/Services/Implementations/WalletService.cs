using AutoMapper;
using WealthTrack.Business.BusinessModels;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Services.Implementations
{
    public class WalletService(IUnitOfWork unitOfWork, IMapper mapper) : IWalletService
    {
        public async Task<WalletBusinessModel> CreateAsync(WalletBusinessModel model)
        {
            var domainModel = mapper.Map<Wallet>(model);
            var createdDomainModel = await unitOfWork.WalletRepository.CreateAsync(domainModel);
            await unitOfWork.SaveAsync();
            var result = mapper.Map<WalletBusinessModel>(createdDomainModel);
            return result;
        }

        public async Task<WalletBusinessModel?> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var domainModel = await unitOfWork.WalletRepository.GetByIdAsync(id);
            var result = mapper.Map<WalletBusinessModel>(domainModel);
            return result;
        }

        public async Task<List<WalletBusinessModel>> GetAllAsync()
        {
            var domainModels = await unitOfWork.WalletRepository.GetAllAsync();
            var result = mapper.Map<List<WalletBusinessModel>>(domainModels);
            return result;
        }

        public async Task<WalletBusinessModel> UpdateAsync(WalletBusinessModel model)
        {
            var domainModel = mapper.Map<Wallet>(model);
            var updatedDomainModel = unitOfWork.WalletRepository.Update(domainModel);
            await unitOfWork.SaveAsync();
            var result = mapper.Map<WalletBusinessModel>(updatedDomainModel);
            return result;
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

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var domainModel = await unitOfWork.WalletRepository.GetByIdAsync(id);
            if (domainModel is null)
            {
                return false;
            }

            domainModel.Status = WalletStatus.Deleted;
            await unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> ArchivedAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var domainModel = await unitOfWork.WalletRepository.GetByIdAsync(id);
            if (domainModel is null)
            {
                return false;
            }

            domainModel.Status = WalletStatus.Archived;
            await unitOfWork.SaveAsync();
            return true;
        }
    }
}
