using System.Windows.Input;

namespace Flourish.Models;

internal sealed record FlourishCommandItem(string Label, string IconGlyph, ICommand? Command = null);
