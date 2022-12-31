using System.Threading.Tasks;
using LanguageExt.SysX.Test;
using Xunit;
using static LanguageExt.Prelude;
using Activity = LanguageExt.SysX.Diag.Activity<LanguageExt.SysX.Test.Runtime>;
using Activity2 = LanguageExt.SysX.Diag.Activity2<LanguageExt.SysX.Test.Runtime>;

namespace LanguageExt.Tests;

public static class ActivityTests
{
    [Fact(DisplayName = "span with an inner Eff will throw a null error if used in an effect")]
    public static void Case1()
    {
        var effect = Activity.span("test", SuccessEff<Runtime, int>(1));

        var result = effect.Run(Runtime.New());

        Assert.True(result.IsFail);
        Assert.Throws<ValueIsNullException>(() => result.ThrowIfFail());
    }

    [Fact(DisplayName = "span with an inner Aff will throw a null error if used in an effect")]
    public static async Task Case2()
    {
        var effect = Activity.span("test", SuccessAff<Runtime, int>(1));

        var result = await effect.Run(Runtime.New());

        Assert.True(result.IsFail);
        Assert.Throws<ValueIsNullException>(() => result.ThrowIfFail());
    }

    [Fact(DisplayName = "span with an inner Eff will run without issue")]
    public static void Case3()
    {
        var effect = Activity2.span("test", SuccessEff<Runtime, int>(1));

        var result = effect.Run(Runtime.New());

        Assert.True(result.IsSucc);
        Assert.Equal(1, result.Case);
    }

    [Fact(DisplayName = "span with an inner Aff will run without issue")]
    public static async Task Case4()
    {
        var effect = Activity2.span("test", SuccessAff<Runtime, int>(1));

        var result = await effect.Run(Runtime.New());

        Assert.True(result.IsSucc);
        Assert.Equal(1, result.Case);
    }
}
