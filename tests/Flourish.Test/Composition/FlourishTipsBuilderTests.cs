using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Composition;

namespace ArkheideSystem.Flourish.Test.Composition;

public sealed class FlourishTipsBuilderTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(350, 12.5)]
    public void ConfigurationMethods_WithNonNegativeValues_UpdateOptionsAndReturnBuilder(
        int delay,
        double margin
    )
    {
        var options = new FlourishTipOptions();
        var sut = new FlourishTipsBuilder(options);

        Assert.Same(sut, sut.SetDelay(delay));
        Assert.Same(sut, sut.SetSpawnableMargin(margin));
        Assert.Equal(delay, options.InitialShowDelayMilliseconds);
        Assert.Equal(margin, options.SpawnableMargin);
    }

    [Fact]
    public void SetDelay_WithNegativeValue_ThrowsArgumentOutOfRangeException()
    {
        var sut = new FlourishTipsBuilder(new FlourishTipOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => sut.SetDelay(-1));

        Assert.Equal("milliseconds", exception.ParamName);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void SetSpawnableMargin_WithInvalidValue_ThrowsArgumentOutOfRangeException(
        double margin
    )
    {
        var sut = new FlourishTipsBuilder(new FlourishTipOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetSpawnableMargin(margin)
        );

        Assert.Equal("margin", exception.ParamName);
    }
}
