using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Business.EventHandlers.TransferTransactionDeletedEventHandlers
{
    public class WalletUpdateOnTransferTransactionDeletionEventHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher) : IEventHandler<TransferTransactionDeletedEvent>
    {
        public async Task Handle(TransferTransactionDeletedEvent eventMessage)
        {
            if (eventMessage.Amount == 0)
            {
                return;
            }

            var sourceWalletEntity = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.SourceWalletId);
            if (sourceWalletEntity == null)
            {
                throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.SourceWalletId.ToString()}");
            }

            var targetWalletEntity = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.TargetWalletId);
            if (targetWalletEntity == null)
            {
                throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.TargetWalletId.ToString()}");
            }

            sourceWalletEntity.Balance += eventMessage.Amount;
            targetWalletEntity.Balance -= eventMessage.Amount;
            await eventPublisher.PublishAsync(new WalletUpdatedEvent
            {
                // Source
            });

            await eventPublisher.PublishAsync(new WalletUpdatedEvent
            {
                // Target
            });
        }
    }
}
