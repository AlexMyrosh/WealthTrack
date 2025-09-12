using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.EventHandlers.CategoryDeletedEventHandlers
{
    public class GoalUpdateOnCategoryDeletionEventHandler(IUnitOfWork unitOfWork) : IEventHandler<CategoryDeletedEvent>
    {
        public async Task Handle(CategoryDeletedEvent eventMessage)
        {
            if (eventMessage is null)
            {
                throw new ArgumentException(nameof(eventMessage));
            }

            var categoryEntity = await unitOfWork.CategoryRepository.GetByIdAsync(eventMessage.CategoryId, "Transactions");
            if(categoryEntity == null)
            {
                throw new KeyNotFoundException($"Unable to get category from database by id - {eventMessage.CategoryId.ToString()}");
            }

            var goals = await unitOfWork.GoalRepository.GetAllAsync($"{nameof(Goal.Categories)}");
            foreach (var goal in goals)
            {
                if(goal.Categories.Any(g => g.Id == eventMessage.CategoryId))
                {
                    foreach (var transaction in categoryEntity.Transactions)
                    {
                        if(isTransactionMeetsGoal(goal, transaction))
                        {
                            goal.ActualMoneyAmount -= transaction.Amount;
                        }
                    }
                }
            }
        }

        private bool isTransactionMeetsGoal(Goal goal, Transaction transaction)
        {
            return goal.Categories.Any(c => c.Id == transaction.CategoryId) &&
                    (goal.Type == GoalType.Income && transaction.Type == TransactionType.Income ||
                     goal.Type == GoalType.Expense && transaction.Type == TransactionType.Expense) &&
                    transaction.TransactionDate >= goal.StartDate && transaction.TransactionDate <= goal.EndDate;
        }
    }
}
