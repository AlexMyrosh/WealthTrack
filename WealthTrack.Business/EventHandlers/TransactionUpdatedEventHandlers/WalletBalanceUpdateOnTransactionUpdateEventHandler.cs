using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.EventHandlers.TransactionUpdatedEventHandlers
{
    public class WalletBalanceUpdateOnTransactionUpdateEventHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher) : IEventHandler<TransactionUpdatedEvent>
    {
        public async Task Handle(TransactionUpdatedEvent eventMessage)
        {
            if (eventMessage.NewCategoryId == null && eventMessage.NewTransactionType == null && eventMessage.NewWalletId == null && eventMessage.NewAmount == null ||
                eventMessage.OldCategoryId == eventMessage.NewCategoryId &&
                 eventMessage.OldTransactionType == eventMessage.NewTransactionType &&
                 eventMessage.OldWalletId == eventMessage.NewWalletId &&
                eventMessage.OldAmount == eventMessage.NewAmount)
            {
                return;
            }

            Wallet wallet;
            var oldWallet = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.OldWalletId);
            if (oldWallet == null)
            {
                throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.OldWalletId.ToString()}");
            }

            if (eventMessage.NewWalletId.HasValue)
            {
                var newWallet = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.NewWalletId.Value);
                if (newWallet == null)
                {
                    throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.NewWalletId.ToString()}");
                }

                switch (eventMessage.OldTransactionType)
                {
                    case TransactionType.Income:
                        oldWallet.Balance -= eventMessage.OldAmount;
                        break;
                    case TransactionType.Expense:
                        oldWallet.Balance += eventMessage.OldAmount;
                        break;
                    default:
                        throw new NotSupportedException($"Transaction type \"{eventMessage.OldTransactionType.ToString()}\" is not supported");
                }

                wallet = newWallet;
            }
            else
            {
                wallet = oldWallet;
            }

            if (eventMessage.NewTransactionType.HasValue && eventMessage.NewTransactionType.Value != eventMessage.OldTransactionType)
            {
                switch (eventMessage.NewTransactionType)
                {
                    case TransactionType.Income:
                        wallet.Balance += eventMessage.OldAmount * 2;
                        break;
                    case TransactionType.Expense:
                        wallet.Balance -= eventMessage.OldAmount * 2;
                        break;
                    default:
                        throw new NotSupportedException($"Transaction type \"{eventMessage.OldTransactionType.ToString()}\" is not supported");
                }
            }

            if (eventMessage.NewAmount.HasValue)
            {
                var type = eventMessage.NewTransactionType ?? eventMessage.OldTransactionType;
                switch (type)
                {
                    case TransactionType.Income:
                        wallet.Balance += decimal.Abs(eventMessage.OldAmount - eventMessage.NewAmount.Value);
                        break;
                    case TransactionType.Expense:
                        wallet.Balance -= decimal.Abs(eventMessage.OldAmount - eventMessage.NewAmount.Value);
                        break;
                    default:
                        throw new NotSupportedException($"Transaction type \"{eventMessage.OldTransactionType.ToString()}\" is not supported");
                }
            }

            if (eventMessage.NewWalletId.HasValue)
            {
                var newWallet = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.NewWalletId.Value);
                if (newWallet == null)
                {
                    throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.NewWalletId.ToString()}");
                }

                await eventPublisher.PublishAsync(new WalletBalanceChangedEvent(eventMessage.NewWalletId.Value, oldWallet.BudgetId, null, oldWallet.Balance, newWallet.Balance, oldWallet.IsPartOfGeneralBalance, null));
            }

            await eventPublisher.PublishAsync(new WalletBalanceChangedEvent(eventMessage.OldWalletId, oldWallet.BudgetId, null, oldWallet.Balance, null, oldWallet.IsPartOfGeneralBalance, null));
        }
    }
}
