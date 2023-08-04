using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

[assembly: HostingStartup(typeof(ChatService.Api.Infra.Authorization.AuthorizationHostingStartup))]
namespace ChatService.Api.Infra.Authorization
{
    public class AuthorizationHostingStartup : IHostingStartup
    {
        /// <inheritdoc />
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((_, services) =>
            {
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateAudience = false,
                            ValidateIssuer = false,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = "pluto",
                            ValidAudience = "12312",
                            ClockSkew = TimeSpan.FromMinutes(30),
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("715B59F3CDB1CF8BC3E7C8F13794CEA9"))
                        };
                        options.RequireHttpsMetadata = false;
                        options.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context =>
                            {
                                var accessToken = context.Request.Query["access_token"];

                                // If the request is for our hub...
                                var path = context.HttpContext.Request.Path;
                                if (!string.IsNullOrEmpty(accessToken) &&
                                    (path.StartsWithSegments("/hubs/chat")))
                                {
                                    // Read the token out of the query string
                                    context.Token = accessToken;
                                }
                                return Task.CompletedTask;
                            }
                        };
                    }); // 认证
                services.AddAuthorization();
                services.AddSingleton<IAuthorizationPolicyProvider, DynamicAuthorizationPolicyProvider>();
                services.AddScoped<IAuthorizationHandler, PermissionRequirementHandler>();
            });
        }
    }
}