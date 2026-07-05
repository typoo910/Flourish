using AcksheedSys.Flourish.Abstract;

namespace AcksheedSys.Flourish.Configuration;

internal sealed record NavigablePageRegistration(
    Type PageType,
    string DisplayName,
    string IconGlyph,
    FlourishPageCacheMode CacheMode
);
