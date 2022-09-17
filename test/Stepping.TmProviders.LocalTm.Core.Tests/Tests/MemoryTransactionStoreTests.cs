using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Stepping.Core.Databases;
using Stepping.Core.Exceptions;
using Stepping.TestBase.Fakes;
using Stepping.TmProviders.LocalTm.Models;
using Stepping.TmProviders.LocalTm.Options;
using Stepping.TmProviders.LocalTm.Steps;
using Stepping.TmProviders.LocalTm.Store;
using Stepping.TmProviders.LocalTm.Timing;
using Xunit;

namespace Stepping.TmProviders.LocalTm.Core.Tests.Tests;

public class MemoryTransactionStoreTests : SteppingTmProvidersLocalTmCoreTestBase
{
    protected ITransactionStore TransactionStore { get; }
    protected ISteppingClock SteppingClock { get; }
    protected LocalTmOptions LocalTmOptions { get; }
    protected ISteppingDbContextLookupInfoProvider SteppingDbContextLookupInfoProvider { get; }

    public MemoryTransactionStoreTests()
    {
        TransactionStore = ServiceProvider.GetRequiredService<MemoryTransactionStore>();
        SteppingClock = ServiceProvider.GetRequiredService<ISteppingClock>();
        LocalTmOptions = ServiceProvider.GetRequiredService<IOptions<LocalTmOptions>>().Value;
        SteppingDbContextLookupInfoProvider = ServiceProvider.GetRequiredService<ISteppingDbContextLookupInfoProvider>();
    }

    [Fact]
    public async Task Should_GetPendingList_Transaction()
    {
        await TransactionStore.CreateAsync(await GenerateTmTransactionModelAsync(SteppingClock.Now.Add(LocalTmOptions.Timeout)));
        await TransactionStore.CreateAsync(await GenerateTmTransactionModelAsync(SteppingClock.Now));
        await TransactionStore.CreateAsync(await GenerateTmTransactionModelAsync(SteppingClock.Now.Add(-LocalTmOptions.Timeout)));
        await TransactionStore.CreateAsync(await GenerateTmTransactionModelAsync(SteppingClock.Now.AddMinutes(-2)));
        await TransactionStore.CreateAsync(await GenerateTmTransactionModelAsync(SteppingClock.Now.AddHours(-1)));
        var nextModel1 = await GenerateTmTransactionModelAsync(SteppingClock.Now);
        nextModel1.NextRetryTime = SteppingClock.Now;
        nextModel1.NextRetryInterval = 1;
        await TransactionStore.CreateAsync(nextModel1);
        var nextModel2 = await GenerateTmTransactionModelAsync(SteppingClock.Now);
        nextModel2.CalculateNextRetryTime(SteppingClock.Now.AddSeconds(5));
        await TransactionStore.CreateAsync(nextModel2);

        var transaction = await TransactionStore.GetPendingListAsync();

        transaction.Count.ShouldBe(4);
    }

    [Fact]
    public async Task Should_Get_Transaction()
    {
        var createModel = await GenerateTmTransactionModelAsync();

        await TransactionStore.CreateAsync(createModel);

        var model = await TransactionStore.GetAsync(createModel.Gid);
        model.Gid.ShouldBe(createModel.Gid);
        model.Status.ShouldBe(createModel.Status);
        model.CreationTime.ShouldBe(createModel.CreationTime);
        model.NextRetryTime.ShouldBe(model.NextRetryTime);
        model.NextRetryInterval.ShouldBe(model.NextRetryInterval);
        model.RollbackReason.ShouldBe(model.RollbackReason);
        model.RollbackTime.ShouldBe(model.RollbackTime);
        model.FinishTime.ShouldBe(model.FinishTime);
        model.ConcurrencyStamp.ShouldNotBeNullOrWhiteSpace();
        model.UpdateTime.ShouldBe(createModel.UpdateTime);
        model.SteppingDbContextLookupInfo?.DbProviderName.ShouldBe(createModel.SteppingDbContextLookupInfo?.DbProviderName);
        model.SteppingDbContextLookupInfo?.HashedConnectionString.ShouldBe(createModel.SteppingDbContextLookupInfo?.HashedConnectionString);
        model.SteppingDbContextLookupInfo?.DbContextType.ShouldBe(createModel.SteppingDbContextLookupInfo?.DbContextType);
        model.SteppingDbContextLookupInfo?.Database.ShouldBe(createModel.SteppingDbContextLookupInfo?.Database);
        model.SteppingDbContextLookupInfo?.TenantId.ShouldBe(createModel.SteppingDbContextLookupInfo?.TenantId);
        model.SteppingDbContextLookupInfo?.CustomInfo.ShouldBe(createModel.SteppingDbContextLookupInfo?.CustomInfo);
        model.Steps.ShouldNotBeNull();
        model.Steps.Steps.Count.ShouldBe(createModel.Steps.Steps.Count);
        model.Steps.Steps[0].StepName.ShouldBe(createModel.Steps.Steps[0].StepName);
        model.Steps.Steps[0].ArgsToByteString.ShouldBe(createModel.Steps.Steps[0].ArgsToByteString);
        model.Steps.Steps[0].Executed.ShouldBe(createModel.Steps.Steps[0].Executed);
    }

