using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.EventHandlers.TransactionUpdatedEventHandlers
{
    public class GoalUpdateOnTransactionUpdateEventHandler(IUnitOfWork unitOfWork) : IEventHandler<TransactionUpdatedEvent>
    {
        public async Task Handle(TransactionUpdatedEvent eventMessage)
        {
            if (eventMessage is null)
            {
                throw new ArgumentException(nameof(eventMessage));
            }

            // if (eventMessage.CategoryId_New is null || eventMessage.CategoryId_New == eventMessage.CategoryId_Old &&
            //     eventMessage.TransactionType_New is null || eventMessage.TransactionType_New == eventMessage.TransactionType_Old &&
            //     eventMessage.TransactionDate_New is null || eventMessage.TransactionDate_New == eventMessage.TransactionDate_Old)
            // {
            //     return;
            // }

            // In the future it will be taking goals of specific user
            var goals = await unitOfWork.GoalRepository.GetAllAsync($"{nameof(Goal.Categories)}");
            if (goals.Count == 0)
            {
                return;
            }

            foreach(var goal in goals)
            {
                // Remove old transaction data from goal
                if (IsTransactionMeetsGoal(goal, eventMessage.CategoryId_Old, eventMessage.TransactionType_Old, eventMessage.TransactionDate_Old))
                {
                    goal.ActualMoneyAmount -= eventMessage.Amount_Old;
                }

                // Add new transaction data to goal
                if (IsTransactionMeetsGoal(goal, eventMessage.CategoryId_New ?? eventMessage.CategoryId_Old,
                    eventMessage.TransactionType_New ?? eventMessage.TransactionType_Old, eventMessage.TransactionDate_New ?? eventMessage.TransactionDate_Old))
                {
                    goal.ActualMoneyAmount += eventMessage.Amount_New ?? eventMessage.Amount_Old;
                }
            }
        }

        private bool IsTransactionMeetsGoal(Goal goal, Guid? categoryId, OperationType transactionType, DateTimeOffset transactionDate)
        {
            return (!categoryId.HasValue && goal.Categories.Any() || categoryId.HasValue && goal.Categories.Any(c => c.Id == categoryId)) &&
                    goal.Type == transactionType &&
                    transactionDate >= goal.StartDate && 
                    transactionDate <= goal.EndDate;
        }
    }
}
