using Abyss.Web.Commands.Discord.Interfaces;
using DSharpPlus.EventArgs;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Abyss.Web.Commands.Discord
{
    public class RegisterCommand : IDiscordCommand
    {
        public string Command => "register";

        private Regex _steamId64Regex = new Regex("^\\d{17}$");

        public async Task ProcessMessage(MessageCreateEventArgs e, List<string> args)
        {
            var steamId64 = args.FirstOrDefault();
            if (string.IsNullOrEmpty(steamId64) || !_steamId64Regex.IsMatch(steamId64)) {
                await e.Message.RespondAsync("Please enter your 64 bit SteamID (SteamId64) - you can find it here: https://steamid.io/");
                return;
            }
            await e.Message.RespondAsync($"Mate you want some registering: {steamId64}");
        }
    }
}
