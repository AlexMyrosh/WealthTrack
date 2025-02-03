using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Business.EventHandlers.WalletCreatedEventHandlers
{
    public class BudgetUpdateOnWalletCreationEventHandler(IUnitOfWork unitOfWork) : IEventHandler<WalletCreatedEvent>
    {
        public async Task Handle(WalletCreatedEvent eventMessage)
        {

        }
    }
}
