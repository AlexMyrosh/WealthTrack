using WealthTrack.IntegrationTests.WebAppFactories;

namespace WealthTrack.IntegrationTests.TestCollections
{
    [CollectionDefinition("CurrencyTests", DisableParallelization = true)]
    public class CurrencyTestCollection : ICollectionFixture<EmptyWebAppFactory> { }
}
