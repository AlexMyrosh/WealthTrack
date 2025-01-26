using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.EventHandlers.TransactionDeletedEventHandlers
{
    public class WalletBalanceUpdateOnTransactionDeletionEventHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher) : IEventHandler<TransactionDeletedEvent>
    {
        public async Task Handle(TransactionDeletedEvent eventMessage)
        {
            var wallet = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.WalletId);
            if (wallet == null)
            {
                throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.WalletId.ToString()}");
            }

            var oldBalance = wallet.Balance;
            switch (eventMessage.TransactionType)
            {
                case TransactionType.Expense:
                    wallet.Balance += eventMessage.Amount;
                    break;
                case TransactionType.Income:
                    wallet.Balance -= eventMessage.Amount;
                    break;
                default:
                    throw new NotSupportedException($"Transaction type \"{eventMessage.TransactionType.ToString()}\" is not supported");
            }

            WalletBalanceChangedEvent balancedChangedEvent = new(eventMessage.WalletId, wallet.BudgetId, null, oldBalance, wallet.Balance, wallet.IsPartOfGeneralBalance, null);
            await eventPublisher.PublishAsync(balancedChangedEvent);
        }
    }
}
