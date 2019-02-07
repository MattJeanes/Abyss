using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abyss.Web.Data;
using Abyss.Web.Data.Options;
using Abyss.Web.Helpers.Interfaces;
using DontPanic.TumblrSharp;
using DontPanic.TumblrSharp.Client;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Options;

namespace Abyss.Web.Commands.Discord
{
    public class QuoteCommand : BaseCommand
    {
        private readonly TumblrClient _client;
        private readonly TumblrOptions _options;
        private List<QuotePost> _posts = new List<QuotePost>();
        private List<QuotePost> _localPosts = new List<QuotePost>();
        private DateTime _cacheExpiry = DateTime.UtcNow;

        public override string Command => "quote";

        public QuoteCommand(IServiceProvider serviceProvider, TumblrClient client, IOptions<TumblrOptions> options) : base(serviceProvider)
        {
            _client = client;
            _options = options.Value;
        }

        public override async Task ProcessMessage(MessageCreateEventArgs e, List<string> args)
        {
            switch (args.FirstOrDefault())
            {
                case null:
                    await GetQuote(e);
                    break;
                case "add":
                    await AddQuote(e, args.Skip(1).ToList());
                    break;
                default:
                    await e.Message.RespondAsync("Unknown sub-command, try: (none), add");
                    break;
            }
        }

        private async Task AddQuote(MessageCreateEventArgs e, List<string> args)
        {
            if (args.Count() != 2)
            {
                await e.Message.RespondAsync($"Syntax: `add \"<quote>\" \"<author>\"`");
                return;
            }
            var clientUser = await GetClientUser(e);
            if (clientUser == null) {
                await e.Message.RespondAsync("You are not registered");
                return;
            };
            if (!_userHelper.HasPermission(clientUser, Permissions.QuoteManager)) {
                await e.Message.RespondAsync("You are not authorized");
                return;
            }
            var quote = args[0];
            var source = args[1];
            await UpdateQuotes();
            var existing = _posts.Any(x => x.Summary == quote) || _localPosts.Any(x => x.Summary == quote);
            if (existing)
            {
                await e.Message.RespondAsync("That quote already exists");
                return;
            }
            var post = await _client.CreatePostAsync(_options.BlogName, PostData.CreateQuote(quote, source));
            _localPosts.Add(new QuotePost
            {
                Id = post.PostId,
                Summary = quote,
                Source = source
            });
            await e.Message.RespondAsync($"_‟{quote}“_ – {source}");
        }

        private async Task GetQuote(MessageCreateEventArgs e)
        {
            var quotes = await GetAllQuotes();
            var quote = quotes.OrderBy(x => Guid.NewGuid()).First();
            await e.Message.RespondAsync($"_‟{quote.Summary}“_ – {quote.Source}");
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
