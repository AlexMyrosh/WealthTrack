using WealthTrack.IntegrationTests.WebAppFactories;

namespace WealthTrack.IntegrationTests.TestCollections
{
    [CollectionDefinition("GoalTests", DisableParallelization = true)]
    public class GoalTestCollection : ICollectionFixture<EmptyWebAppFactory> { }
}
