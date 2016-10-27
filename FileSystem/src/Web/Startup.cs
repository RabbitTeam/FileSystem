using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Web.Services.FileProvider;
using Web.Services.FileProvider.Jkzl;

namespace Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            switch (Configuration["FileProvider"])
            {
                case "JkzlFileProvider":
                    services.AddSingleton<IFileInfoRespoistory, FileInfoRespoistory>();
                    services.AddSingleton<IFileProvider, JkzlFileProvider>();
                    break;

                case "LocalFileProvider":
                    services.AddSingleton<IFileProvider, LocalFileProvider>();
                    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                    break;
            }

            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.UseDeveloperExceptionPage();
            //            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}