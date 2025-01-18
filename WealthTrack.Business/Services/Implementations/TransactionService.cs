using AutoMapper;
using WealthTrack.Business.BusinessModels;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.UnitOfWork;
using Transaction = WealthTrack.Data.DomainModels.Transaction;

namespace WealthTrack.Business.Services.Implementations
{
    public class TransactionService(IUnitOfWork unitOfWork, IMapper mapper) : ITransactionService
    {
        public async Task<TransactionBusinessModel> CreateAsync(TransactionBusinessModel model)
        {
            var domainModel = mapper.Map<Transaction>(model);
            var createdDomainModel = await unitOfWork.TransactionRepository.CreateAsync(domainModel);
            await unitOfWork.SaveAsync();
            var result = mapper.Map<TransactionBusinessModel>(createdDomainModel);
            return result;
        }

        public async Task<TransactionBusinessModel?> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var domainModel = await unitOfWork.TransactionRepository.GetByIdAsync(id);
            var result = mapper.Map<TransactionBusinessModel>(domainModel);
            return result;
        }

        public async Task<List<TransactionBusinessModel>> GetAllAsync()
        {
            var domainModels = await unitOfWork.TransactionRepository.GetAllAsync();
            var result = mapper.Map<List<TransactionBusinessModel>>(domainModels);
            return result;
        }

        public async Task<TransactionBusinessModel> UpdateAsync(TransactionBusinessModel model)
        {
            var domainModel = mapper.Map<Transaction>(model);
            var updatedDomainModel = unitOfWork.TransactionRepository.Update(domainModel);
            await unitOfWork.SaveAsync();
            var result = mapper.Map<TransactionBusinessModel>(updatedDomainModel);
            return result;
        }

        public async Task<bool> HardDeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var deletedDomainModel = await unitOfWork.TransactionRepository.HardDeleteAsync(id);
            if (deletedDomainModel is null)
            {
                return false;
            }

            await unitOfWork.SaveAsync();
            return true;
        }
    }
}