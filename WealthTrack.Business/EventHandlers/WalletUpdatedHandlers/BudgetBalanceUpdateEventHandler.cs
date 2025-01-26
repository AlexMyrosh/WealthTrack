using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Business.EventHandlers.WalletEventHandlers
{
    public class BudgetBalanceUpdateEventHandler(IUnitOfWork unitOfWork) : IEventHandler<WalletBalanceChangedEvent>
    {
        public async Task Handle(WalletBalanceChangedEvent eventMessage)
        {
            // TODO: need recalculate budget balance by its new currency
            if ((eventMessage.IsPartOfGeneralBalanceNewValue == null && eventMessage.NewBalance == null && eventMessage.NewBudgetId == null) ||
                (eventMessage.IsPartOfGeneralBalanceNewValue == eventMessage.IsPartOfGeneralBalanceOldValue &&
                 eventMessage.OldBalance == eventMessage.NewBalance &&
                 eventMessage.OldBudgetId == eventMessage.NewBudgetId))
            {
                return;
            }

            var oldBudget = await unitOfWork.BudgetRepository.GetByIdAsync(eventMessage.OldBudgetId);
            if (oldBudget == null)
            {
                throw new KeyNotFoundException($"Unable to get budget from database by id - {eventMessage.OldBudgetId.ToString()}");
            }

            Budget budget;
            if (eventMessage.NewBudgetId.HasValue)
            {
                var newBudget = await unitOfWork.BudgetRepository.GetByIdAsync(eventMessage.NewBudgetId.Value);
                if (newBudget == null)
                {
                    throw new KeyNotFoundException($"Unable to get budget from database by id - {eventMessage.NewBudgetId.ToString()}");
                }

                var wallet = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.WalletId);
                if (wallet == null)
                {
                    throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.WalletId.ToString()}");
                }

                oldBudget.OverallBalance -= eventMessage.IsPartOfGeneralBalanceOldValue ? wallet.Balance : 0;
                if (eventMessage.IsPartOfGeneralBalanceNewValue.HasValue)
                {
                    newBudget.OverallBalance += eventMessage.IsPartOfGeneralBalanceNewValue.Value ? wallet.Balance : 0;
                }
                else
                {
                    newBudget.OverallBalance += eventMessage.IsPartOfGeneralBalanceOldValue ? wallet.Balance : 0;
                }

                budget = newBudget;
            }
            else
            {
                budget = oldBudget;
            }

            if (eventMessage.IsPartOfGeneralBalanceNewValue.HasValue && eventMessage.IsPartOfGeneralBalanceNewValue.Value)
            {
                budget.OverallBalance += eventMessage.OldBalance;
            }
            else if (eventMessage.IsPartOfGeneralBalanceNewValue.HasValue && !eventMessage.IsPartOfGeneralBalanceNewValue.Value)
            {
                budget.OverallBalance -= eventMessage.OldBalance;
            }

            if (eventMessage.NewBalance.HasValue)
            {
                if (eventMessage.NewBalance.Value > eventMessage.OldBalance)
                {
                    budget.OverallBalance += decimal.Abs(eventMessage.NewBalance.Value - eventMessage.OldBalance);
                }
                else if (eventMessage.NewBalance.Value < eventMessage.OldBalance)
                {
                    budget.OverallBalance -= decimal.Abs(eventMessage.NewBalance.Value - eventMessage.OldBalance);
                }
            }
        }
    }
}
