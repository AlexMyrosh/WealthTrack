using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Business.EventHandlers.WalletDeletedEventHandlers
{
    public class BudgetUpdateOnWalletDeletionEventHandler(IUnitOfWork unitOfWork) : IEventHandler<WalletDeletedEvent>
    {
        public async Task Handle(WalletDeletedEvent eventMessage)
        {
            if (!eventMessage.IsPartOfGeneralBalance || eventMessage.Balance == 0)
            {
                return;
            }

            var budgedEntity = await unitOfWork.BudgetRepository.GetByIdAsync(eventMessage.BudgetId);
            if (budgedEntity is null)
            {
                throw new KeyNotFoundException($"Unable to get budget from database by id - {eventMessage.BudgetId.ToString()}");
            }

            budgedEntity.OverallBalance -= eventMessage.Balance;
        }
    }
}