    [Fact]
    public async Task Should_Throw_Exception_If_Gid_Not_Exists()
    {
        await Should.ThrowAsync<InvalidOperationException>(async () => await TransactionStore.GetAsync(Guid.NewGuid().ToString("N")));
    }

    [Fact]
    public async Task Should_Get_Transaction_If_Gid_Exists()
    {
        var createModel = await GenerateTmTransactionModelAsync();

        await TransactionStore.CreateAsync(createModel);

        var model = await TransactionStore.FindAsync(createModel.Gid);
        model.ShouldNotBeNull();
        model.Gid.ShouldBe(createModel.Gid);
        model.Status.ShouldBe(createModel.Status);
        model.CreationTime.ShouldBe(createModel.CreationTime);
        model.NextRetryTime.ShouldBe(model.NextRetryTime);
        model.NextRetryInterval.ShouldBe(model.NextRetryInterval);
        model.RollbackReason.ShouldBe(model.RollbackReason);
        model.RollbackTime.ShouldBe(model.RollbackTime);
        model.FinishTime.ShouldBe(model.FinishTime);
        model.ConcurrencyStamp.ShouldNotBeNullOrWhiteSpace();
        model.UpdateTime.ShouldBe(createModel.UpdateTime);
        model.SteppingDbContextLookupInfo?.DbProviderName.ShouldBe(createModel.SteppingDbContextLookupInfo?.DbProviderName);
        model.SteppingDbContextLookupInfo?.HashedConnectionString.ShouldBe(createModel.SteppingDbContextLookupInfo?.HashedConnectionString);
        model.SteppingDbContextLookupInfo?.DbContextType.ShouldBe(createModel.SteppingDbContextLookupInfo?.DbContextType);
        model.SteppingDbContextLookupInfo?.Database.ShouldBe(createModel.SteppingDbContextLookupInfo?.Database);
        model.SteppingDbContextLookupInfo?.TenantId.ShouldBe(createModel.SteppingDbContextLookupInfo?.TenantId);
        model.SteppingDbContextLookupInfo?.CustomInfo.ShouldBe(createModel.SteppingDbContextLookupInfo?.CustomInfo);
        model.Steps.ShouldNotBeNull();
        model.Steps.Steps.Count.ShouldBe(createModel.Steps.Steps.Count);
        model.Steps.Steps[0].StepName.ShouldBe(createModel.Steps.Steps[0].StepName);
        model.Steps.Steps[0].ArgsToByteString.ShouldBe(createModel.Steps.Steps[0].ArgsToByteString);
        model.Steps.Steps[0].Executed.ShouldBe(createModel.Steps.Steps[0].Executed);
    }

    [Fact]
    public async Task Should_Return_Null_Transaction_If_Gid_Exists()
    {
        (await TransactionStore.FindAsync(Guid.NewGuid().ToString("N"))).ShouldBeNull();
    }

    [Fact]
    public async Task Should_Create_Transaction()
    {
        var existModel = await GenerateTmTransactionModelAsync();

        await TransactionStore.CreateAsync(existModel);

        var model = await TransactionStore.GetAsync(existModel.Gid);
        model.Gid.ShouldBe(existModel.Gid);
        model.Status.ShouldBe(existModel.Status);
        model.CreationTime.ShouldBe(existModel.CreationTime);
        model.NextRetryTime.ShouldBe(model.NextRetryTime);
        model.NextRetryInterval.ShouldBe(model.NextRetryInterval);
        model.RollbackReason.ShouldBe(model.RollbackReason);
        model.RollbackTime.ShouldBe(model.RollbackTime);
        model.FinishTime.ShouldBe(model.FinishTime);
        model.ConcurrencyStamp.ShouldNotBeNullOrWhiteSpace();
        model.UpdateTime.ShouldBe(existModel.UpdateTime);
        model.SteppingDbContextLookupInfo.ShouldNotBeNull();
        model.SteppingDbContextLookupInfo?.DbProviderName.ShouldBe(existModel.SteppingDbContextLookupInfo?.DbProviderName);
        model.SteppingDbContextLookupInfo?.HashedConnectionString.ShouldBe(existModel.SteppingDbContextLookupInfo?.HashedConnectionString);
        model.SteppingDbContextLookupInfo?.DbContextType.ShouldBe(existModel.SteppingDbContextLookupInfo?.DbContextType);
        model.SteppingDbContextLookupInfo?.Database.ShouldBe(existModel.SteppingDbContextLookupInfo?.Database);
        model.SteppingDbContextLookupInfo?.TenantId.ShouldBe(existModel.SteppingDbContextLookupInfo?.TenantId);
        model.SteppingDbContextLookupInfo?.CustomInfo.ShouldBe(existModel.SteppingDbContextLookupInfo?.CustomInfo);
        model.Steps.ShouldNotBeNull();
        model.Steps.Steps.Count.ShouldBe(existModel.Steps.Steps.Count);
        model.Steps.Steps[0].StepName.ShouldBe(existModel.Steps.Steps[0].StepName);
        model.Steps.Steps[0].ArgsToByteString.ShouldBe(existModel.Steps.Steps[0].ArgsToByteString);
        model.Steps.Steps[0].Executed.ShouldBe(existModel.Steps.Steps[0].Executed);
    }

