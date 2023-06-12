namespace Abyss.Web.Data;

public static class AuthSchemes
{
    // Internal schemes

    public const string JsonWebToken = "JsonWebToken";

    public const string ExternalLogin = "ExternalLogin";

    public const string RefreshToken = "RefreshToken";

    // External schemes 
    public static AuthScheme Steam = new AuthScheme
    {
        Id = AuthSchemeType.Steam,
        Name = "Steam",
        ProfileUrl = "http://steamcommunity.com/profiles/",
        IconUrl = "/images/steam.png"
    };

    public static AuthScheme Google = new AuthScheme
    {
        Id = AuthSchemeType.Google,
        Name = "Google",
        ProfileUrl = "https://plus.google.com/",
        IconUrl = "/images/google.png"
    };

    public static AuthScheme Discord = new AuthScheme
    {
        Id = AuthSchemeType.Discord,
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
