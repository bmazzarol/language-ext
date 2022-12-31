#nullable enable

using Xunit;

namespace LanguageExt.Tests;

public static class FinNullabilityIssueTests
{
    [Fact(DisplayName = "A nullable type cannot be passed to a Fin, it will always fail")]
    public static void Case1()
    {
        string? nullableString = null;

        Assert.Throws<ValueIsNullException>(() => Fin<string?>.Succ(nullableString));
    }
}
