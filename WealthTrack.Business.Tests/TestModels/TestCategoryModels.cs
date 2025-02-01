using WealthTrack.Business.BusinessModels.Category;
using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Tests.TestModels
{
    public static class TestCategoryModels
    {
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
                    Id = DomainModel.Id,
                    Name = DomainModel.Name,
                    IconName = DomainModel.IconName,
                    Type = DomainModel.Type,
                    ParentCategory = new ParentCategoryDetailsBusinessModel
                    {
                        Id = DomainModelWithoutDetails.Id,
                        Name = DomainModel.Name,
                        IconName = DomainModel.IconName
                    },
                    ChildCategories =
                    [
                        new()
                        {
                            Id = DomainModelWithoutDetails.Id,
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
                    Id = Guid.NewGuid(),
                    Name = "Test category domain model",
                    IconName = "Test category icon name",
                    Type = CategoryType.Income,
                    CreatedDate = DateTimeOffset.Now,
                    ModifiedDate = DateTimeOffset.Now,
                    Status = CategoryStatus.Active
                };
            }
        }
    }
}
