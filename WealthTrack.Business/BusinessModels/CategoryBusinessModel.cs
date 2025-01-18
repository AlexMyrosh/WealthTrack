using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.BusinessModels
{
    public class CategoryBusinessModel
    {
        public string Name { get; set; }

        public string IconName { get; set; }

        public CategoryType Type { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset ModifiedDate { get; set; }

        public CategoryStatus Status { get; set; }

        public CategoryBusinessModel ParentCategory { get; set; }

        public List<CategoryBusinessModel> ChildCategories { get; set; }
    }
}