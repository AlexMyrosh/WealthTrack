using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.EventHandlers.TransactionUpdatedEventHandlers
{
    public class WalletUpdateOnTransactionUpdateEventHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher) : IEventHandler<TransactionUpdatedEvent>
    {
        public async Task Handle(TransactionUpdatedEvent eventMessage)
        {
            if (eventMessage is null)
            {
                throw new ArgumentException(nameof(eventMessage));
            }
            
            Wallet wallet;
            var oldWallet = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.WalletId_Old.Value);
            if (oldWallet == null)
            {
                throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.WalletId_Old.ToString()}");
            }

            // Case 1. Wallet was changed
            if (eventMessage.WalletId_New.HasValue && eventMessage.WalletId_New != eventMessage.WalletId_Old)
            {
                var newWallet = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.WalletId_New.Value);
                if (newWallet == null)
                {
                    throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.WalletId_New.ToString()}");
                }
                
                switch (eventMessage.TransactionType_Old)
                {
                    case TransactionType.Income:
                        oldWallet.Balance -= eventMessage.Amount_Old;
                        newWallet.Balance += eventMessage.Amount_Old;
                        break;
                    case TransactionType.Expense:
                        oldWallet.Balance += eventMessage.Amount_Old;
                        newWallet.Balance -= eventMessage.Amount_Old;
                        break;
                    default:
                        throw new NotSupportedException($"Transaction type \"{eventMessage.TransactionType_Old.ToString()}\" is not supported");
                }

                wallet = newWallet;
            }
            else
            {
                wallet = oldWallet;
            }

            // Case 2. Transaction type was changed
            if (eventMessage.TransactionType_New.HasValue && eventMessage.TransactionType_New.Value != eventMessage.TransactionType_Old)
            {
                switch (eventMessage.TransactionType_New)
                {
                    // Expense -> Income
                    case TransactionType.Income:
                        wallet.Balance += eventMessage.Amount_Old * 2;
                        break;
                    // Income -> Expense
                    case TransactionType.Expense:
                        wallet.Balance -= eventMessage.Amount_Old * 2;
                        break;
                    default:
                        throw new NotSupportedException($"Transaction type \"{eventMessage.TransactionType_New.ToString()}\" is not supported");
                }
            }

            // Case 3. Amount was changed
            if (eventMessage.Amount_New.HasValue && eventMessage.Amount_New != eventMessage.Amount_Old)
            {
                var transactionType = eventMessage.TransactionType_New ?? eventMessage.TransactionType_Old;
                switch (transactionType)
                {
                    case TransactionType.Income:
                        wallet.Balance -= eventMessage.Amount_Old - eventMessage.Amount_New.Value;
                        break;
                    case TransactionType.Expense:
                        wallet.Balance += eventMessage.Amount_Old - eventMessage.Amount_New.Value;
                        break;
                    default:
                        throw new NotSupportedException($"Transaction type \"{transactionType.ToString()}\" is not supported");
                }
            }
        }
    }
}
