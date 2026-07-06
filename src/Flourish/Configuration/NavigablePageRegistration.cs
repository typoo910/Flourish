using AckSS.Flourish.Abstract;

namespace AckSS.Flourish.Configuration;

internal sealed record NavigablePageRegistration(
    Type PageType,
    string DisplayName,
    string IconGlyph,
    FlourishPageCacheMode CacheMode
);
