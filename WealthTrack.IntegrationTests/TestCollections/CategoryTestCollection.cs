using WealthTrack.IntegrationTests.WebAppFactories;

namespace WealthTrack.IntegrationTests.TestCollections;

[CollectionDefinition("CategoryTests", DisableParallelization = true)]
public class CategoryTestCollection : ICollectionFixture<EmptyWebAppFactory>;