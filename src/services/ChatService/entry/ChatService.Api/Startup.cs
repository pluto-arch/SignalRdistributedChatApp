using Dncy.Tools;
using ChatService.Api.BackgroundServices;
using ChatService.Api.Infra.ApiDoc;
using ChatService.Api.Infra.LogSetup;

using ChatService.Application;
using ChatService.Domain;
using ChatService.Infra;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Primitives;
using System.Threading.RateLimiting;
using ChatService.Api.Hubs;
using Dncy.EventBus.RabbitMQ;
using ChatService.Infra.Constants;
using System.Security.Cryptography;

namespace ChatService.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            #region 缓存
            services.AddMemoryCache(options =>
            {
                options.SizeLimit = 10240;
            });
            #endregion

            services.AddHttpClient();
            services.AddApplicationModule(Configuration);
            services.AddInfraModule(Configuration);
            services.AddDomainModule();
            #region background service
            services.AddHostedService<PrductBackgroundService>();
            #endregion


            #region 速率限制
            services.AddRateLimiter(options =>
            {
                //options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                //    RateLimitPartition.GetFixedWindowLimiter(
                //        partitionKey: httpContext.Connection.RemoteIpAddress?.ToNumber().ToString(),
                //        factory: partition => new FixedWindowRateLimiterOptions
                //        {
                //            AutoReplenishment = true,
                //            PermitLimit = 50,
                //            QueueLimit = 10,
                //            Window = TimeSpan.FromMinutes(1)
                //        }));

                // action or controller use [EnableRateLimiting("Api")] to enable this policy
                options.AddPolicy("home.RateLimit_action", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(httpContext.Connection.RemoteIpAddress?.ToNumber().ToString(),
                        partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 2,
                            Window = TimeSpan.FromSeconds(10)
                        }));

            });
            #endregion

            services.AddSignalR(options =>
            {
            });

            services.AddSingleton<RSACryptoServiceProvider>(s=>new RSACryptoServiceProvider(2048));
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {

            var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
            var address = serverAddressesFeature?.Addresses;
            Log.Logger.Information("应用程序运行地址: {@Address}. net version:{version}. 环境:{envName}. HostName:{hostName}", address, Environment.Version,env.EnvironmentName,EnvironmentConstants.EnvHostName);
            app.ApplicationServices.StartBasicConsume();

            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);

            app.UseResponseCompression();
            app.UseForwardedHeaders()
                .UseCertificateForwarding();

            //if (env.IsEnvironment(AppConstant.EnvironmentName.DEV))
            {
                app.UseCustomSwagger();
            }

            app.UseHttpRequestLogging();

            app.UseCors(AppConstant.DEFAULT_CORS_NAME);

            if (env.IsEnvironment(AppConstant.EnvironmentName.DEV))
            {
                app.UseDeveloperExceptionPage();
                // 初始化种子数据
                app.DataSeederAsync().Wait();
            }
            else
            {
                app.UseExceptionHandle();
                // TODO Notice: UseHsts, UseHttpsRedirection are not necessary if using reverse proxy with ssl, like nginx with ssl proxy
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            // 用户访问器
            app.UseCurrentUserAccessor();
            app.UseRouting();
            app.UseRateLimiter();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/hubs/chat");
                endpoints.MapSystemHealthChecks();
                endpoints.MapControllers();
            });

        }
    }
}

