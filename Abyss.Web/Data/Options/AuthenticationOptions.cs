namespace Abyss.Web.Data.Options;

public class AuthenticationOptions
{
    public InternalAuthenticationProviderOptions ExternalLogin { get; set; }

    public InternalAuthenticationProviderOptions RefreshToken { get; set; }

    public InternalAuthenticationProviderOptions AccessToken { get; set; }
}

public class InternalAuthenticationProviderOptions
{
    public int ValidMinutes { get; set; }
}
