using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Business.EventHandlers.TransferTransactionUpdatedEventHandlers
{
    public class WalletUpdateOnTransferTransactionUpdateEventHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher) : IEventHandler<TransferTransactionUpdatedEvent>
    {
        public async Task Handle(TransferTransactionUpdatedEvent eventMessage)
        {

        }
    }
}
