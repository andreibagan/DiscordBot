using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace DiscordBot.Handlers.Dialogue.Steps
{
    public abstract class DialogStepBase : IDialoguaStep
    {
        protected readonly string _content;

        public DialogStepBase(string content)
        {
            _content = content;
        }

        public Action<DiscordMessage> onMessageAdded { get; set; } = delegate { };

        public abstract IDialoguaStep NextStep { get; }

        public abstract Task<bool> ProcessStep(DiscordClient client, DiscordChannel channel, DiscordUser user);

        protected async Task TryAgain (DiscordChannel channel, string problem)
        {
            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = "Пожалуйста, повторите снова",
                Color = DiscordColor.Red,
            };

            embedBuilder.AddField("Ошибка связана с вашим прошлым вводом", problem);

            var embed = await channel.SendMessageAsync(embed: embedBuilder).ConfigureAwait(false);

            onMessageAdded(embed);
        }
    }
}
