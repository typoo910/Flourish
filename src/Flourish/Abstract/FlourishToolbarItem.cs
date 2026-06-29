namespace AcksheedSys.Flourish.Abstract;

public sealed record FlourishToolbarItem(string DisplayName, string IconGlyph, Action? Action = null);
