using Orange.Domain.Common;

namespace Orange.Domain.Tests;

public class SmokeTest
{
    [Fact]
    public void EmployeeId_is_strongly_typed()
    {
        var id = new EmployeeId(Guid.NewGuid());
        Assert.NotEqual(default, id.Value);
    }

    [Fact]
    public void Result_success_contains_value()
    {
        Result<int> result = 42;
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Result_failure_contains_error()
    {
        Result<int> result = DomainErrors.EndBeforeStart;
        Assert.False(result.IsSuccess);
        Assert.Equal("EndBeforeStart", result.Error.Code);
    }
}
