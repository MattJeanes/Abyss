using System.Collections.Generic;

namespace Abyss.Web.Data
{
    public static class AuthSchemes
    {
        public static AuthScheme Steam = new AuthScheme
        {
            Id = "steam",
            Name = "Steam"
        };

        public static AuthScheme Google = new AuthScheme
        {
            Id = "google",
            Name = "Google"
        };

        public static List<AuthScheme> All = new List<AuthScheme>
        {
            Steam,
            Google,
        };
    }
}
