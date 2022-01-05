using DontPanic.TumblrSharp.Client;

namespace Abyss.Web.Data;

public struct AddQuoteResult
{
    public bool Success;
    public string ErrorMessage;
    public QuotePost Quote;
}
