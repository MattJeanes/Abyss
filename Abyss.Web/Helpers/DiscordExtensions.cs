using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace Abyss.Web.Helpers;

public static class DiscordExtensions
{
    public static async Task<DiscordMessage> EditResponseAsync(this BaseContext ctx, string message)
    {
        return await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(message));
    }

    public static async Task CreateDeferredResponseAsync(this BaseContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
    }

    public static async Task CreateResponseAsync(this BaseContext ctx, string message)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(message));
    }

    public static async Task<DiscordMessage> CreateFollowupMessageAsync(this BaseContext ctx, string message)
    {
        return await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent(message));
    }
}
