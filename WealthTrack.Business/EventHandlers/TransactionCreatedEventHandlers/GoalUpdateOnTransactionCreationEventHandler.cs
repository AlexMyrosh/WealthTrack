using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.EventHandlers.TransactionCreatedEventHandlers
{
    public class GoalUpdateOnTransactionCreationEventHandler(IUnitOfWork unitOfWork) : IEventHandler<TransactionCreatedEvent>
    {
        public async Task Handle(TransactionCreatedEvent eventMessage)
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

        private bool isTransactionMeetsGoal(Goal goal, TransactionCreatedEvent transaction)
        {
            return goal.Categories.Any(c => c.Id == transaction.CategoryId) &&
                    (goal.Type == GoalType.Income && transaction.Type == TransactionType.Income ||
                     goal.Type == GoalType.Expense && transaction.Type == TransactionType.Expense) &&
                    transaction.TransactionDate >= goal.StartDate && transaction.TransactionDate <= goal.EndDate;
        }
    }
}
