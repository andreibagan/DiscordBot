using DiscordBot.Handlers.Dialogue.Steps;
using DSharpPlus;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBot.Handlers.Dialogue
{
    class DialoguaHandler
    {
        private readonly DiscordClient _client;
        private readonly DiscordChannel _channel;
        private readonly DiscordUser _user;
        private IDialoguaStep _currentStep;

        public DialoguaHandler(DiscordClient client, DiscordChannel channel, DiscordUser user, IDialoguaStep startingStep)
        {
            _client = client;
            _channel = channel;
            _user = user;
            _currentStep = startingStep;
        }

        private readonly List<DiscordMessage> messages = new List<DiscordMessage>();

        public async Task<bool> ProcessDialogue()
        {
            while (_currentStep != null)
            {
                _currentStep.onMessageAdded += (message) => messages.Add(message);

                bool cancled = await _currentStep.ProcessStep(_client, _channel, _user).ConfigureAwait(false);

                if (cancled)
                {
                    await DeleteMessages().ConfigureAwait(false);

                    var cancelEmbed = new DiscordEmbedBuilder
                    {
                        Title = "Данное диалоговое окно было успешно закрыто",
                        Description = _user.Mention,
                        Color = DiscordColor.Aquamarine
                    };

                    await _channel.SendMessageAsync(embed: cancelEmbed).ConfigureAwait(false);
                     
                    return false;
                }

                _currentStep = _currentStep.NextStep;
            }
            await DeleteMessages().ConfigureAwait(false);

            return true;
        }

        private async Task DeleteMessages()
        {
            if (_channel.IsPrivate)
            {
                return;
            }

            foreach (var message in messages)
            {
                await message.DeleteAsync().ConfigureAwait(false);
            }
        }
    }
}
