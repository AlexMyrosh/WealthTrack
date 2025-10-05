using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.EventHandlers.TransactionDeletedEventHandlers
{
    public class WalletUpdateOnTransactionDeletionEventHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher) : IEventHandler<TransactionDeletedEvent>
    {
        public async Task Handle(TransactionDeletedEvent eventMessage)
        {
            if (eventMessage is null)
            {
                throw new ArgumentException(nameof(eventMessage));
            }

            if (eventMessage.Amount == 0)
            {
                return;
            }

            var wallet = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.WalletId);
            if (wallet == null)
            {
                throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.WalletId.ToString()}");
            }

            switch (eventMessage.Type)
            {
                case OperationType.Expense:
                    wallet.Balance += eventMessage.Amount;
                    break;
                case OperationType.Income:
                    wallet.Balance -= eventMessage.Amount;
                    break;
                default:
                    throw new NotSupportedException($"Transaction type \"{eventMessage.Type.ToString()}\" is not supported");
            }
        }
    }
}
