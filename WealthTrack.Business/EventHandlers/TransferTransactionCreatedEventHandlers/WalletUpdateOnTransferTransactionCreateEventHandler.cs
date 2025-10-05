using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Business.EventHandlers.TransferTransactionCreatedEventHandlers
{
    public class WalletUpdateOnTransferTransactionCreateEventHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher) : IEventHandler<TransferTransactionCreatedEvent>
    {
        public async Task Handle(TransferTransactionCreatedEvent eventMessage)
        {
            if (eventMessage is null)
            {
                throw new ArgumentException(nameof(eventMessage));
            }

            if (eventMessage.Amount == 0)
            {
                return;
            }

            var sourceWalletEntity = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.SourceWalletId);
            if(sourceWalletEntity == null)
            {
                throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.SourceWalletId.ToString()}");
            }

            var targetWalletEntity = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.TargetWalletId);
            if (targetWalletEntity == null)
            {
                throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.TargetWalletId.ToString()}");
            }
            
            sourceWalletEntity.Balance -= eventMessage.Amount;
            targetWalletEntity.Balance += eventMessage.Amount;
        }
    }
}
