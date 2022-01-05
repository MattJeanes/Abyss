using Abyss.Web.Data;
using Abyss.Web.Helpers.Interfaces;
using DontPanic.TumblrSharp.Client;
using DSharpPlus.EventArgs;

namespace Abyss.Web.Commands.Discord;

public class QuoteCommand : BaseCommand
{
    private readonly IQuoteHelper _quoteHelper;

    public override string Command => "quote";

    public QuoteCommand(IServiceProvider serviceProvider, IQuoteHelper quoteHelper) : base(serviceProvider)
    {
        _quoteHelper = quoteHelper;
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
        if (clientUser == null)
        {
            await e.Message.RespondAsync("You are not registered");
            return;
        };
        if (!_userHelper.HasPermission(clientUser, Permissions.QuoteManager))
        {
            await e.Message.RespondAsync("You are not authorized");
            return;
        }
        var quote = args[0];
        var source = args[1];
        var result = await _quoteHelper.AddQuote(quote, source);
        if (!result.Success)
        {
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                await e.Message.RespondAsync("Failed to add quote");
            }
            else
            {
                await e.Message.RespondAsync(result.ErrorMessage);
            }
        }
        await WriteQuote(e, result.Quote);
    }

    private async Task GetQuote(MessageCreateEventArgs e)
    {
        var quote = await _quoteHelper.GetQuote();
        await WriteQuote(e, quote);
    }

    private async Task WriteQuote(MessageCreateEventArgs e, QuotePost quote)
    {
        await e.Message.RespondAsync(_quoteHelper.FormatQuote(quote));
    }
}
