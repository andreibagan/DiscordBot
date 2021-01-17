using DiscordBot.Attributes;
using DiscordBot.Core.Services.Parser;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Commands
{
    public class ParserCommands : BaseCommandModule
    {
        private readonly IParserService _parserService;

        public ParserCommands(IParserService parserService)
        {
            _parserService = parserService;
        }

        [Command("parse")]
        [Description("Парсинг ВК групп")]
        [RequireCategories(ChannelCheckMode.Any, "Text Channels")]
        [RequireRoles(RoleCheckMode.Any, "Банан", "Сатанист")]
        public async Task Parse(CommandContext ctx, string URL)
        {
            var News = _parserService.GetNews(URL);


            foreach (var message in News.Result)
            {
                var joinEmbed = new DiscordEmbedBuilder
                {
                    Description = message.Content,
                    Title = message.HeaderText,
                    ImageUrl = message.ContentUrl[0],
                    Color = DiscordColor.Gold,
                };

                // DiscordMessage a = new DiscordMessage();

                // foreach (var image in message.ContentUrl)
                //{
                //     joinEmbed.WithImageUrl(image);

                //}

                await ctx.Channel.SendMessageAsync(embed: joinEmbed).ConfigureAwait(false);
            }
        }
    }
}