    [Fact]
    public async Task Should_Not_Create_Transaction_If_Gid_Exists()
    {
        var existModel = await GenerateTmTransactionModelAsync();

        await TransactionStore.CreateAsync(existModel);

        await Should.ThrowAsync<SteppingException>(async () => await TransactionStore.CreateAsync(existModel));
    }

    [Fact]
    public async Task Should_Update_Transaction()
    {
        var existModel = await GenerateTmTransactionModelAsync();

        await TransactionStore.CreateAsync(existModel);

        var oldConcurrencyStamp = existModel.ConcurrencyStamp;
        existModel.UpdateTime = SteppingClock.Now;
        existModel.Status = LocalTmConst.StatusPrepare;
        existModel.CalculateNextRetryTime(SteppingClock.Now);

        await TransactionStore.UpdateAsync(existModel);

        var model = await TransactionStore.GetAsync(existModel.Gid);
        model.Gid.ShouldBe(existModel.Gid);
        model.Status.ShouldBe(existModel.Status);
        model.CreationTime.ShouldBe(existModel.CreationTime);
        model.NextRetryTime.ShouldBe(model.NextRetryTime);
        model.NextRetryInterval.ShouldBe(model.NextRetryInterval);
        model.RollbackReason.ShouldBe(model.RollbackReason);
        model.RollbackTime.ShouldBe(model.RollbackTime);
        model.FinishTime.ShouldBe(model.FinishTime);
        model.ConcurrencyStamp.ShouldNotBeNullOrWhiteSpace();
        model.ConcurrencyStamp.ShouldNotBe(oldConcurrencyStamp);
        model.UpdateTime.ShouldBe(existModel.UpdateTime);
        model.SteppingDbContextLookupInfo?.DbProviderName.ShouldBe(existModel.SteppingDbContextLookupInfo?.DbProviderName);
        model.SteppingDbContextLookupInfo?.HashedConnectionString.ShouldBe(existModel.SteppingDbContextLookupInfo?.HashedConnectionString);
        model.SteppingDbContextLookupInfo?.DbContextType.ShouldBe(existModel.SteppingDbContextLookupInfo?.DbContextType);
        model.SteppingDbContextLookupInfo?.Database.ShouldBe(existModel.SteppingDbContextLookupInfo?.Database);
        model.SteppingDbContextLookupInfo?.TenantId.ShouldBe(existModel.SteppingDbContextLookupInfo?.TenantId);
        model.SteppingDbContextLookupInfo?.CustomInfo.ShouldBe(existModel.SteppingDbContextLookupInfo?.CustomInfo);
        model.Steps.ShouldNotBeNull();
        model.Steps.Steps.Count.ShouldBe(existModel.Steps.Steps.Count);
        model.Steps.Steps[0].StepName.ShouldBe(existModel.Steps.Steps[0].StepName);
        model.Steps.Steps[0].ArgsToByteString.ShouldBe(existModel.Steps.Steps[0].ArgsToByteString);
        model.Steps.Steps[0].Executed.ShouldBe(existModel.Steps.Steps[0].Executed);
    }

    [Fact]
    public async Task Should_Not_Update_Transaction_If_ConcurrencyStamp_Not_Match()
    {
        var existModel = await GenerateTmTransactionModelAsync();

        await TransactionStore.CreateAsync(existModel);

        existModel.ConcurrencyStamp = Guid.NewGuid().ToString("N");

        await Should.ThrowAsync<SteppingException>(async () => await TransactionStore.UpdateAsync(existModel));
    }

    protected async Task<TmTransactionModel> GenerateTmTransactionModelAsync(DateTime? creationTime = null)
    {
        var model = new LocalTmStepModel();
        model.Steps.Add(new LocalTmStepInfoModel(FakeExecutableStep.FakeExecutableStepName, null));

        var steppingDbContextLookupInfo = await SteppingDbContextLookupInfoProvider.GetAsync(new FakeSteppingDbContext(true, "some-info"));

        return new TmTransactionModel(
            Guid.NewGuid().ToString(),
            model,
            steppingDbContextLookupInfo,
            creationTime ?? SteppingClock.Now
        );
    }
}
