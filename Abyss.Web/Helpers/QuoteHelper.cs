using Abyss.Web.Data;
using Abyss.Web.Data.Options;
using Abyss.Web.Helpers.Interfaces;
using DontPanic.TumblrSharp;
using DontPanic.TumblrSharp.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Web.Helpers
{
    public class QuoteHelper : IQuoteHelper
    {
        private readonly TumblrClient _client;
        private readonly TumblrOptions _options;
        private List<QuotePost> _posts = new List<QuotePost>();
        private List<QuotePost> _localPosts = new List<QuotePost>();
        private DateTime _cacheExpiry = DateTime.UtcNow;

        public QuoteHelper(TumblrClient client, IOptions<TumblrOptions> options)
        {
            _client = client;
            _options = options.Value;
        }

        public async Task<QuotePost> GetQuote()
        {
            var quotes = await GetAllQuotes();
            var quote = quotes.OrderBy(x => Guid.NewGuid()).First();
            return quote;
        }

        public async Task<AddQuoteResult> AddQuote(string quote, string source)
        {
            await UpdateQuotes();
            var existing = _posts.Any(x => x.Summary == quote) || _localPosts.Any(x => x.Summary == quote);
            if (existing)
            {
                return new AddQuoteResult { ErrorMessage = "That quote already exists" };
            }
            var post = await _client.CreatePostAsync(_options.BlogName, PostData.CreateQuote(quote, source));
            var newQuote = new QuotePost
            {
                Id = post.PostId,
                Summary = quote,
                Source = source,
                Timestamp = DateTime.Now
            };
            _localPosts.Add(newQuote);
            return new AddQuoteResult { Success = true, Quote = newQuote };
        }

        public string FormatQuote(QuotePost quote)
        {
            return $"_‟{quote.Summary}“_ – {quote.Source}, {quote.Timestamp.ToString("yyyy")}";
        }

        private async Task<List<QuotePost>> GetAllQuotes()
        {
            if (!_posts.Any() || DateTime.UtcNow >= _cacheExpiry)
            {
                await UpdateQuotes();
            }
            return _posts;
        }

        private async Task UpdateQuotes()
        {
            var count = 20;
            var startIndex = 0;
            var allPosts = new List<BasePost>();

            int lastCount;
            do
            {
                var posts = await _client.GetPostsAsync(_options.BlogName, startIndex: startIndex, count: count, type: PostType.Quote);
                allPosts.AddRange(posts.Result);
                lastCount = posts.Result.Count();
                startIndex += count;
            }
            while (lastCount >= count);
            _cacheExpiry = DateTime.UtcNow.AddMinutes(_options.CacheMinutes);
            _posts = allPosts.Select(x => (QuotePost)x).ToList();
        }
    }
}
