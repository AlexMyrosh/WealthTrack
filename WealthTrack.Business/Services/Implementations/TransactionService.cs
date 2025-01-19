using AutoMapper;
using WealthTrack.Business.BusinessModels.Transaction;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.UnitOfWork;
using Transaction = WealthTrack.Data.DomainModels.Transaction;

namespace WealthTrack.Business.Services.Implementations
{
    public class TransactionService(IUnitOfWork unitOfWork, IMapper mapper) : ITransactionService
    {
        public async Task CreateAsync(CreateTransactionBusinessModel model)
        {
            var domainModel = mapper.Map<Transaction>(model);
            domainModel.CreatedDate = DateTimeOffset.Now;
            await unitOfWork.TransactionRepository.CreateAsync(domainModel);
            var wallet = await unitOfWork.WalletRepository.GetByIdAsync(domainModel.WalletId);
            if (wallet is null)
            {
                throw new ArgumentException($"Unable to get wallet by id. Id value: {domainModel.WalletId}");
            }

            await unitOfWork.SaveAsync();
        }

        public async Task<TransactionDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "")
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var domainModel = await unitOfWork.TransactionRepository.GetByIdAsync(id, include);
            var result = mapper.Map<TransactionDetailsBusinessModel>(domainModel);
            return result;
        }

        public async Task<List<TransactionDetailsBusinessModel>> GetAllAsync(string include = "")
        {
            var domainModels = await unitOfWork.TransactionRepository.GetAllAsync(include);
            var result = mapper.Map<List<TransactionDetailsBusinessModel>>(domainModels);
            return result;
        }

        public async Task UpdateAsync(UpdateTransactionBusinessModel model)
        {
            var originalModel = await unitOfWork.TransactionRepository.GetByIdAsync(model.Id, "Wallet");
            mapper.Map(model, originalModel);
            if (originalModel is null)
            {
                throw new AutoMapperMappingException("Entity is null after mapping");
            }

            unitOfWork.TransactionRepository.Update(originalModel);
            await unitOfWork.SaveAsync();
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