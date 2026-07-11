using System;
using System.IO;

namespace SysBot.Pokemon.Helpers
{
    /// <summary>
    /// Configurable base URL for raid embed sprite images.
    ///
    /// Defaults to PKM Universe's cached sprite endpoint (creator.pkm-universe.com/rs) so the bot
    /// serves rate-limit-free sprites out of the box. Anyone running their own copy can point this
    /// at a different host — e.g. their own mirror of the /rs cached proxy — without recompiling:
    ///   * set the RAIDBOT_SPRITE_HOST environment variable, or
    ///   * drop a spritehost.txt (one line, the base URL) next to the EXE.
    ///
    /// The value should be the base that /home, /spr, /alt (etc.) sub-paths hang off of, with no
    /// trailing slash.
    /// </summary>
    public static class RaidSpriteHost
    {
        public const string Default = "https://creator.pkm-universe.com/rs";

        public static readonly string Base = Resolve();

        private static string Resolve()
        {
            try
            {
                var env = Environment.GetEnvironmentVariable("RAIDBOT_SPRITE_HOST");
                if (!string.IsNullOrWhiteSpace(env))
                    return env.Trim().TrimEnd('/');

                var exeDir = Path.GetDirectoryName(Environment.ProcessPath ?? "");
                if (!string.IsNullOrEmpty(exeDir))
                {
                    var file = Path.Combine(exeDir, "spritehost.txt");
                    if (File.Exists(file))
                    {
                        var v = File.ReadAllText(file).Trim();
                        if (!string.IsNullOrWhiteSpace(v))
                            return v.TrimEnd('/');
                    }
                }
            }
            catch
            {
                // fall through to default
            }
            return Default;
        }
    }
}
