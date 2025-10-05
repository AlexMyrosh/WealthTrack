using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.EventHandlers.TransactionCreatedEventHandlers
{
    public class WalletUpdateOnTransactionCreationEventHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher) : IEventHandler<TransactionCreatedEvent>
    {
        public async Task Handle(TransactionCreatedEvent eventMessage)
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
                throw new ArgumentException($"Unable to get wallet from database by id - {eventMessage.WalletId.ToString()}");
            }
            
            switch (eventMessage.Type)
            {
                case OperationType.Expense:
                    wallet.Balance -= eventMessage.Amount;
                    break;
                case OperationType.Income:
                    wallet.Balance += eventMessage.Amount;
                    break;
                default:
                    throw new NotSupportedException($"Transaction type \"{eventMessage.Type.ToString()}\" is not supported");
            }

        }
    }
}
