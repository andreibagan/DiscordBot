using System.Linq;
using System.Threading.Tasks;
using DiscordBot.Core.Services.Profiles;
using DiscordBot.DAL.Models.Profiles;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DiscordBot
{
    public class ProfileCommands : BaseCommandModule
    {
        private readonly IProfileService _profileService;

        public ProfileCommands(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [Command("profile")]
        [Description("Выводит профиль участника")]
        public async Task Profile(CommandContext ctx)
        {
            await GetProfileToDisplayAsync(ctx, ctx.Member.Id);
        }

        [Command("profile")]
        [Description("Выводит профиль участника по его id")]
        public async Task Profile(CommandContext ctx, [Description("ID участника")] DiscordMember member)
        {
            await GetProfileToDisplayAsync(ctx, member.Id);
        }

        private async Task GetProfileToDisplayAsync(CommandContext ctx, ulong memberId)
        {
            Profile profile = await _profileService.GetOrCreateProfileAsync(memberId, ctx.Guild.Id).ConfigureAwait(false);

            DiscordMember member = ctx.Guild.Members[profile.DiscordId];

            var profileEmbed = new DiscordEmbedBuilder
            {
                Title = $"Профиль: {member.DisplayName}",
                ImageUrl = member.AvatarUrl,
                Color = DiscordColor.Blurple,
            };

            profileEmbed.AddField("Level", profile.Level.ToString());
            profileEmbed.AddField("Xp", profile.Xp.ToString());
            profileEmbed.AddField("Gold", profile.Gold.ToString());

            if (profile.Items.Count > 0)
            {
                profileEmbed.AddField("Items", string.Join(", ", profile.Items.Select(x => x.Item.Name)));
            }

            await ctx.Channel.SendMessageAsync(embed: profileEmbed).ConfigureAwait(false);

            //GrantXpViewModel viewModel = await _experienceService.GrantXpAsync(memberId, ctx.Guild.Id, 100).ConfigureAwait(false);

            //if (!viewModel.LevelledUp)
            //{
            //    return;
            //}

            //var levelUpEmbed = new DiscordEmbedBuilder
            //{
            //    Title = $"У нашего дорогого участника '{member.DisplayName}' новый уровень!\nТекущий уровень: '{viewModel.Profile.Level}'",
            //    ImageUrl = member.AvatarUrl,
            //};

            //levelUpEmbed.AddField("Xp", profile.Xp.ToString());

            //await ctx.Channel.SendMessageAsync(embed: levelUpEmbed).ConfigureAwait(false);
        }
    }
}