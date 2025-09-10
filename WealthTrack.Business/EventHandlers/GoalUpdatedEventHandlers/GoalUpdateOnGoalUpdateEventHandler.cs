using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.EventHandlers.GoalUpdatedEventHandlers
{
    public class GoalUpdateOnGoalUpdateEventHandler(IUnitOfWork unitOfWork) : IEventHandler<GoalUpdatedEvent>
    {
        public async Task Handle(GoalUpdatedEvent eventMessage)
        {
            if (eventMessage is null)
            {
                throw new ArgumentException(nameof(eventMessage));
            }
            
            // if (eventMessage.Type == eventMessage.GoalModel.Type && eventMessage.StartDate == eventMessage.GoalModel.StartDate &&
            //     eventMessage.EndDate == eventMessage.GoalModel.EndDate &&
            //     eventMessage.GoalModel.Categories.TrueForAll(x => eventMessage.CategoryIds.Contains(x.Id)))
            // {
            //     return;
            // }
            
            var transactions = await unitOfWork.TransactionRepository.GetAllAsync();
            var applicableTransactions = transactions.Where(t => t.CategoryId.HasValue && eventMessage.CategoryIds.Contains(t.CategoryId.Value) &&
                                                                 eventMessage.StartDate <= t.TransactionDate &&
                                                                 eventMessage.EndDate >= t.TransactionDate &&
                                                                 (eventMessage.Type == GoalType.Income && t.Type == TransactionType.Income ||
                                                                 eventMessage.Type == GoalType.Expense && t.Type == TransactionType.Expense)).ToList();

            eventMessage.GoalModel.ActualMoneyAmount = 0;
            foreach (var transaction in applicableTransactions)
            {
                eventMessage.GoalModel.ActualMoneyAmount += transaction.Amount;
            }
        }
    }
}
