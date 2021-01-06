using DiscordBot.Attributes;
using DiscordBot.Core.Services.Items;
using DiscordBot.DAL.Models.Items;
using DiscordBot.Handlers.Dialogue;
using DiscordBot.Handlers.Dialogue.Steps;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    public class ItemCommands : BaseCommandModule
    {
        private readonly IItemService _itemService;

        public ItemCommands(IItemService itemService)
        {
            _itemService = itemService;
        }

        [Command("createitem")]
        [Description("Команда создания предмета")]
        [RequireCategories(ChannelCheckMode.Any, "Text Channels")]
        [RequireRoles(RoleCheckMode.Any, "Банан", "Сатанист")]
        public async Task CreateItem(CommandContext ctx)
        {
            var itemPriceStep = new IntStep("Сколько стоит данный предмет?", null, 1);
            var itemDescriptionStep = new TextStep("Какое будет описание у предмета?", itemPriceStep);
            var itemNameStep = new TextStep("Как бы ты хотел назвать этот предмет?)", itemDescriptionStep);

            var item = new Item();

            itemNameStep.OnValidResult += (result) => item.Name = result;
            itemDescriptionStep.OnValidResult += (result) => item.Description = result;
            itemPriceStep.OnValidResult += (result) => item.Price = result;

            //var userChannel = await ctx.Member.CreateDmChannelAsync().ConfigureAwait(false);

            var inputDialogueHandler = new DialoguaHandler(
                ctx.Client,
                ctx.Channel, 
                ctx.User, 
                itemNameStep);

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            if (!succeeded)
            {
                return;
            }


            await _itemService.CreateNewItemAsync(item).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync($"Предмет '{item.Name}' был успешно создан!").ConfigureAwait(false);
        }

        [Command("iteminfo")]
        [Description("Основная информация об выбранном предмете")]
        public async Task ItemInfo(CommandContext ctx)
        {
            var itemNameStep = new TextStep("Какой предмет вас интересует?", null);

            string itemName = string.Empty;

            itemNameStep.OnValidResult += (result) => itemName = result;

            //var userChannel = await ctx.Member.CreateDmChannelAsync().ConfigureAwait(false);

            var inputDialogueHandler = new DialoguaHandler(ctx.Client, ctx.Channel, ctx.User, itemNameStep);

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            if (!succeeded)
            {
                return;
            }

            Item item = await _itemService.GetItemByNameAsync(itemName).ConfigureAwait(false);
                         
            if (item == null)
            {
                await ctx.Channel.SendMessageAsync($"Такого предмета ({itemName}) нету");
                return;
            }

            await ctx.Channel.SendMessageAsync($"Название: {item.Name}\nОписание: {item.Description}\nЦена: {item.Price}").ConfigureAwait(false);
        }

        [Command("buy")]
        [Description("Покупка предмета")]
        public async Task Buy(CommandContext ctx, [Description("Название предмета")] params string[] itemNameSplit)
        {
            string itemName = string.Join(' ', itemNameSplit);

            var item = await _itemService.GetItemByNameAsync(itemName).ConfigureAwait(false);

            bool succees = await _itemService.PurchaseItemAsync(ctx.Member.Id, ctx.Guild.Id, item).ConfigureAwait(false);

            if (!succees)
            {
                await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention}, не удалось купить данный предмет, т. к. он у вас есть или у вас недостаточно золота").ConfigureAwait(false);
                return;
            }
            await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} купил '{item.Name}' за {item.Price} золота").ConfigureAwait(false);
        }


        [Command("items")]
        [Description("Информация обо всех действующих предметах")]
        public async Task Items(CommandContext ctx)
        {
            List<Item> items = await _itemService.GetItemsList().ConfigureAwait(false);

            //var ItemsEmbed = new DiscordEmbedBuilder
            //{
            //    Title = $"Список всех предметов",
            //    ImageUrl = "https://www.google.com/url?sa=i&url=https%3A%2F%2Fzen.yandex.ru%2Fmedia%2Fgames_anime%2Ftopovye-vesci-dlia-animeshnika-s-ali-5dd4c99ef4882e4380662593&psig=AOvVaw1wZyhfpZb_7HXTWO0dmI1c&ust=1591443072467000&source=images&cd=vfe&ved=0CAIQjRxqFwoTCIjfl9DJ6ukCFQAAAAAdAAAAABAD",
            //    Color = DiscordColor.Blurple,
            //};

            //ItemsEmbed.AddField("Количество предметов", items.Count.ToString());
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var item in items)
            {
                stringBuilder.Append($"Название: {item.Name}\nОписание: {item.Description}\nЦена: {item.Price}\n\n");
            }

            await ctx.Channel.SendMessageAsync(stringBuilder.ToString()).ConfigureAwait(false);
            //if (profile.Items.Count > 0)
            //{
            //    profileEmbed.AddField("Items", string.Join(", ", profile.Items.Select(x => x.Item.Name)));
            //}

            //await ctx.Channel.SendMessageAsync(embed: ItemsEmbed).ConfigureAwait(false);

        }
        [Command("sellitem")]
        [Description("Продажа предмета")]
        [RequireRoles(RoleCheckMode.Any, "Банан", "Программист")]
        public async Task SellItem(CommandContext ctx, [Description("Название предмета")] params string[] itemNameSplit)
        {
            string itemName = string.Join(' ', itemNameSplit);

            Item item = await _itemService.GetItemByNameAsync(itemName).ConfigureAwait(false);

            bool succees = await _itemService.SellItemAsync(ctx.Member.Id, ctx.Guild.Id, item).ConfigureAwait(false);

            if (!succees)
            {
                await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention}, не удалось продать данный предмет, т. к. такого предмета у вас нету").ConfigureAwait(false);
                return;
            }
            await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} продал '{item.Name}'\n{ctx.Member.Mention} получил {item.Price - (int)(item.Price * 0.35)} золота").ConfigureAwait(false);
        }
    }
}