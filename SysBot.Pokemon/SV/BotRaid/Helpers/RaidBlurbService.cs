using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SysBot.Base;

namespace SysBot.Pokemon.SV.BotRaid.Helpers;

/// <summary>
/// Generates a one-line tactical flavor blurb for a raid embed via Google Gemini
/// (gemini-2.0-flash-lite). Results are cached per species+tera+ability so the same
/// raid type doesn't re-hit the API. Fully best-effort: any failure returns "" and the
/// embed simply omits the blurb.
/// </summary>
public static class RaidBlurbService
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(6) };
    private static readonly ConcurrentDictionary<string, string> Cache = new();

    /// <summary>
    /// Returns a short blurb (≤ ~160 chars) or empty string on any failure / disabled state.
    /// </summary>
    public static async Task<string> GenerateAsync(
        string apiKey, string species, string tera, string ability,
        string moves, bool shiny, int stars, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(species))
            return string.Empty;

        var cacheKey = $"{species}|{tera}|{ability}|{shiny}|{stars}";
        if (Cache.TryGetValue(cacheKey, out var cached))
            return cached;

        try
        {
            string prompt =
                $"Write ONE punchy sentence (max 22 words, no emoji, no quotes) hyping a Pokémon Tera Raid for a Discord community. " +
                $"Pokémon: {(shiny ? "Shiny " : "")}{species}. Tera type: {tera}. Ability: {ability}. " +
                $"Moves: {moves}. Difficulty: {stars}-star. " +
                $"Give a quick tactical tip (what to bring or watch for). Be exciting but concise.";

            var body = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                },
                generationConfig = new { temperature = 0.9, maxOutputTokens = 80 }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-lite:generateContent?key={apiKey}";
            using var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            using var resp = await Http.PostAsync(url, content, token).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                LogUtil.LogInfo($"AI blurb HTTP {(int)resp.StatusCode}; skipping.", nameof(RaidBlurbService));
                return string.Empty;
            }

            var json = await resp.Content.ReadAsStringAsync(token).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            text = text.Trim().Trim('"');
            // Hard cap so a runaway response can't blow out the embed field.
            if (text.Length > 200) text = text[..200].TrimEnd() + "…";

            if (!string.IsNullOrWhiteSpace(text))
                Cache[cacheKey] = text;
            return text;
        }
        catch (OperationCanceledException) { return string.Empty; }
        catch (Exception ex)
        {
            LogUtil.LogInfo($"AI blurb failed: {ex.Message}", nameof(RaidBlurbService));
            return string.Empty;
        }
    }
}
