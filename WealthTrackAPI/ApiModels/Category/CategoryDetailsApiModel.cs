using WealthTrack.Shared.Enums;

namespace WealthTrack.API.ApiModels.Category
{
    public class CategoryDetailsApiModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string IconName { get; set; }

        public CategoryType Type { get; set; }

        public ParentCategoryDetailsApiModel ParentCategory { get; set; }

        public List<ChildCategoryDetailsApiModel> ChildCategories { get; set; }
    }

    public class ParentCategoryDetailsApiModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string IconName { get; set; }
    }

    public class ChildCategoryDetailsApiModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string IconName { get; set; }

        public List<ChildCategoryDetailsApiModel> ChildCategories { get; set; }
    }
}