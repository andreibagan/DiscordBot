using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Threading.Tasks;

namespace DiscordBot.Handlers.Dialogue.Steps
{
    public class TextStep : DialogStepBase
    {
        private readonly int? _minLength;
        private readonly int? _maxLength;

        private IDialoguaStep _nextStep;

        public TextStep (string content, IDialoguaStep nextStep, int? minLength = null, int? maxLength = null) : base(content)
        {
            _nextStep = nextStep;
            _minLength = minLength;
            _maxLength = maxLength;
        }

        public Action<string> OnValidResult { get; set; } = delegate { };

        public override IDialoguaStep NextStep => _nextStep;

        public void SetNextStep (IDialoguaStep nextStep)
        {
            _nextStep = nextStep;
        }

        public override async Task<bool> ProcessStep(DiscordClient client, DiscordChannel channel, DiscordUser user)
        {
            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = "Пожалуйста ответьте на следующее:",
                Color = DiscordColor.Red,
                Description = $"{user.Mention}, {_content}",
            };

            embedBuilder.AddField("Для закрытия окна", "Используйте команду '!cancel'");

            if (_minLength.HasValue)
            {
                embedBuilder.AddField("Минимальная длина: ", $"{_minLength.Value} символов");
            }
            if (_maxLength.HasValue)
            {
                embedBuilder.AddField("Максимальная длина: ", $"{_maxLength.Value} символов");
            }

            var interactivity = client.GetInteractivity();

            while (true)
            {
                var embed = await channel.SendMessageAsync(embed: embedBuilder).ConfigureAwait(false);

                onMessageAdded(embed);

                var messageResult = await interactivity.WaitForMessageAsync(x => x.ChannelId == channel.Id && x.Author.Id == user.Id).ConfigureAwait(false);

                if (messageResult.Result == null)
                {
                    return false;
                    //Придумать что-тол интересное
                }

                onMessageAdded(messageResult.Result);

                if (messageResult.Result.Content.Equals("!cancel", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }


                if (_minLength.HasValue)
                {
                    if (messageResult.Result.Content.Length < _minLength.Value)
                    {
                        await TryAgain(channel, $"Ты ввёл ({_minLength.Value - messageResult.Result.Content.Length}) символов, слишком мало").ConfigureAwait(false);
                        continue;   
                    }
                }
                if (_maxLength.HasValue)
                {
                    if (messageResult.Result.Content.Length > _maxLength.Value)
                    {
                        await TryAgain(channel, $"Ты ввёл ({messageResult.Result.Content.Length - _maxLength.Value}) символов, слишком много").ConfigureAwait(false);
                        continue;
                    }
                }

                OnValidResult(messageResult.Result.Content);

                return false;
            }
        }
    }
}
