using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Business.EventHandlers.CategoryDeletedEventHandlers
{
    public class GoalUpdateOnCategoryDeletionEventHandler(IUnitOfWork unitOfWork) : IEventHandler<CategoryDeletedEvent>
    {
        public async Task Handle(CategoryDeletedEvent eventMessage)
        {

        }
    }
}
