using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using SafePoint.Data;
using SafePoint.Data.Entities;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using IdentityServer4.Models;

namespace SafePoint.Server
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
            services.AddControllers();

            services.AddDbContext<SafePointContext>(options =>
                    options.UseNpgsql(Configuration.GetConnectionString("SafePointContext")));

            services.AddDefaultIdentity<ApplicationUser>()
                .AddEntityFrameworkStores<SafePointContext>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

            });

            services.AddIdentityServer()
                      .AddDeveloperSigningCredential()
                      .AddAspNetIdentity<ApplicationUser>()
                      .AddInMemoryPersistedGrants()
                      .AddInMemoryClients(Config.GetClients())
                      .AddInMemoryIdentityResources(Config.GetIdentityResources())
                      .AddInMemoryApiResources(Config.GetApiResources())
                      .AddDefaultEndpoints();

            services.AddCors();

            services.AddAuthentication()
                .AddIdentityServerJwt();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseIdentityServer();

            app.UseAuthorization();

            app.UseCors(o=> o.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
