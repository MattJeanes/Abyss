using Abyss.Web.Data;
using DontPanic.TumblrSharp.Client;

namespace Abyss.Web.Helpers.Interfaces;

public interface IQuoteHelper
{
    Task<AddQuoteResult> AddQuote(string quote, string source);
    string FormatQuote(QuotePost quote);
    Task<QuotePost> GetQuote();
}
