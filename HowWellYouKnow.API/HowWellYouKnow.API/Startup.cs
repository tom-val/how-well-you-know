using System;
using System.ComponentModel.DataAnnotations;
using HowWellYouKnow.API.Hubs;
using HowWellYouKnow.API.Services;
using HowWellYouKnow.Domain.Dtos;
using HowWellYouKnow.Infrastructure;
using HowWellYouKnow.Infrastructure.Repositories;
using HowWellYouKnow.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HowWellYouKnow.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.AddSignalR();

            services.AddScoped<GameRepository>();
            services.AddScoped<GameReadService>();
            services.AddScoped<GameService>();
            services.AddScoped<UserRepository>();
            services.AddScoped<UserService>();
            services.AddScoped<QuestionRepository>();
            services.AddScoped<QuestionsService>();
            services.AddScoped<GameStatusService>();
            services.AddScoped<AnswerRepository>();
            services.AddScoped<AnswerResultRepository>();
            services.AddScoped<GuessRepository>();
            services.AddScoped<AnswerService>();

            services.AddDbContext<DatabaseContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler(appBuilder =>
            {
                appBuilder.Run(async context => {
                    var ex = context.Features.Get<IExceptionHandlerPathFeature>();
                    if (ex?.Error is NullReferenceException)
                        context.Response.StatusCode = 404;
                    else if (ex?.Error is InvalidOperationException)
                        context.Response.StatusCode = 400;
                    else if (ex?.Error is ValidationException)
                        context.Response.StatusCode = 400;
                    else
                        context.Response.StatusCode = 500;

                    context.Response.ContentType = "application/json";

                    ApiError error = new ApiError()
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = ex?.Error.Message
                    };

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(error, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() })).ConfigureAwait(false);
                });
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<QuestionsHub>("/questions");
                endpoints.MapHub<GameStateHub>("/gameState");
                endpoints.MapHub<UserHub>("/users");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });

            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetService<DatabaseContext>().Database.Migrate();
            }
        }
    }
}
