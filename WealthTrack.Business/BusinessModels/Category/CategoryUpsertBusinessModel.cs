using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.BusinessModels.Category
{
    public class CategoryUpsertBusinessModel
    {
        public string? Name { get; set; }

        public string? IconName { get; set; }

        public OperationType? Type { get; set; }

        public Guid? ParentCategoryId { get; set; }
    }
}