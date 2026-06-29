using System;

namespace Vistara.Wpf.Services;

public sealed record PageStackEntry(Type SourcePageType, object? Parameter);
