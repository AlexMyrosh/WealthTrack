using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Business.EventHandlers.WalletCreatedEventHandlers
{
    public class BudgetUpdateOnWalletCreationEventHandler(IUnitOfWork unitOfWork) : IEventHandler<WalletCreatedEvent>
    {
        public async Task Handle(WalletCreatedEvent eventMessage)
        {
            if (eventMessage is null)
            {
                throw new ArgumentException(nameof(eventMessage));
            }

            if (!eventMessage.IsPartOfGeneralBalance || eventMessage.Balance == 0)
            {
                return;
            }

            var budgedEntity = await unitOfWork.BudgetRepository.GetByIdAsync(eventMessage.BudgetId);
            if(budgedEntity is null)
            {
                throw new KeyNotFoundException($"Unable to get budget from database by id - {eventMessage.BudgetId.ToString()}");
            }

            budgedEntity.OverallBalance += eventMessage.Balance;
        }
    }
}
