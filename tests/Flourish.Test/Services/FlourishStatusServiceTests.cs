using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class FlourishStatusServiceTests
{
    [Fact]
    public void Properties_ReturnCurrentStatusOptions()
    {
        var options = new FlourishShellOptions { StatusText = "Starting" };
        options.StatusItems.Add(new FlourishStatusItem("Offline", "O"));
        var sut = new FlourishStatusService(options);

        Assert.Equal("Starting", sut.StatusText);
        Assert.Same(options.StatusItems, sut.StatusItems);

        options.StatusText = "Ready";
        options.StatusItems.Add(new FlourishStatusItem("Online", "N"));

        Assert.Equal("Ready", sut.StatusText);
        Assert.Equal(2, sut.StatusItems.Count);
        Assert.Equal("Online", sut.StatusItems[1].Text);
    }
}
