using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.EventHandlers.WalletDeletedEventHandlers
{
    public class GoalUpdateOnWalletDeletionEventHandler(IUnitOfWork unitOfWork) : IEventHandler<WalletDeletedEvent>
    {
        public async Task Handle(WalletDeletedEvent eventMessage)
        {
            if (eventMessage is null)
            {
                throw new ArgumentException(nameof(eventMessage));
            }

            // In the future it will be taking goals of specific user
            var goals = await unitOfWork.GoalRepository.GetAllAsync($"{nameof(Goal.Categories)}");
            if (goals.Count == 0)
            {
                return;
            }
            
            var transactions = await unitOfWork.TransactionRepository.GetAllAsync();
            var walletTransactions = transactions.Where(t => t.WalletId == eventMessage.WalletId).ToList();
            foreach (var goal in goals)
            {
                var applicableTransactions = walletTransactions.Where(transaction => IsTransactionMeetsGoal(goal, transaction)).ToList();
                goal.ActualMoneyAmount -= applicableTransactions.Sum(transaction => transaction.Amount);
            }
        }

        private bool IsTransactionMeetsGoal(Goal goal, Transaction transaction)
        {
            return goal.Categories.Any(c => c.Id == transaction.CategoryId) &&
                   goal.Type == transaction.Type &&
                   transaction.TransactionDate >= goal.StartDate && 
                   transaction.TransactionDate <= goal.EndDate;
        }
    }
}
