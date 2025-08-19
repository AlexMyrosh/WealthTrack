using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.Helpers
{
    public static class TestTransactionModels
    {
        public static readonly Guid FirstTransactionId = new("84a584c8-0203-448e-9ab4-189ff0ae5f67");
        public static readonly Guid SecondTransactionId = new("826e05f3-e1a1-4365-abef-b80ad024af6a");

        public static Transaction FirstDomainModelWithoutDetails
        {
            get
            {
                return new Transaction
                {
                    Id = FirstTransactionId,
                    Amount = 50.123M,
                    Description = "Test transaction 1",
                    TransactionDate = new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero),
                    CreatedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    Type = TransactionType.Income
                };
            }
        }

        public static Transaction SecondDomainModelWithoutDetails
        {
            get
            {
                return new Transaction
                {
                    Id = SecondTransactionId,
                    Amount = 50.123M,
                    Description = "Test transaction 2",
                    TransactionDate = new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero),
                    CreatedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    Type = TransactionType.Expense
                };
            }
        }
    }
}
