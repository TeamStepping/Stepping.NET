using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Stepping.Core.Databases;
using Stepping.Core.Infrastructures;
using Xunit;

namespace Stepping.Core.Tests.Tests;

public class BarrierInfoModelFactoryTests : SteppingCoreTestBase
{
    protected IBarrierInfoModelFactory BarrierInfoModelFactory { get; }

    public BarrierInfoModelFactoryTests()
    {
        BarrierInfoModelFactory = ServiceProvider.GetRequiredService<IBarrierInfoModelFactory>();
    }

    [Fact]
    public async Task Should_Create_BarrierInfoModel_For_Commit()
    {
        var model = await BarrierInfoModelFactory.CreateForCommitAsync("my-gid");

        model.ShouldNotBeNull();
        model.Gid.ShouldBe("my-gid");
        model.TransType.ShouldBe(SteppingConsts.TypeMsg);
        model.Op.ShouldBe(SteppingConsts.MsgOp);
        model.BarrierId.ShouldBe(SteppingConsts.MsgBarrierId);
        model.BranchId.ShouldBe(SteppingConsts.MsgBranchId);
        model.Reason.ShouldBe(SteppingConsts.MsgBarrierReasonCommit);
    }

    [Fact]
    public async Task Should_Create_BarrierInfoModel_For_Rollback()
    {
        var model = await BarrierInfoModelFactory.CreateForRollbackAsync("my-gid");

        model.ShouldNotBeNull();
        model.Gid.ShouldBe("my-gid");
        model.TransType.ShouldBe(SteppingConsts.TypeMsg);
        model.Op.ShouldBe(SteppingConsts.MsgOp);
        model.BarrierId.ShouldBe(SteppingConsts.MsgBarrierId);
        model.BranchId.ShouldBe(SteppingConsts.MsgBranchId);
        model.Reason.ShouldBe(SteppingConsts.MsgBarrierReasonRollback);
    }
}