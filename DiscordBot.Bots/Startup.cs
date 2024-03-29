﻿using DiscordBot.Core.Services.Items;
using DiscordBot.Core.Services.Parser;
using DiscordBot.Core.Services.Profiles;
using DiscordBot.DAL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace DiscordBot.Bots
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<RPGContext>(options =>
            {
                options.UseSqlServer(@"Server=.\SQLEXPRESS;Database=RPGContext;Trusted_Connection=True;MultipleActiveResultSets=True",
                    x => x.MigrationsAssembly("DiscordBot.DAL.Migrations"));

                options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
            });

            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IExperienceService, ExperienceService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IParserService, ParserService>();

            var serviceProvider = services.BuildServiceProvider();

            var bot = new Bot(serviceProvider);
            services.AddSingleton(bot);

            var servs = services.ToList();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }
    }
}
