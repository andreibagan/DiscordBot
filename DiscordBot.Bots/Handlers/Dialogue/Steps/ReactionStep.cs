﻿using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Handlers.Dialogue.Steps
{
    public class ReactionStep : DialogStepBase
    {
        private readonly Dictionary<DiscordEmoji, ReactionStepData> _options;

        private DiscordEmoji _selectedEmoji; 

        public ReactionStep(string content, Dictionary<DiscordEmoji, ReactionStepData> options) : base(content)
        {
            _options = options;
        }

        public override IDialoguaStep NextStep => _options[_selectedEmoji].NextStep;

        public Action<DiscordEmoji> OnValidResult { get; set; } = delegate { };

        public override async Task<bool> ProcessStep(DiscordClient client, DiscordChannel channel, DiscordUser user)
        {
            var cancelEmoji = DiscordEmoji.FromName(client, ":x:");

            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = "Пожалуйста, ответьте на это",
                Description = $"{user.Mention}, {_content}",
                Color = DiscordColor.Yellow,
            };

            embedBuilder.AddField("Для закрытия окна", "Выберите это эмоджи ':x:'");

            var interactivity = client.GetInteractivity();

            while (true)
            {
                var embed = await channel.SendMessageAsync(embed: embedBuilder).ConfigureAwait(false);

                onMessageAdded(embed);

                foreach (var emoji in _options.Keys)
                {
                    await embed.CreateReactionAsync(emoji).ConfigureAwait(false);
                }

                await embed.CreateReactionAsync(cancelEmoji).ConfigureAwait(false);

                var reactionResult = await interactivity.WaitForReactionAsync(x => _options.ContainsKey(x.Emoji) || x.Emoji == cancelEmoji, embed, user).ConfigureAwait(false);

                if (reactionResult.Result.Emoji == cancelEmoji)
                {
                    return true;
                }

                _selectedEmoji = reactionResult.Result.Emoji;

                OnValidResult(_selectedEmoji);

                return false;
            }
        }
    }

    public class ReactionStepData
    {
        public string Content { get; set; }
        public IDialoguaStep NextStep { get; set; }
    }
}
