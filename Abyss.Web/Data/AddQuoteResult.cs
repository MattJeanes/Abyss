using DontPanic.TumblrSharp.Client;

namespace Abyss.Web.Data
{
    public class AddQuoteResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public QuotePost Quote { get; set; }
    }
}
