using DiscordBot.DAL;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System.Threading.Tasks;

namespace DiscordBot
{
    public class RolesCommands : BaseCommandModule
    {

        public RolesCommands(RPGContext context)
        {
        }

        [Command("join")]
        [Description("Приглашает в команду программистов, а также автоматически выдаёт роль 'Программист'")]
        public async Task Join(CommandContext ctx)
        {
            var joinEmbed = new DiscordEmbedBuilder
            {
                Title = "Хочешь присоединиться?)",
                ImageUrl = ctx.Client.CurrentUser.AvatarUrl,
                Color = DiscordColor.Gold
            };

            var joinMessage = await ctx.Channel.SendMessageAsync(embed: joinEmbed).ConfigureAwait(false);

            var thumbsUpEmoji = DiscordEmoji.FromName(ctx.Client, ":+1:");

            var thumbsDownEmoji = DiscordEmoji.FromName(ctx.Client, ":-1:");

            await joinMessage.CreateReactionAsync(thumbsUpEmoji).ConfigureAwait(false);
            await joinMessage.CreateReactionAsync(thumbsDownEmoji).ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();

            var reactionResult = await interactivity.WaitForReactionAsync(x => x.Message == joinMessage &&
                                                    x.User == ctx.User &&
                                                    (x.Emoji == thumbsDownEmoji || x.Emoji == thumbsUpEmoji)).ConfigureAwait(false);

            if (reactionResult.Result.Emoji == thumbsUpEmoji)
            {
                var role = ctx.Guild.GetRole(715895125695266926);
                await ctx.Member.GrantRoleAsync(role).ConfigureAwait(false);
            }
            //else if (reactionResult.Result.Emoji == thumbsDownEmoji)
            //{

            //}
            //else
            //{

            //} 

            await joinMessage.DeleteAsync().ConfigureAwait(false);
        }
    }
}