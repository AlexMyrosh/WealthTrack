using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.EventHandlers.CategoryDeletedEventHandlers
{
    public class GoalUpdateOnCategoryDeletionEventHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher) : IEventHandler<CategoryDeletedEvent>
    {
        public async Task Handle(CategoryDeletedEvent eventMessage)
        {
            if (eventMessage is null)
            {
                throw new ArgumentException(nameof(eventMessage));
            }

            var categoryEntity = await unitOfWork.CategoryRepository.GetByIdAsync(eventMessage.CategoryId, $"{nameof(Category.Transactions)},{nameof(Category.ChildCategories)}");
            if(categoryEntity == null)
            {
                throw new KeyNotFoundException($"Unable to get category from database by id - {eventMessage.CategoryId.ToString()}");
            }

            foreach (var childCategory in categoryEntity.ChildCategories)
            {
                await eventPublisher.PublishAsync(new CategoryDeletedEvent
                {
                    CategoryId = childCategory.Id
                });
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
                    goal.Type == transaction.Type &&
                    transaction.TransactionDate >= goal.StartDate && 
                    transaction.TransactionDate <= goal.EndDate;
        }
    }
}
