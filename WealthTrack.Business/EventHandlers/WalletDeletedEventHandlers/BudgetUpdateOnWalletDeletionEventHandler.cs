using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Business.EventHandlers.WalletDeletedEventHandlers
{
    public class BudgetUpdateOnWalletDeletionEventHandler(IUnitOfWork unitOfWork) : IEventHandler<WalletDeletedEvent>
    {
        public async Task Handle(WalletDeletedEvent eventMessage)
        {

        }
    }
}
