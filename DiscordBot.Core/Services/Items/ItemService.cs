using DiscordBot.Core.Services.Profiles;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.Items;
using DiscordBot.DAL.Models.Profiles;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBot.Core.Services.Items
{
    public interface IItemService
    {
        Task CreateNewItemAsync(Item item);
        Task<Item> GetItemByNameAsync(string itemName);
        Task<bool> PurchaseItemAsync(ulong discordId, ulong guildId, string itemName);
        Task<bool> PurchaseItemAsync(ulong discordId, ulong guildId, Item item);
        Task<List<Item>> GetItemsList();
        Task<bool> SellItemAsync(ulong discordId, ulong guildId, string itemName);
        Task<bool> SellItemAsync(ulong discordId, ulong guildId, Item item);
    }

    public class ItemService : IItemService
    {
        private readonly DbContextOptions<RPGContext> _options;
        private readonly IProfileService _profileService;

        public ItemService(DbContextOptions<RPGContext> options, IProfileService profileService)
        {
            _options = options;
            _profileService = profileService;
        }

        public async Task CreateNewItemAsync(Item item)
        {
            using var context = new RPGContext(_options);  

            context.Add(item);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<Item> GetItemByNameAsync(string itemName)
        {
            using var context = new RPGContext(_options);

            return await context.Items.Include(x => x.Items)
                .Include(x => x.Items).ThenInclude(x => x.Item)
                .FirstOrDefaultAsync(x => x.Name.ToLower() == itemName.ToLower()).ConfigureAwait(false);
            
        }

        public async Task<List<Item>> GetItemsList()
        {
            using var context = new RPGContext(_options);

            return await context.Items.ToListAsync().ConfigureAwait(false);
        }

        public async Task<bool> PurchaseItemAsync(ulong discordId, ulong guildId, string itemName)
        {
            using var context = new RPGContext(_options);

            Item item = await GetItemByNameAsync(itemName).ConfigureAwait(false);

            if (item == null)
            {
                return false;
            }

            Profile profile = await _profileService.GetOrCreateProfileAsync(discordId, guildId).ConfigureAwait(false);

            if (profile.Gold < item.Price)
            {
                return false;
            }

            if (profile.Items.Equals(new ProfileItem
            {
                ProfileId = profile.Id,
                ItemId = item.Id,
            }))
            {
                return false;
            }

            profile.Gold -= item.Price;
            profile.Xp += item.Price - (int)(item.Price * 0.25);

            context.Add(new ProfileItem
            {
                ItemId = item.Id,
                ProfileId = profile.Id,
            });

            context.Profiles.Update(profile);

            await context.SaveChangesAsync().ConfigureAwait(false);

            return true;
        }

        public async Task<bool> PurchaseItemAsync(ulong discordId, ulong guildId, Item item)
        {
            using var context = new RPGContext(_options);

            if (item == null)
            {
                return false;
            }

            Profile profile = await _profileService.GetOrCreateProfileAsync(discordId, guildId).ConfigureAwait(false);

            if (profile.Gold < item.Price)
            {
                return false;
            }

            if (profile.Items.Contains(new ProfileItem
            {
                ProfileId = profile.Id,
                ItemId = item.Id,
            }))
            {
                return false;
            }

            profile.Gold -= item.Price;
            profile.Xp += item.Price - (int)(item.Price * 0.25);

            context.Add(new ProfileItem
            {
                ItemId = item.Id,
                ProfileId = profile.Id,
            });

            context.Profiles.Update(profile);

            await context.SaveChangesAsync().ConfigureAwait(false);

            return true;
        }

        public async Task<bool> SellItemAsync(ulong discordId, ulong guildId, string itemName)
        {
            using var context = new RPGContext(_options);

            Item item = await GetItemByNameAsync(itemName).ConfigureAwait(false);

            if (item == null)
            {
                return false;
            }

            Profile profile = await _profileService.GetOrCreateProfileAsync(discordId, guildId).ConfigureAwait(false);

            var profileitem = await context.ProfileItems
                .FirstOrDefaultAsync(x => x.ProfileId == profile.Id && x.ItemId == item.Id).ConfigureAwait(false);

            if (!profile.Items.Contains(profileitem))
            {
                return false;
            }

            profile.Gold += item.Price - (int)(item.Price * 0.35);
            profile.Xp += item.Price - (int)(item.Price * 0.70);

            profile.Items.Remove(profileitem);//1
            context.Remove(profileitem);//2

            context.Profiles.Update(profile);//3

            await context.SaveChangesAsync().ConfigureAwait(false);

            return true;
        }

        public async Task<bool> SellItemAsync(ulong discordId, ulong guildId, Item item)
        {
            using var context = new RPGContext(_options);

            if (item == null)
            {
                return false;
            }

            Profile profile = await _profileService.GetOrCreateProfileAsync(discordId, guildId).ConfigureAwait(false);

            var profileitem = await context.ProfileItems
                .FirstOrDefaultAsync(x => x.ProfileId == profile.Id && x.ItemId == item.Id).ConfigureAwait(false);

            if (!profile.Items.Contains(profileitem))
            {
                return false;
            }

            profile.Gold += item.Price - (int)(item.Price * 0.35);
            profile.Xp += item.Price - (int)(item.Price * 0.70);

            profile.Items.Remove(profileitem);//1
            context.Remove(profileitem);//2

            context.Profiles.Update(profile);//3

            await context.SaveChangesAsync().ConfigureAwait(false);

            return true;
        }
    }
}
