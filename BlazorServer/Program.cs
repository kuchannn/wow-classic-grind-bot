using Core;

using Frontend;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;

using System;
using System.IO;
using System.Threading;

namespace BlazorServer;

public static class Program
{
    public static void Main(string[] args)
    {
        while (true)
        {
            Log.Information($"[{nameof(Program),-17}] Starting WoW Classic Grind Bot BlazorServer");
            Log.Information($"[{nameof(Program),-17}] Server will be available at: http://localhost:5000");
            Log.Information($"[{nameof(Program),-17}] Chrome should open automatically");
            
            try
            {
                var host = CreateApp(args);
                var logger = host.Services.GetRequiredService<Microsoft.Extensions.Logging.ILogger>();

                AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs args) =>
                {
                    Exception e = (Exception)args.ExceptionObject;
                    logger.LogError(e, e.Message);
                };

                Log.Information($"[{nameof(Program),-17}] Server started successfully");
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Error($"[{nameof(Program),-17}] Server failed to start: {ex.Message}");
                Log.Information($"[{nameof(Program),-17}] Retrying in 3 seconds...");
                
                Thread.Sleep(3000);
            }
        }
    }

    private static WebApplication CreateApp(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders().AddSerilog();

        ConfigureServices(builder.Configuration, builder.Services);

        return ConfigureApp(builder, builder.Environment);
    }

    private static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        ILoggerFactory logFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders().AddSerilog();
        });

        services.AddLogging(builder =>
        {
            LoggerSink sink = new();
            builder.Services.AddSingleton(sink);

            const string outputTemplate = "[{@t:HH:mm:ss:fff} {@l:u1}] {#if Length(SourceContext) > 0}[{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1),-17}] {#end}{@m}\n{@x}";
            //const string outputTemplate = "[{@t:HH:mm:ss:fff} {@l:u1}] {SourceContext}] {@m}\n{@x}";

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .WriteTo.Sink(sink)
                .WriteTo.File(new ExpressionTemplate(outputTemplate),
                    "out.log",
                    rollingInterval: RollingInterval.Day)
                .WriteTo.Debug(new ExpressionTemplate(outputTemplate))
                .WriteTo.Console(new ExpressionTemplate(outputTemplate, theme: TemplateTheme.Literate))
                .CreateLogger();

            builder.Services.AddSingleton<Microsoft.Extensions.Logging.ILogger>(logFactory.CreateLogger(string.Empty));
        });

        Microsoft.Extensions.Logging.ILogger log = logFactory.CreateLogger("Program");

        log.LogInformation(
            $"{Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName} " +
            $"{DateTimeOffset.Now}");

        services.AddStartupConfigurations(configuration);

        services.AddWoWProcess(log);

        services.AddCoreBase();

        if (AddonConfig.Exists() && FrameConfig.Exists())
        {
            services.AddCoreNormal(log);
        }
        else
        {
            services.AddCoreConfiguration(log);
        }

        services.AddFrontend();

        services.AddCoreFrontend();

        services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateOnBuild = true });
    }

    private static WebApplication ConfigureApp(WebApplicationBuilder builder, IWebHostEnvironment env)
    {
        WebApplication app = builder.Build();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();

        DataConfig dataConfig = app.Services.GetRequiredService<DataConfig>();
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, dataConfig.Path)),
            RequestPath = "/path"
        });

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddAdditionalAssemblies(typeof(Frontend._Imports).Assembly);

        app.UseAntiforgery();

        return app;
    }

}