using Discord;

namespace SysBot.Pokemon.SV.BotRaid.Helpers;

/// <summary>
/// PKM Universe Raid System branding helpers — Tera-typed embed colors and visual star rendering.
/// </summary>
public static class PkmUniverseTheme
{
    /// <summary>Render a star count as visual stars: 3 → "★★★", 7 → "★★★★★★★".</summary>
    public static string VisualStars(int count)
    {
        if (count <= 0)
            return string.Empty;
        if (count > 7)
            count = 7;
        return new string('★', count);
    }

    /// <summary>
    /// Map a Tera type name (English, case-insensitive) to its canonical Discord color.
    /// Returns <c>null</c> for unknown / empty input so callers can fall back to dominant-color.
    /// </summary>
    public static Color? TeraColor(string? teraTypeEnglish)
    {
        if (string.IsNullOrWhiteSpace(teraTypeEnglish))
            return null;

        return teraTypeEnglish.Trim().ToLowerInvariant() switch
        {
            "normal"   => new Color(0xA8, 0xA8, 0x78),
            "fire"     => new Color(0xF0, 0x80, 0x30),
            "water"    => new Color(0x68, 0x90, 0xF0),
            "electric" => new Color(0xF8, 0xD0, 0x30),
            "grass"    => new Color(0x78, 0xC8, 0x50),
            "ice"      => new Color(0x98, 0xD8, 0xD8),
            "fighting" => new Color(0xC0, 0x30, 0x28),
            "poison"   => new Color(0xA0, 0x40, 0xA0),
            "ground"   => new Color(0xE0, 0xC0, 0x68),
            "flying"   => new Color(0xA8, 0x90, 0xF0),
            "psychic"  => new Color(0xF8, 0x58, 0x88),
            "bug"      => new Color(0xA8, 0xB8, 0x20),
            "rock"     => new Color(0xB8, 0xA0, 0x38),
            "ghost"    => new Color(0x70, 0x58, 0x98),
            "dragon"   => new Color(0x70, 0x38, 0xF8),
            "dark"     => new Color(0x70, 0x58, 0x48),
            "steel"    => new Color(0xB8, 0xB8, 0xD0),
            "fairy"    => new Color(0xEE, 0x99, 0xAC),
            "stellar"  => new Color(0x40, 0xB5, 0xA5),
            _          => null,
        };
    }
}
