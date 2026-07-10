namespace ArkheideSystem.Flourish.Services;

internal interface IPageFactory
{
    object? Create(Type sourcePageType);
}
