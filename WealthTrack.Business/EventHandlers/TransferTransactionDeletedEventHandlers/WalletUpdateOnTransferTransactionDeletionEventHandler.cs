using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Business.EventHandlers.TransferTransactionDeletedEventHandlers
{
    public class WalletUpdateOnTransferTransactionDeletionEventHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher) : IEventHandler<TransferTransactionDeletedEvent>
    {
        public async Task Handle(TransferTransactionDeletedEvent eventMessage)
        {

        }
    }
}
