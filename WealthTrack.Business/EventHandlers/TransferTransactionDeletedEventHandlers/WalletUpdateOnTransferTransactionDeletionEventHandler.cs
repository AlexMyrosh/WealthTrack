using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Business.EventHandlers.TransferTransactionDeletedEventHandlers
{
    public class WalletUpdateOnTransferTransactionDeletionEventHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher) : IEventHandler<TransferTransactionDeletedEvent>
    {
        public async Task Handle(TransferTransactionDeletedEvent eventMessage)
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
            if (sourceWalletEntity == null)
            {
                throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.SourceWalletId.ToString()}");
            }

            var targetWalletEntity = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.TargetWalletId);
            if (targetWalletEntity == null)
            {
                throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.TargetWalletId.ToString()}");
            }

            var sourceWalletBalanceBeforeUpdate = sourceWalletEntity.Balance;
            var targetWalletBalanceBeforeUpdate = targetWalletEntity.Balance;
            sourceWalletEntity.Balance += eventMessage.Amount;
            targetWalletEntity.Balance -= eventMessage.Amount;
            await eventPublisher.PublishAsync(new WalletUpdatedEvent
            {
                WalletId = sourceWalletEntity.Id,
                BudgetId_Old = sourceWalletEntity.BudgetId,
                Balance_Old = sourceWalletBalanceBeforeUpdate,
                IsPartOfGeneralBalance_Old = sourceWalletEntity.IsPartOfGeneralBalance
            });

            await eventPublisher.PublishAsync(new WalletUpdatedEvent
            {
                WalletId = targetWalletEntity.Id,
                BudgetId_Old = targetWalletEntity.BudgetId,
                Balance_Old = targetWalletBalanceBeforeUpdate,
                IsPartOfGeneralBalance_Old = targetWalletEntity.IsPartOfGeneralBalance
            });
        }
    }
}
