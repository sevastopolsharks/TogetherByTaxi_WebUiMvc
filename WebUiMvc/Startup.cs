using System;
using IdentityServer4;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace WebUiMvc
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddIdentityServer()
                .AddInMemoryApiResources(Config.GetApis())
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryClients(Config.GetClients())
                .AddDeveloperSigningCredential(persistKey: false);
            services.AddAuthorization();
            var identityServerAuthenticationAuthority = GetStringFromConfig("IdentityServerAuthentication", "Authority");
            var identityServerAuthenticationApiName = GetStringFromConfig("IdentityServerAuthentication", "ApiName");

            services.AddAuthentication()
                .AddOpenIdConnect("aad", "Sign-in with Azure AD", options =>
                {
                    options.Authority = identityServerAuthenticationAuthority;
                    options.ClientId = identityServerAuthenticationApiName;

                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SignOutScheme = IdentityServerConstants.SignoutScheme;

                    options.ResponseType = "id_token";
                    options.CallbackPath = "/signin-aad";
                    options.SignedOutCallbackPath = "/signout-callback-aad";
                    options.RemoteSignOutPath = "/signout-aad";

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidAudience = "165b99fd-195f-4d93-a111-3e679246e6a9",

                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                })
                .AddIdentityServerAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = identityServerAuthenticationAuthority;
                    options.ApiName = identityServerAuthenticationApiName;
                    // options.RequireHttpsMetadata = false;//TODO: if we need https
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
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }

        private string GetStringFromConfig(string firstName, string secondName)
        {
            var result = Configuration[$"{firstName}:{secondName}"];
            if (string.IsNullOrEmpty(result))
            {
                result = Configuration[$"{firstName}_{secondName}"];
            }

            if (string.IsNullOrEmpty(result))
            {
                throw new Exception($"Configuration setting does not exist. Setting name {firstName}:{secondName}");
            }

            return result;
        }
    }
}
