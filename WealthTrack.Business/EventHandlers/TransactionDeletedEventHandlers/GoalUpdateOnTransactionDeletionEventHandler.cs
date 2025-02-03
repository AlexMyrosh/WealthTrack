using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.EventHandlers.TransactionDeletedEventHandlers
{
    public class GoalUpdateOnTransactionDeletionEventHandler(IUnitOfWork unitOfWork) : IEventHandler<TransactionDeletedEvent>
    {
        public async Task Handle(TransactionDeletedEvent eventMessage)
        {
            //var wallet = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.WalletId, "Goals");
            //if (wallet == null)
            //{
            //    throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.WalletId.ToString()}");
            //}

            //foreach (var goal in wallet.Goals)
            //{
            //    if (goal.Categories.Any(c => c.Id == eventMessage.CategoryId) &&
            //        (goal.Type == GoalType.Income && eventMessage.TransactionType == TransactionType.Income ||
            //         goal.Type == GoalType.Expense && eventMessage.TransactionType == TransactionType.Expense) &&
            //        eventMessage.TransactionDate >= goal.StartDate && eventMessage.TransactionDate <= goal.EndDate)
            //    {
            //        goal.ActualMoneyAmount -= eventMessage.Amount;
            //    }
            //}
        }
    }
}
