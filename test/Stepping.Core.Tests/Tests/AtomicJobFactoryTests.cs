using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Exceptions;
using Stepping.Core.Jobs;
using Stepping.TestBase.Fakes;
using Xunit;

namespace Stepping.Core.Tests.Tests;

public class AtomicJobFactoryTests : SteppingCoreTestBase
{
    protected IAtomicJobFactory AtomicJobFactory { get; }

    public AtomicJobFactoryTests()
    {
        AtomicJobFactory = ServiceProvider.GetRequiredService<IAtomicJobFactory>();
    }

    [Fact]
    public async Task Should_Create_Job()
    {
        var job = await AtomicJobFactory.CreateJobAsync("my-gid");

        job.ShouldNotBeNull();
        job.Gid.ShouldBe("my-gid");
        job.DbContext.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Create_Job_Without_Gid_Input()
    {
        var job = await AtomicJobFactory.CreateJobAsync();

        job.ShouldNotBeNull();
        job.Gid.ShouldNotBeNullOrWhiteSpace();
        job.DbContext.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Create_Job_With_Transactional_DbContext()
    {
        var dbContext = new FakeSteppingDbContext(true);

        var job = await AtomicJobFactory.CreateJobAsync("my-gid", dbContext);

        job.ShouldNotBeNull();
        job.Gid.ShouldBe("my-gid");
        job.DbContext.ShouldBe(dbContext);
    }

    [Fact]
    public async Task Should_Not_Create_Job_With_Non_Transactional_DbContext()
    {
        var dbContext = new FakeSteppingDbContext(false);

        (await Should.ThrowAsync<SteppingException>(() => AtomicJobFactory.CreateJobAsync("my-gid", dbContext)))
            .Message.ShouldBe("Specified DB context should be with a transaction.");
    }

    [Fact]
    public async Task Should_Create_Advanced_Job()
    {
        var job = await AtomicJobFactory.CreateAdvancedJobAsync("my-gid");

        job.ShouldNotBeNull();
        job.Gid.ShouldBe("my-gid");
        job.DbContext.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Create_Advanced_Job_Without_Gid_Input()
    {
        var job = await AtomicJobFactory.CreateAdvancedJobAsync();

        job.ShouldNotBeNull();
        job.Gid.ShouldNotBeNullOrWhiteSpace();
        job.DbContext.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Create_Advanced_Job_With_Transactional_DbContext()
    {
        var dbContext = new FakeSteppingDbContext(true);

        var job = await AtomicJobFactory.CreateAdvancedJobAsync("my-gid", dbContext);

        job.ShouldNotBeNull();
        job.Gid.ShouldBe("my-gid");
        job.DbContext.ShouldBe(dbContext);
    }

    [Fact]
    public async Task Should_Not_Create_Advanced_Job_With_Non_Transactional_DbContext()
    {
        var dbContext = new FakeSteppingDbContext(false);

        (await Should.ThrowAsync<SteppingException>(
                () => AtomicJobFactory.CreateAdvancedJobAsync("my-gid", dbContext)))
            .Message.ShouldBe("Specified DB context should be with a transaction.");
    }
}