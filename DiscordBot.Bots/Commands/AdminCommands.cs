using DiscordBot.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Commands
{
    public class AdminCommands : BaseCommandModule
    {
        [Command("ban")]
        [Description("Команда для бана участника")]
        [RequireCategories(ChannelCheckMode.Any, "Text Channels")]
        [RequireRoles(RoleCheckMode.Any, "Банан", "Сатанист")]
        public async Task Ban(CommandContext ctx, [Description("ID участника")] DiscordMember member, [Description("Количество в днях")] int timeIntdays = 0, [Description("Причина")] string reason = null)
        {
            if (member == null)
            {
                await ctx.Channel.SendMessageAsync($"Участник не был найден").ConfigureAwait(false);
                return;
            }
            await member.BanAsync(timeIntdays, reason).ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync($"Участник '{member.DisplayName}' был забанен на {timeIntdays} дней по причине '{reason}'").ConfigureAwait(false);
        }

        [Command("unban")]
        [Description("Команда для разбана участника")]
        [RequireCategories(ChannelCheckMode.Any, "Text Channels")]
        [RequireRoles(RoleCheckMode.Any, "Банан", "Сатанист")]
        public async Task Unban(CommandContext ctx, [Description("ID участника")] DiscordMember member, [Description("Причина")] string reason = null)
        {
            if (member == null)
            {
                await ctx.Channel.SendMessageAsync($"Участник не был найден").ConfigureAwait(false);
                return;
            }

            await member.UnbanAsync(reason).ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync($"Участник '{member.DisplayName}' был разбанен по причине '{reason}'").ConfigureAwait(false);
        }

        [Command("mute")]
        [Description("Команда для заглушения участника")]
        [RequireCategories(ChannelCheckMode.Any, "Text Channels")]
        [RequireRoles(RoleCheckMode.Any, "Банан", "Сатанист")]
        public async Task Mute(CommandContext ctx, [Description("ID участника")] DiscordMember member, [Description("Причина")] string reason = null)
        {
            if (member == null)
            {
                await ctx.Channel.SendMessageAsync($"Участник не был найден").ConfigureAwait(false);
                return;
            }

            await member.SetMuteAsync(true, reason).ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync($"Участник '{member.DisplayName}' был заглушен по причине '{reason}'").ConfigureAwait(false);
        }

        [Command("unmute")]
        [Description("Команда для снятия заглушения участника")]
        [RequireCategories(ChannelCheckMode.Any, "Text Channels")]
        [RequireRoles(RoleCheckMode.Any, "Банан", "Сатанист")]
        public async Task Unmute(CommandContext ctx, [Description("ID участника")] DiscordMember member, [Description("Причина")] string reason = null)
        {
            if (member == null)
            {
                await ctx.Channel.SendMessageAsync($"Участник не был найден").ConfigureAwait(false);
                return;
            }

            await member.SetMuteAsync(false, reason).ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync($"Участник '{member.DisplayName}' был разглушен по причине '{reason}'").ConfigureAwait(false);
        }
    }
}
