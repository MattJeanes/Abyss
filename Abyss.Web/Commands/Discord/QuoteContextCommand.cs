using Abyss.Web.Helpers.Interfaces;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace Abyss.Web.Commands.Discord;

public class QuoteContextCommand(IServiceProvider serviceProvider, IQuoteHelper quoteHelper) : BaseCommand(serviceProvider)
{
    private readonly IQuoteHelper _quoteHelper = quoteHelper;

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
