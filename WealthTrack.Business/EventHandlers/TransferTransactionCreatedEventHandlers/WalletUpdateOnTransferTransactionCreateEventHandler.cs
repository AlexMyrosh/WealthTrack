using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Business.EventHandlers.TransferTransactionCreatedEventHandlers
{
    public class WalletUpdateOnTransferTransactionCreateEventHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher) : IEventHandler<TransferTransactionCreatedEvent>
    {
        public async Task Handle(TransferTransactionCreatedEvent eventMessage)
        {

        }
    }
}
