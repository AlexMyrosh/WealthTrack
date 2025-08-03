using WealthTrack.IntegrationTests.WebAppFactories;

namespace WealthTrack.IntegrationTests.TestCollections
{
    [CollectionDefinition("BudgetTests", DisableParallelization = true)]
    public class BudgetTestCollection : ICollectionFixture<EmptyWebAppFactory> { }
}
