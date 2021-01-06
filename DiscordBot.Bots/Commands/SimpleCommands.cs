using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordBot.Attributes;
using DiscordBot.Core.Services.Profiles;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.Profiles;
using DiscordBot.Handlers.Dialogue;
using DiscordBot.Handlers.Dialogue.Steps;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands
{
    public class SimpleCommands : BaseCommandModule
    {
        private readonly IProfileService _profileService;
        private readonly DbContextOptions<RPGContext> _options;


        public SimpleCommands(DbContextOptions<RPGContext> options, IProfileService profileService) 
        {
            _options = options;
            _profileService = profileService;
        }

        [Command("ping")]
        [Description("Отвечает 'Pong'")]
        [RequireRoles(RoleCheckMode.All)]
        [RequireCategories(ChannelCheckMode.Any, "Text Channels")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);
        }

        [Command("plus")]
        [Description("Складывает массив целых или дробных чисел")]
        [RequireRoles(RoleCheckMode.All)]
        [RequireCategories(ChannelCheckMode.Any, "Text Channels")]
        public async Task Plus(CommandContext ctx, [Description("Массив чисел через пробел")] params double[] numbers)
        {
            await ctx.Channel.SendMessageAsync("Ответ: " + (numbers.Sum()).ToString()).ConfigureAwait(false);
        }

        [Command("getdate")]
        [Description("Выдаёт текущую дату")]
        [RequireRoles(RoleCheckMode.All)]
        [RequireCategories(ChannelCheckMode.Any, "Text Channels")]
        public async Task GetDate(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(ctx.Member.DisplayName.ToString() + ", текущая дата: " + DateTime.Now.ToString()).ConfigureAwait(false);
        }

        [Command("say")]
        [Description("Повторяет сказанное")]
        [RequireRoles(RoleCheckMode.All)]
        [RequireCategories(ChannelCheckMode.Any, "Text Channels")]
        public async Task Say(CommandContext ctx, [Description("Текст, который нужно повторить")] params string[] text)
        {
            await ctx.Channel.SendMessageAsync($"{String.Join(" ", text)}").ConfigureAwait(false);
        }
        //[Не работает]
        [Command("say")]
        [Description("Повторяет сказанное")]
        [RequireRoles(RoleCheckMode.All)]
        [RequireCategories(ChannelCheckMode.Any, "Text Channels")]
        public async Task Say(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(message.Result.Content);
        }

        [Command("repeatmessage")]
        [Description("Повторяет за тобой (Сообщением)")]
        [RequireRoles(RoleCheckMode.Any, "Банан", "Программист")]
        [RequireCategories(ChannelCheckMode.Any, "Text Channels")]
        public async Task RepeatMessage(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(message.Result.Content);
        }
        //[Переделать в сервисы]
        [Command("quiz")]
        [Description("Викторина")]
        [RequireRoles(RoleCheckMode.Any, "Банан")]
        [RequireCategories(ChannelCheckMode.Any, "Text Channels")]
        public async Task Quiz(CommandContext ctx, int gold, int minvalue = 10, int maxvalue = int.MaxValue)
        {
            using var context = new RPGContext(_options);

            Random random = new Random();

            var interactivity = ctx.Client.GetInteractivity();


            int value1 = random.Next(minvalue, maxvalue);

            int value2 = random.Next(minvalue, maxvalue);

            int value = value1 + value2;

            await ctx.Channel.SendMessageAsync($"Сколько будет {value1} + {value2}?\nКто первый ответит получит {gold} золота").ConfigureAwait(false);

            while (true)
            {
                var message = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false);

                int result;

                if (!Int32.TryParse(message.Result.Content, out result))
                {
                    continue;
                }

                Profile profile = await _profileService.GetOrCreateProfileAsync(message.Result.Author.Id, ctx.Guild.Id).ConfigureAwait(false);

                if (Convert.ToInt32(message.Result.Content) != value)
                {
                    await ctx.Channel.SendMessageAsync($"Участник {message.Result.Author.Mention} ответил неверно!\nШтраф: 1 золотой").ConfigureAwait(false);

                    profile.Gold -= 1;

                    context.Profiles.Update(profile);

                    await context.SaveChangesAsync().ConfigureAwait(false);
                    continue;
                }

     

                profile.Gold += gold;

                context.Profiles.Update(profile);

                await context.SaveChangesAsync().ConfigureAwait(false);

                await ctx.Channel.SendMessageAsync($"Победитель: {message.Result.Author.Username}\n{message.Result.Author.Mention} выйграл {gold} золота\nПравильный ответ: {value}");

                return;
            }
        }
        //[Не работает]
        [Command("repeatemoji")]
        [Description("Повторяет за тобой (Emoji)")]
        [RequireRoles(RoleCheckMode.Any, "Банан", "Программист")]
        [RequireCategories(ChannelCheckMode.Any, "Text Channels")]
        public async Task RepeatEmoji(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForReactionAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(message.Result?.Emoji);
        }
        //[Доработать сервисы?]
        [Command("poll")]
        [Description("Создаёт голосовалку")]
        [RequireRoles(RoleCheckMode.Any, "Банан", "Программист")]
        [RequireCategories(ChannelCheckMode.Any, "Text Channels")]
        public async Task Poll(CommandContext ctx, [Description("Продолжительность голосования")] TimeSpan duration, [Description("Emojies")] params DiscordEmoji[] emojiOptions)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var options = emojiOptions.Select(x => x.ToString());

            var pollEmbed = new DiscordEmbedBuilder
            {
                Title = "Poll",
                Description = string.Join(" ", options),
            };

            var pollMessage =  await ctx.Channel.SendMessageAsync(embed: pollEmbed).ConfigureAwait(false);

            foreach (var option in emojiOptions)
            {
                await pollMessage.CreateReactionAsync(option).ConfigureAwait(false);
            }

            var result = await interactivity.CollectReactionsAsync(pollMessage, duration).ConfigureAwait(false);

            var distinctResult = result.Distinct();

            var results = result.Select(x => $"{x.Emoji}: {x.Total}");

            await ctx.Channel.SendMessageAsync(string.Join("\n", results)).ConfigureAwait(false);
        }
        //[Почему добавляет 0? cancel добавить в конец]
        [Command("dialogue")]
        [Description("Повторяет твоё сообщение в общий чат")]
        [RequireRoles(RoleCheckMode.Any, "Банан", "Программист")]
        [RequireCategories(ChannelCheckMode.Any, "Text Channels")]
        public async Task Dialogue(CommandContext ctx)
        {
            var inputStep = new TextStep("Скажи что-нибудь интересное!", null, 1, 100);
            var funnyStep = new IntStep("АхАх, весело!, введите число", null, minValue: 1, maxValue: 100);

            string input = string.Empty;

            int value = 0;

            inputStep.OnValidResult += (result) =>
            {
                input = result;

                if (result == "something interesting")
                {
                    inputStep.SetNextStep(funnyStep);
                }
            };

            funnyStep.OnValidResult += (result) => value = result;

            var userChannel = await ctx.Member.CreateDmChannelAsync().ConfigureAwait(false);

            var inputDialogueHandler = new DialoguaHandler(ctx.Client, userChannel, ctx.User, inputStep);

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            if (!succeeded)
            {
                return;
            }

            await ctx.Channel.SendMessageAsync(input).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(value.ToString()).ConfigureAwait(false);
        }

        [Command("emojidialogue")]
        [Description("Повторяет за тобой (Emoji)")]
        [RequireRoles(RoleCheckMode.Any, "Банан", "Программист")]
        [RequireCategories(ChannelCheckMode.Any, "Text Channels")]
        public async Task EmojiDialogue(CommandContext ctx)
        {
            var yesStep = new TextStep ("Ты выбрал 'Да'", null);
            var noStep = new TextStep("Ты выбрал 'Нет'", null);

            var emojiStep = new ReactionStep("Да или нет?", new Dictionary<DiscordEmoji, ReactionStepData>
            {
                {DiscordEmoji.FromName(ctx.Client, ":thumbsup:"), new ReactionStepData {Content = "Это как 'Да'", NextStep = yesStep} },
                {DiscordEmoji.FromName(ctx.Client, ":thumbsdown:"), new ReactionStepData {Content = "Это как 'Нет'", NextStep = noStep} }
            });

            var userChannel = await ctx.Member.CreateDmChannelAsync().ConfigureAwait(false);

            var inputDialogueHandler = new DialoguaHandler(ctx.Client, userChannel, ctx.User, emojiStep);

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            if (!succeeded)
            {
                return;
            }
        }
    }
}
