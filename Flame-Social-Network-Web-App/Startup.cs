using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flame_Social_Network_Web_App.Data;
using Flame_Social_Network_Web_App.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Flame_Social_Network_Web_App
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
            services.AddOptions();

            services.AddDbContext<AppDbContext>(config =>
            {
                config.UseSqlServer(Configuration.GetConnectionString("Default"));
            });

            services.AddIdentity<IdentityUser, IdentityRole>(config =>
            {
                // Password
                config.Password.RequiredLength = Constants.MinimumPasswordLength;
                config.Password.RequiredUniqueChars = 1;
                config.Password.RequireDigit = true;
                config.Password.RequireNonAlphanumeric = false; // because we do not want to force the user the use a too complex password(even if it's better to do so)
                config.Password.RequireUppercase = false; // same here

                // Email
                config.SignIn.RequireConfirmedEmail = false; // it will be false for now
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options => // this is how we add the default login path
            {
                options.LoginPath = Constants.DefaultLoginPath;
            });

            services.AddAuthentication("CookieAuth")
                .AddCookie("CookieAuth", config =>
                {
                    config.Cookie.Name = "Flame.Cookie";
                    config.LoginPath = Constants.DefaultLoginPath;
                })
                .AddJwtBearer(options => {
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"]; // this implements the authorization over the hubs
                            if (string.IsNullOrEmpty(accessToken) == false)
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            //services.AddSingleton<RuntimeRepositoryService>();

            services.AddSignalR(o =>
            {
                o.EnableDetailedErrors = true; // used for debugging purpose
            });

            services.AddAuthorization();

            services.AddControllersWithViews();

            services.Configure<AppSettings>(Configuration);
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/chatHub");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Newsfeed}/{action=Index}/{id?}");
            });
        }
    }
}
