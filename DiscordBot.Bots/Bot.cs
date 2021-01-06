using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using System.IO;
using DiscordBot.Commands;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Interactivity;
using System;
using DiscordBot.Bots.Commands;

namespace DiscordBot
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        public Bot(IServiceProvider services)
        {
            var json = string.Empty;

            using (var fs = File.OpenRead("config.json"))
            {
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                {
                    json = sr.ReadToEnd();
                }
            }

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true,
            };

            Client = new DiscordClient(config);

            Client.Ready += OnClientReady;

            Client.GuildRoleCreated += Client_GuildRoleCreated;

            Client.GuildRoleUpdated += Client_GuildRoleUpdated;

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(1)
            });

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.Prefix },
                EnableDms = false,
                EnableMentionPrefix = true,
                DmHelp = true,
                Services = services,
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<SimpleCommands>();
            Commands.RegisterCommands<RolesCommands>();
            Commands.RegisterCommands<ItemCommands>();
            Commands.RegisterCommands<ProfileCommands>(); 
            Commands.RegisterCommands<AdminCommands>();

            Client.ConnectAsync();
        }

        private Task Client_GuildRoleUpdated(GuildRoleUpdateEventArgs e)
        {
            Console.WriteLine("Updated ROLE!");
            Console.WriteLine($"Роль {e.RoleBefore.Name} была изменена на {e.RoleAfter.Name}");
            return Task.CompletedTask;
        }

        private Task Client_GuildRoleCreated(GuildRoleCreateEventArgs e)
        {
            Console.WriteLine("Created new ROLE!");
            return Task.CompletedTask;
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            Console.WriteLine("DiscordBot is Ready!");
            return Task.CompletedTask;
        }
    }
}
