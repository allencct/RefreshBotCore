using Hangfire;
using Hangfire.Console;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RefreshWeb.DataAccess;
using RefreshWeb.Jobs;
using RefreshWeb.Services;

namespace RefreshWeb
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
            services.AddTransient<DataService, DataService>();

            services.AddDbContext<EntityContext>(options => options.UseNpgsql("Host=localhost;Port=5432;Database=refresh;Username=postgres;Password=password"));

            //services.AddHangfire(config =>
            //{
            //    config.UsePostgreSqlStorage("Host=host.docker.internal;Port=5432;Database=refresh;Username=postgres;Password=password");
            //    config.UseConsole();
            //});
            //services.AddHangfireServer();

            services.AddControllersWithViews();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            //app.UseHangfireDashboard();
            //app.UseHangfireDashboard(options: new DashboardOptions
            //{
            //    Authorization = new[] { new DashboardNoAuthorizationFilter() },
            //    //AppPath = "/"
            //});
            //app.UseHangfireServer();
            //RecurringJob.AddOrUpdate<CheckTargetJob>("check-targets", j => j.ExecuteAsync(null), Cron.Minutely());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                //endpoints.MapControllerRoute(
                //    name: "default",
                //    pattern: "{controller}/{action=Index}/{id?}");
                //endpoints.MapHangfireDashboard();
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
