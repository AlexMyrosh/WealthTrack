using WealthTrack.IntegrationTests.WebAppFactories;

namespace WealthTrack.IntegrationTests.TestCollections
{
    [CollectionDefinition("TransactionTests", DisableParallelization = true)]
    public class TransactionTestCollection : ICollectionFixture<EmptyWebAppFactory> { }
}
