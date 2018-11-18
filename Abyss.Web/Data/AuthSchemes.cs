using System.Collections.Generic;

namespace Abyss.Web.Data
{
    public static class AuthSchemes
    {
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

        public static List<AuthScheme> All = new List<AuthScheme>
        {
            Steam,
            Google,
        };
    }
}
