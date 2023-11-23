using Abyss.Web.Helpers;
using Abyss.Web.Helpers.Interfaces;
using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace Abyss.Web.Commands.Discord;

[SlashCommandGroup("quote", "Add or view quotes")]
public class QuoteCommand(IServiceProvider serviceProvider, IQuoteHelper quoteHelper) : BaseCommand(serviceProvider)
{
    private readonly IQuoteHelper _quoteHelper = quoteHelper;

    [SlashCommand("get", "Get a random Abyss quote")]
    public async Task GetQuote(InteractionContext ctx)
    {
        await ctx.CreateDeferredResponseAsync();
        var quote = await _quoteHelper.GetQuote();
        await ctx.EditResponseAsync(_quoteHelper.FormatQuote(quote));
    }

    [SlashCommand("add", "Add a quote")]
    public async Task AddQuote(InteractionContext ctx, [Option("quote", "Quote text")] string quote, [Option("author", "Quote author")] string author)
    {
        await ctx.CreateDeferredResponseAsync();

        if (!await CheckPermission(ctx, Data.Permissions.QuoteManager))
        {
            return;
        }

        var result = await _quoteHelper.AddQuote(quote, author);
        if (result.Success)
        {
            await ctx.EditResponseAsync(_quoteHelper.FormatQuote(result.Quote));
        }
        else
        {
            await ctx.EditResponseAsync(result.ErrorMessage);
        }
    }

    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Add Quote")]
    public async Task AddQuoteFromMessage(ContextMenuContext ctx)
    {
        await ctx.CreateDeferredResponseAsync();

        if (!await CheckPermission(ctx, Data.Permissions.QuoteManager))
        {
            return;
        }

        var quote = ctx.TargetMessage.Content;
        var author = ctx.TargetMessage.Author;
        var member = await ctx.Guild.GetMemberAsync(author.Id);

        var result = await _quoteHelper.AddQuote(quote, member.DisplayName);
        if (result.Success)
        {
            await ctx.EditResponseAsync(_quoteHelper.FormatQuote(result.Quote));
        }
        else
        {
            await ctx.EditResponseAsync(result.ErrorMessage);
        }
    }
}