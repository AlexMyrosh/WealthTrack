using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.Helpers
{
    public static class TestCategoryModels
    {
        public static readonly Guid FirstCategoryId = new("920c5272-3c65-4b6d-92ed-d65c6672bec2");
        public static readonly Guid SecondCategoryId = new("0b7d7d48-2a85-435e-a09f-bd1155467493");
        public static readonly Guid ThirdCategoryId = new("d69fc57d-cb7a-4162-a599-06e5a1a04f30");

        public static Category FirstDomainModelWithoutDetails
        {
            get
            {
                return new Category
                {
                    Id = FirstCategoryId,
                    Name = "First Test Category Domain Model",
                    IconName = "Icon 1",
                    Type = CategoryType.Income,
                    CreatedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    ModifiedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    Status = CategoryStatus.Active
                };
            }
        }

        public static Category SecondDomainModelWithoutDetails
        {
            get
            {
                return new Category
                {
                    Id = SecondCategoryId,
                    Name = "Second Test Category Domain Model",
                    IconName = "Icon 2",
                    Type = CategoryType.Income,
                    CreatedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    ModifiedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    Status = CategoryStatus.Active
                };
            }
        }

        public static Category ThirdDomainModelWithoutDetails
        {
            get
            {
                return new Category
                {
                    Id = ThirdCategoryId,
                    Name = "Third Test Category Domain Model",
                    IconName = "Icon 3",
                    Type = CategoryType.Income,
                    CreatedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    ModifiedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    Status = CategoryStatus.Active
                };
            }
        }
    }
}
