using WealthTrack.IntegrationTests.WebAppFactories;

namespace WealthTrack.IntegrationTests.TestCollections;

[CollectionDefinition("WalletTests", DisableParallelization = true)]
public class WalletTestCollection : ICollectionFixture<SeededWebAppFactory>;