using AcksheedSys.Flourish.Abstract;

namespace AcksheedSys.Flourish.Internal;

internal sealed record NavigablePageRegistration(
    Type PageType,
    string DisplayName,
    string IconGlyph,
    bool IsInitial,
    FlourishPageCacheMode CacheMode
);
