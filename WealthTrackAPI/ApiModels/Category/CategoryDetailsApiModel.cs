using WealthTrack.Shared.Enums;

namespace WealthTrack.API.ApiModels.Category
{
    public class CategoryDetailsApiModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string? IconName { get; set; }

        public OperationType? Type { get; set; }
        
        public bool IsSystem { get; set; }

        public CategoryRelatedToCategoryDetailsApiModel? ParentCategory { get; set; }

        public List<CategoryRelatedToCategoryDetailsApiModel> ChildCategories { get; set; }
    }

    public class CategoryRelatedToCategoryDetailsApiModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string? IconName { get; set; }
        
        public List<CategoryRelatedToCategoryDetailsApiModel> ChildCategories { get; set; }
    }
}