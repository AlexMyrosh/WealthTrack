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
                
                oldSourceWallet.Balance += eventMessage.Amount_New ?? eventMessage.Amount_Old;
                newSourceWallet.Balance -= eventMessage.Amount_New ?? eventMessage.Amount_Old;
            }

            // Case 3. Target wallet has changed
            if (eventMessage.TargetWalletId_New.HasValue && eventMessage.TargetWalletId_New != eventMessage.TargetWalletId_Old)
            {
                var newTargetWallet = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.TargetWalletId_New.Value);
                if (newTargetWallet == null)
                {
                    throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.TargetWalletId_New.ToString()}");
                }
                
                oldTargetWallet.Balance -= eventMessage.Amount_New ?? eventMessage.Amount_Old;
                newTargetWallet.Balance += eventMessage.Amount_New ?? eventMessage.Amount_Old;
            }
        }
    }
}
