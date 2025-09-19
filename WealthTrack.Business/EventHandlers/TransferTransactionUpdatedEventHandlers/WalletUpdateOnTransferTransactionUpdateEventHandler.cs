using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Business.EventHandlers.TransferTransactionUpdatedEventHandlers
{
    public class WalletUpdateOnTransferTransactionUpdateEventHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher) : IEventHandler<TransferTransactionUpdatedEvent>
    {
        public async Task Handle(TransferTransactionUpdatedEvent eventMessage)
        {
            if (eventMessage is null)
            {
                throw new ArgumentException(nameof(eventMessage));
            }

            // if (eventMessage.Amount_New is null || eventMessage.Amount_New == eventMessage.Amount_Old &&
            //     eventMessage.SourceWalletId_New is null || eventMessage.SourceWalletId_New == eventMessage.SourceWalletId_Old &&
            //     eventMessage.TargetWalletId_New is null || eventMessage.TargetWalletId_New == eventMessage.TargetWalletId_Old)
            // {
            //     return;
            // }

            var oldSourceWallet = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.SourceWalletId_Old);
            if (oldSourceWallet == null)
            {
                throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.SourceWalletId_Old.ToString()}");
            }

            var oldTargetWallet = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.TargetWalletId_Old);
            if (oldTargetWallet == null)
            {
                throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.TargetWalletId_Old.ToString()}");
            }

            // Case 1. Amount has changed
            if(eventMessage.Amount_New.HasValue && eventMessage.Amount_New != eventMessage.Amount_Old)
            {
                oldSourceWallet.Balance -= eventMessage.Amount_New.Value - eventMessage.Amount_Old;
                oldTargetWallet.Balance += eventMessage.Amount_New.Value - eventMessage.Amount_Old;
            }

            // Case 2. Source wallet has changed
            if (eventMessage.SourceWalletId_New.HasValue && eventMessage.SourceWalletId_New != eventMessage.SourceWalletId_Old)
            {
                var newSourceWallet = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.SourceWalletId_New.Value);
                if (newSourceWallet == null)
                {
                    throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.SourceWalletId_New.ToString()}");
                }

                var balanceBeforeUpdate = newSourceWallet.Balance;
                oldSourceWallet.Balance += eventMessage.Amount_New ?? eventMessage.Amount_Old;
                newSourceWallet.Balance -= eventMessage.Amount_New ?? eventMessage.Amount_Old;
                await eventPublisher.PublishAsync(new WalletUpdatedEvent
                {
                    WalletId = eventMessage.SourceWalletId_New.Value,
                    BudgetId_Old = newSourceWallet.BudgetId,
                    Balance_Old = balanceBeforeUpdate,
                    IsPartOfGeneralBalance_Old = newSourceWallet.IsPartOfGeneralBalance
                });
            }

            // Case 3. Target wallet has changed
            if (eventMessage.TargetWalletId_New.HasValue && eventMessage.TargetWalletId_New != eventMessage.TargetWalletId_Old)
            {
                var newTargetWallet = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.TargetWalletId_New.Value);
                if (newTargetWallet == null)
                {
                    throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.TargetWalletId_New.ToString()}");
                }

                var balanceBeforeUpdate = newTargetWallet.Balance;
                oldTargetWallet.Balance -= eventMessage.Amount_New ?? eventMessage.Amount_Old;
                newTargetWallet.Balance += eventMessage.Amount_New ?? eventMessage.Amount_Old;
                await eventPublisher.PublishAsync(new WalletUpdatedEvent
                {
                    WalletId = eventMessage.TargetWalletId_New.Value,
                    BudgetId_Old = newTargetWallet.BudgetId,
                    Balance_Old = balanceBeforeUpdate,
                    IsPartOfGeneralBalance_Old = newTargetWallet.IsPartOfGeneralBalance
                });
            }
        }
    }
}
