using WealthTrack.Business.BusinessModels.Category;
using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.Helpers
{
    public static class TestCategoryModels
    {
        public static readonly Guid CategoryId = new("920c5272-3c65-4b6d-92ed-d65c6672bec2");
        public static readonly Guid ParentCategoryId = new("0b7d7d48-2a85-435e-a09f-bd1155467493");
        public static readonly Guid ChildCategoryId = new("d69fc57d-cb7a-4162-a599-06e5a1a04f30");

        public static Category DomainModel
        {
            get
            {
                var model = DomainModelWithoutDetails;
                model.ParentCategory = DomainModelWithoutDetails;
                model.ParentCategoryId = model.ParentCategory.Id;
                model.ChildCategories = [DomainModelWithoutDetails];
                model.Transactions = [TestTransactionModels.DomainModelWithoutDetails];
                model.Goals = [TestGoalModels.DomainModelWithoutDetails];
                return model;
            }
        }

        public static CategoryUpsertBusinessModel UpsertBusinessModel
        {
            get
            {
                var model = new CategoryUpsertBusinessModel
                {
                    Name = DomainModel.Name,
                    IconName = DomainModel.IconName,
                    Type = DomainModel.Type,
                    ParentCategoryId = DomainModel.ParentCategoryId
                };

                return model;
            }
        }

        public static CategoryDetailsBusinessModel DetailsBusinessModel
        {
            get
            {
                var model = new CategoryDetailsBusinessModel
                {
                    Id = CategoryId,
                    Name = DomainModel.Name,
                    IconName = DomainModel.IconName,
                    Type = DomainModel.Type,
                    ParentCategory = new ParentCategoryDetailsBusinessModel
                    {
                        Id = ParentCategoryId,
                        Name = DomainModel.Name,
                        IconName = DomainModel.IconName
                    },
                    ChildCategories =
                    [
                        new()
                        {
                            Id = ChildCategoryId,
                            Name = DomainModel.Name,
                            IconName = DomainModel.IconName,
                            ChildCategories = new List<ChildCategoryDetailsBusinessModel>()
                        }
                    ]
                };

                return model;
            }
        }

        public static Category DomainModelWithoutDetails
        {
            get
            {
                return new Category
                {
                    Id = CategoryId,
                    Name = "Test category domain model",
                    IconName = "Test category icon name",
                    Type = CategoryType.Income,
                    CreatedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    ModifiedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    Status = CategoryStatus.Active
                };
            }
        }
    }
}
