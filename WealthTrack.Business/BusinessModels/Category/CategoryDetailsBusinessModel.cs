using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.BusinessModels.Category
{
    public class CategoryDetailsBusinessModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string? IconName { get; set; }

        public OperationType? Type { get; set; }
        
        public bool IsSystem { get; set; }

        public CategoryRelatedToCategoryDetailsBusinessModel? ParentCategory { get; set; }

        public List<CategoryRelatedToCategoryDetailsBusinessModel> ChildCategories { get; set; }
    }

    public class CategoryRelatedToCategoryDetailsBusinessModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string? IconName { get; set; }
        
        public List<CategoryRelatedToCategoryDetailsBusinessModel> ChildCategories { get; set; }
    }
}