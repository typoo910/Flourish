namespace AcksheedSys.Flourish.Abstract;

public interface IFlourishPageHistoryService
{
    bool CanGoBack { get; }

    bool CanGoForward { get; }

    IReadOnlyCollection<FlourishPageStackEntry> BackStack { get; }

    IReadOnlyCollection<FlourishPageStackEntry> ForwardStack { get; }

    void Clear();
}
