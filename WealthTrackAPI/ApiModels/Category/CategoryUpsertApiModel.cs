using WealthTrack.Shared.Enums;

namespace WealthTrack.API.ApiModels.Category
{
    public class CategoryUpsertApiModel
    {
        public string? Name { get; set; }

        public string? IconName { get; set; }

        public OperationType? Type { get; set; }

        public Guid? ParentCategoryId { get; set; }
    }
}