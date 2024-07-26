using Abyss.Web.Helpers.Interfaces;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using System.ComponentModel;

namespace Abyss.Web.Commands.Discord;

[Command("quote"), Description("Add or view quotes")]
public class QuoteCommand(IServiceProvider serviceProvider, IQuoteHelper quoteHelper) : BaseCommand(serviceProvider)
{
    private readonly IQuoteHelper _quoteHelper = quoteHelper;

    [Command("get"), Description("Get a random Abyss quote")]
    public async Task GetQuote(CommandContext ctx)
    {
        await ctx.DeferResponseAsync();
        var quote = await _quoteHelper.GetQuote();
        await ctx.EditResponseAsync(_quoteHelper.FormatQuote(quote));
    }

    [Command("add"), Description("Add a quote")]
    public async Task AddQuote(CommandContext ctx, [Description("Quote text")] string quote, [Description("Quote author")] string author)
    {
        await ctx.DeferResponseAsync();

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

    [Command("Add Quote"), SlashCommandTypes(DiscordApplicationCommandType.MessageContextMenu)]
    public async Task AddQuoteFromMessage(SlashCommandContext ctx, DiscordMessage message)
    {
        await ctx.DeferResponseAsync();

        if (!await CheckPermission(ctx, Data.Permissions.QuoteManager))
        {
            return;
        }

        var quote = message.Content;
        var author = message.Author;
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