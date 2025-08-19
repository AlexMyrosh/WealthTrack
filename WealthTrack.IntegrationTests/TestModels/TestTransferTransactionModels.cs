using WealthTrack.Data.DomainModels;

namespace WealthTrack.IntegrationTests.Helpers
{
    public static class TestTransferTransactionModels
    {
        public static readonly Guid FirstTransferTransactionId = new("91d23e0f-06b9-40ab-9ead-9ceeb8a32826");
        public static readonly Guid SecondTransferTransactionId = new("6553164a-0d2f-4c72-aa5a-95b1e9774409");

        public static TransferTransaction FirstDomainModelWithoutDetails
        {
            get
            {
                return new TransferTransaction
                {
                    Id = FirstTransferTransactionId,
                    Amount = 50.123M,
                    Description = "Test transfer transaction 1",
                    TransactionDate = new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero),
                    CreatedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero)
                };
            }
        }

        public static TransferTransaction SecondDomainModelWithoutDetails
        {
            get
            {
                return new TransferTransaction
                {
                    Id = SecondTransferTransactionId,
                    Amount = 100.123M,
                    Description = "Test transfer transaction 2",
                    TransactionDate = new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero),
                    CreatedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero)
                };
            }
        }
    }
}
