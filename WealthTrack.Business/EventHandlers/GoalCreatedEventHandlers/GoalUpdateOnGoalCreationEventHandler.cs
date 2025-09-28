using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.EventHandlers.GoalCreatedEventHandlers
{
    public class GoalUpdateOnGoalCreationEventHandler(IUnitOfWork unitOfWork) : IEventHandler<GoalCreatedEvent>
    {
        public async Task Handle(GoalCreatedEvent eventMessage)
        {
            if (eventMessage is null)
            {
                throw new ArgumentException(nameof(eventMessage));
            }

            var transactions = await unitOfWork.TransactionRepository.GetAllAsync();
            var applicableTransactions = transactions.Where(t => t.CategoryId.HasValue && eventMessage.CategoryIds.Contains(t.CategoryId.Value) &&
                                                                 eventMessage.StartDate <= t.TransactionDate &&
                                                                 eventMessage.EndDate >= t.TransactionDate &&
                                                                 eventMessage.Type == t.Type).ToList();

            foreach (var transaction in applicableTransactions)
            {
                eventMessage.GoalModel.ActualMoneyAmount += transaction.Amount;
            }
        }
    }
}
