using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace DiscordBot.Handlers.Dialogue.Steps
{
    public interface IDialoguaStep
    {
        Action<DiscordMessage> onMessageAdded { get; set; }
        IDialoguaStep NextStep { get; }
        Task<bool> ProcessStep(DiscordClient client, DiscordChannel channel, DiscordUser user);
    }
}
