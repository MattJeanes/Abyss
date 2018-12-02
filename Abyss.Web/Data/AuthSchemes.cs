using System.Collections.Generic;

namespace Abyss.Web.Data
{
    public static class AuthSchemes
    {
        // Internal schemes

        public const string JsonWebToken = "JsonWebToken";

        public const string ExternalLogin = "ExternalLogin";

        public const string RefreshToken = "RefreshToken";

        // External schemes 
        public static AuthScheme Steam = new AuthScheme
        {
            Id = "steam",
            Name = "Steam",
            ProfileUrl = "http://steamcommunity.com/profiles/",
            IconUrl = "/images/steam.png"
        };

        public static AuthScheme Google = new AuthScheme
        {
            Id = "google",
            Name = "Google",
            ProfileUrl = "https://plus.google.com/",
            IconUrl = "/images/google.png"
        };

        public static AuthScheme Discord = new AuthScheme
        {
            Id = "discord",
            Name = "Discord",
            IconUrl = "/images/discord.png"
        };

        public static List<AuthScheme> ExternalSchemes = new List<AuthScheme>
        {
            Steam,
            Google,
            Discord
        };
    }
}
