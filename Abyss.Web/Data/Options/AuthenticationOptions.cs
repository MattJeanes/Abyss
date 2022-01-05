using System.Diagnostics.CodeAnalysis;

namespace Abyss.Web.Data.Options;

public class AuthenticationOptions
{
    [NotNull]
    public InternalAuthenticationProviderOptions? ExternalLogin { get; set; }

    [NotNull]
    public InternalAuthenticationProviderOptions? RefreshToken { get; set; }

    [NotNull]
    public InternalAuthenticationProviderOptions? AccessToken { get; set; }
}

public class InternalAuthenticationProviderOptions
{
    public int ValidMinutes { get; set; }
}
