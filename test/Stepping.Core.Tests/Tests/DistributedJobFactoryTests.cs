using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Exceptions;
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
    public async Task Should_Create_Job_With_Transactional_DbContext()
    {
        var dbContext = new FakeSteppingDbContext(true);

        var job = await DistributedJobFactory.CreateJobAsync("my-gid", dbContext);

        job.ShouldNotBeNull();
        job.Gid.ShouldBe("my-gid");
        job.DbContext.ShouldBe(dbContext);
    }

    [Fact]
    public async Task Should_Not_Create_Job_With_Non_Transactional_DbContext()
    {
        var dbContext = new FakeSteppingDbContext(false);

        await Should.ThrowAsync<SteppingException>(() => DistributedJobFactory.CreateJobAsync("my-gid", dbContext),
            "Specified DB context should be with a transaction.");
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
    public async Task Should_Not_Create_Advanced_Job_With_Non_Transactional_DbContext()
    {
        var dbContext = new FakeSteppingDbContext(false);

        await Should.ThrowAsync<SteppingException>(
            () => DistributedJobFactory.CreateAdvancedJobAsync("my-gid", dbContext),
            "Specified DB context should be with a transaction.");
    }
}