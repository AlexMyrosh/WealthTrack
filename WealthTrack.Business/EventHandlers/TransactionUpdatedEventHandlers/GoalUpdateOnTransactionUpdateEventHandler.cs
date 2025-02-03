using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.EventHandlers.TransactionUpdatedEventHandlers
{
    public class GoalUpdateOnTransactionUpdateEventHandler(IUnitOfWork unitOfWork) : IEventHandler<TransactionUpdatedEvent>
    {
        public async Task Handle(TransactionUpdatedEvent eventMessage)
        {
            //var oldWallet = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.OldWalletId, "Goals");
            //if (oldWallet == null)
            //{
            //    throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.OldWalletId.ToString()}");
            //}

            //if (eventMessage.NewWalletId.HasValue)
            //{
            //    foreach (var goal in oldWallet.Goals)
            //    {
            //        if (goal.Categories.Any(c => c.Id == eventMessage.OldCategoryId) &&
            //            (goal.Type == GoalType.Income && eventMessage.OldTransactionType == TransactionType.Income ||
            //             goal.Type == GoalType.Expense && eventMessage.OldTransactionType == TransactionType.Expense))
            //        {
            //            goal.ActualMoneyAmount -= eventMessage.OldAmount;
            //        }
            //    }

            //    var newWallet = await unitOfWork.WalletRepository.GetByIdAsync(eventMessage.NewWalletId.Value, "Goals");
            //    if (newWallet == null)
            //    {
            //        throw new KeyNotFoundException($"Unable to get wallet from database by id - {eventMessage.NewWalletId.ToString()}");
            //    }

            //    foreach (var goal in newWallet.Goals)
            //    {
            //        if (goal.Categories.Any(c => c.Id == (eventMessage.NewCategoryId ?? eventMessage.OldCategoryId)) &&
            //            (goal.Type == GoalType.Income && (eventMessage.NewTransactionType ?? eventMessage.OldTransactionType) == TransactionType.Income ||
            //             goal.Type == GoalType.Expense && (eventMessage.NewTransactionType ?? eventMessage.OldTransactionType) == TransactionType.Expense) &&
            //            (eventMessage.NewTransactionDate ?? eventMessage.NewTransactionDate) >= goal.StartDate &&
            //             (eventMessage.NewTransactionDate ?? eventMessage.NewTransactionDate) <= goal.EndDate)
            //        {
            //            goal.ActualMoneyAmount += eventMessage.NewAmount ?? eventMessage.OldAmount;
            //        }
            //    }
            //}
            //else
            //{
            //    foreach (var goal in oldWallet.Goals)
            //    {
            //        if (goal.Categories.Any(c => c.Id == eventMessage.OldCategoryId) &&
            //            (goal.Type == GoalType.Income && eventMessage.OldTransactionType == TransactionType.Income ||
            //             goal.Type == GoalType.Expense && eventMessage.OldTransactionType == TransactionType.Expense) &&
            //            eventMessage.OldTransactionDate >= goal.StartDate && eventMessage.OldTransactionDate <= goal.EndDate)
            //        {
            //            if (goal.Categories.Any(c => c.Id == (eventMessage.NewCategoryId ?? eventMessage.OldCategoryId)) &&
            //                (goal.Type == GoalType.Income && (eventMessage.NewTransactionType ?? eventMessage.OldTransactionType) == TransactionType.Income ||
            //                 goal.Type == GoalType.Expense && (eventMessage.NewTransactionType ?? eventMessage.OldTransactionType) == TransactionType.Expense) &&
            //                (eventMessage.NewTransactionDate ?? eventMessage.NewTransactionDate) >= goal.StartDate &&
            //                 (eventMessage.NewTransactionDate ?? eventMessage.NewTransactionDate) <= goal.EndDate)
            //            {
            //                goal.ActualMoneyAmount -= eventMessage.OldAmount;
            //                goal.ActualMoneyAmount += eventMessage.NewAmount ?? eventMessage.OldAmount;
            //            }
            //            else
            //            {
            //                goal.ActualMoneyAmount -= eventMessage.OldAmount;
            //            }
            //        }
            //    }
            //}
        }
    }
}
