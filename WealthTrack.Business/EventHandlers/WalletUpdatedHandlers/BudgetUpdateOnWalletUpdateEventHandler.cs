using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Business.EventHandlers.WalletUpdatedHandlers
{
    public class BudgetUpdateOnWalletUpdateEventHandler(IUnitOfWork unitOfWork) : IEventHandler<WalletUpdatedEvent>
    {
        public async Task Handle(WalletUpdatedEvent eventMessage)
        {
            if (eventMessage is null)
            {
                throw new ArgumentException(nameof(eventMessage));
            }

            if (eventMessage.IsPartOfGeneralBalance_New is null || eventMessage.IsPartOfGeneralBalance_New == eventMessage.IsPartOfGeneralBalance_Old &&
                eventMessage.BudgetId_New is null || eventMessage.BudgetId_New == eventMessage.BudgetId_Old &&
                eventMessage.Balance_New is null || eventMessage.Balance_New == eventMessage.Balance_Old)
            {
                return;
            }

            var oldBudget = await unitOfWork.BudgetRepository.GetByIdAsync(eventMessage.BudgetId_Old);
            if (oldBudget == null)
            {
                throw new KeyNotFoundException($"Unable to get budget from database by id - {eventMessage.BudgetId_Old.ToString()}");
            }

            // Case 1. Budget was changed
            Budget budget;
            if (eventMessage.BudgetId_New != null && eventMessage.BudgetId_New != eventMessage.BudgetId_Old)
            {
                var newBudget = await unitOfWork.BudgetRepository.GetByIdAsync(eventMessage.BudgetId_New.Value);
                if (newBudget == null)
                {
                    throw new KeyNotFoundException($"Unable to get budget from database by id - {eventMessage.BudgetId_New.ToString()}");
                }

                var wallet = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.WalletId);
                if (wallet == null)
                {
                    throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.WalletId.ToString()}");
                }

                oldBudget.OverallBalance -= eventMessage.IsPartOfGeneralBalance_Old ? wallet.Balance : 0;
                if (eventMessage.IsPartOfGeneralBalance_New.HasValue)
                {
                    newBudget.OverallBalance += eventMessage.IsPartOfGeneralBalance_New.Value ? wallet.Balance : 0;
                }
                else
                {
                    newBudget.OverallBalance += eventMessage.IsPartOfGeneralBalance_Old ? wallet.Balance : 0;
                }

                budget = newBudget;
            }
            else
            {
                budget = oldBudget;
            }

            // Case 2. IsPartOfGeneralBalance was changed
            if (eventMessage.IsPartOfGeneralBalance_New.HasValue && eventMessage.IsPartOfGeneralBalance_New != eventMessage.IsPartOfGeneralBalance_Old)
            {
                budget.OverallBalance += eventMessage.IsPartOfGeneralBalance_New.Value ? eventMessage.Balance_Old : -eventMessage.Balance_Old;
            }

            // Case 3. Balance was changed
            if (eventMessage.Balance_New.HasValue && eventMessage.Balance_New != eventMessage.Balance_Old)
            {
                var balanceDifference = eventMessage.Balance_New.Value - eventMessage.Balance_Old;
                budget.OverallBalance += balanceDifference;
            }
        }
    }
}
