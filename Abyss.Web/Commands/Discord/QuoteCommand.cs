using Abyss.Web.Helpers.Interfaces;
using DontPanic.TumblrSharp.Client;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace Abyss.Web.Commands.Discord;

[SlashCommandGroup("quote", "Add or view quotes")]
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

    [SlashCommand("get", "Get a random Abyss quote")]
    public async Task RunGetQuote(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
        var quote = await _quoteHelper.GetQuote();
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(_quoteHelper.FormatQuote(quote)));
    }

    [SlashCommand("add", "Add a quote")]
    public async Task RunAddQuote(InteractionContext ctx, [Option("quote", "Quote text")] string quote, [Option("author", "Quote author")] string author)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var clientUser = await GetClientUser(ctx.User);
        if (!_userHelper.HasPermission(clientUser, Data.Permissions.QuoteManager))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not authorized"));
            return;
        }

        var result = await _quoteHelper.AddQuote(quote, author);
        if (result.Success)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(_quoteHelper.FormatQuote(result.Quote)));
        }
        else
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(result.ErrorMessage));
        }
    }

    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Add Quote")]
    public async Task AddQuoteFromMessage(ContextMenuContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var clientUser = await GetClientUser(ctx.User);
        if (!_userHelper.HasPermission(clientUser, Data.Permissions.QuoteManager))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not authorized"));
            return;
        }

        var quote = ctx.TargetMessage.Content;
        var author = ctx.TargetMessage.Author.Username;

        var result = await _quoteHelper.AddQuote(quote, author);
        if (result.Success)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(_quoteHelper.FormatQuote(result.Quote)));
        }
        else
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(result.ErrorMessage));
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
        if (!_userHelper.HasPermission(clientUser, Data.Permissions.QuoteManager))
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