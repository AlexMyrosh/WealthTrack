using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.BusinessModels.Category
{
    public class CategoryDetailsBusinessModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string? IconName { get; set; }

        public OperationType? Type { get; set; }

        public ParentCategoryDetailsBusinessModel ParentCategory { get; set; }

        public List<ChildCategoryDetailsBusinessModel> ChildCategories { get; set; }
    }

    public class ParentCategoryDetailsBusinessModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string? IconName { get; set; }
    }

    public class ChildCategoryDetailsBusinessModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string? IconName { get; set; }

        public List<ChildCategoryDetailsBusinessModel> ChildCategories { get; set; }
    }
}