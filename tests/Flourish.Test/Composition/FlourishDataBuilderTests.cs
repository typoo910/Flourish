using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Composition;

namespace ArkheideSystem.Flourish.Test.Composition;

public sealed class FlourishDataBuilderTests
{
    [Fact]
    public void ConfigurationMethods_WithValidValues_UpdateOptionsAndReturnBuilder()
    {
        var options = new FlourishDataOptions();
        var sut = new FlourishDataBuilder(options);

        Assert.Same(sut, sut.SetAppPreferenceDataPath("C:\\Flourish Data"));
        Assert.Same(sut, sut.SetAppName("Gallery"));
        Assert.Same(sut, sut.SetAppCompany("Arkheide"));

        Assert.Equal("C:\\Flourish Data", options.AppPreferenceDataPath);
        Assert.Equal("Gallery", options.AppName);
        Assert.Equal("Arkheide", options.CompanyName);
        Assert.True(options.HasConfiguration);
    }

    [Theory]
    [InlineData("path", null)]
    [InlineData("path", "")]
    [InlineData("path", "   ")]
    [InlineData("appName", null)]
    [InlineData("appName", "")]
    [InlineData("appName", "   ")]
    [InlineData("companyName", null)]
    [InlineData("companyName", "")]
    [InlineData("companyName", "   ")]
    public void ConfigurationMethods_WithBlankValue_ThrowArgumentException(
        string parameterName,
        string? value
    )
    {
        var options = new FlourishDataOptions();
        var sut = new FlourishDataBuilder(options);

        var exception = Assert.Throws<ArgumentException>(() =>
        {
            switch (parameterName)
            {
                case "path":
                    sut.SetAppPreferenceDataPath(value!);
                    break;
                case "appName":
                    sut.SetAppName(value!);
                    break;
                case "companyName":
                    sut.SetAppCompany(value!);
                    break;
            }
        });

        Assert.Equal(parameterName, exception.ParamName);
        Assert.False(options.HasConfiguration);
    }
}
