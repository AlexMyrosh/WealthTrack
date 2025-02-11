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

            if (eventMessage.TransactionType_New is null || eventMessage.TransactionType_New == eventMessage.TransactionType_Old &&
                eventMessage.WalletId_New is null || eventMessage.WalletId_New == eventMessage.WalletId_Old &&
                eventMessage.Amount_New is null || eventMessage.Amount_New == eventMessage.Amount_Old)
            {
                return;
            }

            Wallet wallet;
            var oldWallet = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.WalletId_Old);
            if (oldWallet == null)
            {
                throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.WalletId_Old.ToString()}");
            }

            // Case 1. Wallet was changed
            decimal walletBalanceBeforeUpdate = 0;
            if (eventMessage.WalletId_New.HasValue && eventMessage.WalletId_New != eventMessage.WalletId_Old)
            {
                var newWallet = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.WalletId_New.Value);
                if (newWallet == null)
                {
                    throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.WalletId_New.ToString()}");
                }

                walletBalanceBeforeUpdate = newWallet.Balance;
                switch (eventMessage.TransactionType_Old)
                {
                    case TransactionType.Income:
                        oldWallet.Balance -= eventMessage.Amount_Old;
                        break;
                    case TransactionType.Expense:
                        oldWallet.Balance += eventMessage.Amount_Old;
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
                if (eventMessage.TransactionType_New.Value == TransactionType.Transfer || eventMessage.TransactionType_Old == TransactionType.Transfer)
                {
                    throw new ArgumentException("Transfer transaction is not supported here");
                }

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
                if (transactionType == TransactionType.Transfer)
                {
                    throw new ArgumentException("Transfer transaction is not supported here");
                }

                var difference = eventMessage.Amount_New.Value - eventMessage.Amount_Old;
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

            // Send event notification that new wallet was updated
            if (eventMessage.WalletId_New.HasValue)
            {
                await eventPublisher.PublishAsync(new WalletUpdatedEvent
                {
                    WalletId = wallet.Id,
                    BudgetId_Old = wallet.BudgetId,
                    Balance_Old = walletBalanceBeforeUpdate,
                    Balance_New = wallet.Balance,
                    IsPartOfGeneralBalance_Old = wallet.IsPartOfGeneralBalance
                });
            }

            // Send event notification that old wallet was updated
            await eventPublisher.PublishAsync(new WalletUpdatedEvent
            {
                WalletId = eventMessage.WalletId_Old,
                BudgetId_Old = oldWallet.BudgetId,
                Balance_Old = oldWallet.Balance,
                IsPartOfGeneralBalance_Old = oldWallet.IsPartOfGeneralBalance
            });
        }
    }
}
