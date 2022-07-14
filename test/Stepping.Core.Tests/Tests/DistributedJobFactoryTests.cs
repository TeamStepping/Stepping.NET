using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Jobs;
using Stepping.TestBase.Fakes;
using Xunit;

namespace Stepping.Core.Tests.Tests;

public class DistributedJobFactoryTests : SteppingCoreTestBase
{
    protected IDistributedJobFactory DistributedJobFactory { get; }

    public DistributedJobFactoryTests()
    {
        DistributedJobFactory = ServiceProvider.GetRequiredService<IDistributedJobFactory>();
    }

    [Fact]
    public async Task Should_Create_Job()
    {
        var job = await DistributedJobFactory.CreateJobAsync("my-gid", null);

        job.ShouldNotBeNull();
        job.Gid.ShouldBe("my-gid");
        job.DbContext.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Create_Job_With_Db_Transaction()
    {
        var dbContext = new FakeSteppingDbContext(true);

        var job = await DistributedJobFactory.CreateJobAsync("my-gid", dbContext);

        job.ShouldNotBeNull();
        job.Gid.ShouldBe("my-gid");
        job.DbContext.ShouldBe(dbContext);
    }

    [Fact]
    public async Task Should_Create_Advanced_Job()
    {
        var job = await DistributedJobFactory.CreateAdvancedJobAsync("my-gid", null);

        job.ShouldNotBeNull();
        job.Gid.ShouldBe("my-gid");
        job.DbContext.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Create_Advanced_Job_With_Db_Transaction()
    {
        var dbContext = new FakeSteppingDbContext(true);

        var job = await DistributedJobFactory.CreateAdvancedJobAsync("my-gid", dbContext);

        job.ShouldNotBeNull();
        job.Gid.ShouldBe("my-gid");
        job.DbContext.ShouldBe(dbContext);
    }
}