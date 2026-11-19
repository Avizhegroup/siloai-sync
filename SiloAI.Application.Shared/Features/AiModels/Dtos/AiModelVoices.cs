using System.Text.RegularExpressions;

namespace SiloAI.Application.Shared.Features;

/// <summary>
/// A text-to-speech model's voices are stored as one comma-separated string (first = default).
/// These helpers are the single place that parses, validates and picks from it.
/// </summary>
public static class AiModelVoices
{
    public const int MaxLength = 1000;

    /// <summary>Voice names as providers publish them: letters, digits, '_', '-', '.', ':' (e.g. "alloy", "en-US-JennyNeural").</summary>
    private static readonly Regex ValidName = new(@"^[A-Za-z0-9][A-Za-z0-9_\-.:]{0,59}$", RegexOptions.Compiled);

    /// <summary>Splits on commas / semicolons / newlines, trims, drops blanks and case-insensitive duplicates.</summary>
    public static IReadOnlyList<string> Parse(string? voices)
    {
        if (string.IsNullOrWhiteSpace(voices)) return [];

        return voices
            .Split([',', ';', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>Canonical stored form: <c>"alloy,nova"</c>; null when there are no voices.</summary>
    public static string? Normalize(string? voices)
    {
        var list = Parse(voices);
        return list.Count == 0 ? null : string.Join(",", list);
    }

    /// <returns>An error message, or null when every voice name is well-formed and the list fits the column.</returns>
    public static string? Validate(string? voices)
    {
        var list = Parse(voices);

        var bad = list.FirstOrDefault(v => !ValidName.IsMatch(v));
        if (bad is not null)
            return $"Invalid voice name '{bad}'. Use letters, digits and . _ - : only (max 60 characters).";

        if (string.Join(",", list).Length > MaxLength)
            return $"The voice list is too long (max {MaxLength} characters).";

        return null;
    }

    /// <summary>
    /// Picks the voice to synthesize with. A blank request gets the model's default (first voice);
    /// a named voice must be one the model lists (case-insensitive) — returned in the model's own casing.
    /// A model with no voices configured cannot validate, so the requested name is passed through
    /// and a blank request falls back to <paramref name="fallbackDefault"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">The requested voice is not offered by the model.</exception>
    public static string Choose(IReadOnlyList<string> modelVoices, string? requested, string fallbackDefault = "alloy")
    {
        if (string.IsNullOrWhiteSpace(requested))
            return modelVoices.Count > 0 ? modelVoices[0] : fallbackDefault;

        if (modelVoices.Count == 0) return requested.Trim();

        return modelVoices.FirstOrDefault(v => string.Equals(v, requested.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(
                $"Voice '{requested}' is not supported by this model. Available voices: {string.Join(", ", modelVoices)}.");
    }
}
