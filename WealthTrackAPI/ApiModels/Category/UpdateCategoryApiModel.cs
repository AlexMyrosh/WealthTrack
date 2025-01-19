using WealthTrack.Shared.Enums;

namespace WealthTrack.API.ApiModels.Category
{
    public class UpdateCategoryApiModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string? IconName { get; set; }

        public CategoryType Type { get; set; }

        public Guid? ParentCategoryId { get; set; }
    }
}