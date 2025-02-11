using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.EventHandlers.TransactionDeletedEventHandlers
{
    public class GoalUpdateOnTransactionDeletionEventHandler(IUnitOfWork unitOfWork) : IEventHandler<TransactionDeletedEvent>
    {
        public async Task Handle(TransactionDeletedEvent eventMessage)
        {
            if (eventMessage is null)
            {
                throw new ArgumentException(nameof(eventMessage));
            }

            // In future it will be taking goals of specific user
            var goals = await unitOfWork.GoalRepository.GetAllAsync();
            if (goals.Count == 0)
            {
                return;
            }

            foreach (var goal in goals)
            {
                if (isTransactionMeetsGoal(goal, eventMessage))
                {
                    goal.ActualMoneyAmount += eventMessage.Amount;
                }
            }
        }

        private bool isTransactionMeetsGoal(Goal goal, TransactionDeletedEvent transaction)
        {
            return goal.Categories.Any(c => c.Id == transaction.CategoryId) &&
                    (goal.Type == GoalType.Income && transaction.Type == TransactionType.Income ||
                     goal.Type == GoalType.Expense && transaction.Type == TransactionType.Expense) &&
                    transaction.TransactionDate >= goal.StartDate && transaction.TransactionDate <= goal.EndDate;
        }
    }
}
