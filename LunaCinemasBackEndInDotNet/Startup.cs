﻿using LunaCinemasBackEndInDotNet.BusinessLogic;
using LunaCinemasBackEndInDotNet.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LunaCinemasBackEndInDotNet.Persistence;

namespace LunaCinemasBackEndInDotNet
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.Configure<LunaCinemasDatabaseSettings>(
                Configuration.GetSection(nameof(LunaCinemasDatabaseSettings)));

            services.AddSingleton<ILunaCinemasDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<LunaCinemasDatabaseSettings>>().Value);

            services.AddSingleton<FilmService>();
            services.AddSingleton<ReviewFilter>();
            services.AddSingleton<CommentFilter>();
            services.AddSingleton<ShowingService>();
            services.AddSingleton<BookingService>();
            services.AddSingleton<IShowingContext, ShowingContext>();
            services.AddSingleton<ICommentContext, CommentContext>();
            services.AddSingleton<IReviewContext, ReviewContext>();
            services.AddSingleton<IFilmContext, FilmContext>();
            services.AddSingleton<InitialisationHandler>();

            services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:3000").AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseCors(MyAllowSpecificOrigins); 

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
