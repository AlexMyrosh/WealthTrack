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
            if(eventMessage.Amount == 0)
            {
                return;
            }

            var wallet = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.WalletId);
            if (wallet == null)
            {
                throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.WalletId.ToString()}");
            }

            var oldBalance = wallet.Balance;
            switch (eventMessage.TransactionType)
            {
                case TransactionType.Expense:
                    wallet.Balance -= eventMessage.Amount;
                    break;
                case TransactionType.Income:
                    wallet.Balance += eventMessage.Amount;
                    break;
                default:
                    throw new NotSupportedException($"Transaction type \"{eventMessage.TransactionType.ToString()}\" is not supported");
            }

            await eventPublisher.PublishAsync(new WalletUpdatedEvent
            {

            });
        }
    }
}
