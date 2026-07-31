using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ParsWoW.Launcher.Configuration;
using ParsWoW.Launcher.Interfaces;
using ParsWoW.Launcher.Launcher;
using ParsWoW.Launcher.Services.Api;
using ParsWoW.Launcher.Services.Auth;
using ParsWoW.Launcher.Services.Expansion;
using ParsWoW.Launcher.Services.Navigation;
using ParsWoW.Launcher.Services.Theme;
using ParsWoW.Launcher.Services.Cache;
using ParsWoW.Launcher.Services.Localization;
using ParsWoW.Launcher.Services.Security;
using ParsWoW.Launcher.Services.Update;
using ParsWoW.Launcher.Services.Discord;
using ParsWoW.Launcher.Services.Music;
using ParsWoW.Launcher.Services.Animation;
using ParsWoW.Launcher.Services.Plugin;
using ParsWoW.Launcher.Services.Notifications;
using ParsWoW.Launcher.Services.Background;
using ParsWoW.Launcher.Services.Logging;
using ParsWoW.Launcher.ViewModels;
using ParsWoW.Launcher.Views;
using Serilog;

namespace ParsWoW.Launcher;

public partial class App
{
    private IServiceProvider _services = null!;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ConfigureLogging();
        _services = ConfigureServices();

        var launcherEngine = _services.GetRequiredService<LauncherEngine>();
        launcherEngine.InitializeAsync().GetAwaiter().GetResult();

        var mainWindow = _services.GetRequiredService<MainWindow>();
        Current.MainWindow = mainWindow;
        mainWindow.Show();
    }

    private static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "launcher-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Configuration
        services.AddSingleton<LauncherConfig>();

        // Logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });
        services.AddSingleton<LogService>();

        // HttpClient
        services.AddHttpClient("ParsWoWApi", client =>
        {
            client.BaseAddress = new Uri("https://localhost:49643");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // ===== CORE SERVICES =====
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IExpansionService, ExpansionService>();
        services.AddSingleton<LauncherEngine>();

        // ===== PART 1: API SERVICES =====
        services.AddSingleton<ApiClient>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IArmoryApiService, ArmoryApiService>();
        services.AddSingleton<INewsService, NewsService>();
        services.AddSingleton<IShopService, ShopService>();
        services.AddSingleton<IRealmService, RealmService>();
        services.AddSingleton<IDownloadService, DownloadService>();

        // ===== PART 3: SECURITY =====
        services.AddSingleton<ISecurityService, SecurityManager>();

        // ===== PART 3: LOCALIZATION =====
        services.AddSingleton<ILocalizationService, LocalizationService>();

        // ===== PART 3: CACHE =====
        services.AddSingleton<ICacheService, CacheService>();

        // ===== PART 3: AUTO-UPDATE =====
        services.AddSingleton<IUpdateService, AutoUpdateService>();

        // ===== PART 3: NOTIFICATIONS =====
        services.AddSingleton<INotificationService, NotificationService>();

        // ===== PART 3: DISCORD RPC =====
        services.AddSingleton<IDiscordService, DiscordRichPresence>();

        // ===== PART 3: MUSIC =====
        services.AddSingleton<IMusicService, MusicService>();

        // ===== PART 3: ANIMATIONS =====
        services.AddSingleton<IAnimationService, AnimationService>();

        // ===== PART 3: PLUGIN SYSTEM =====
        services.AddSingleton<IPluginService, PluginLoader>();

        // ===== PART 3: BACKGROUND SERVICES =====
        services.AddSingleton<BackgroundServiceManager>();

        // ===== VIEWMODELS =====
        services.AddTransient<MainViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<ForgotPasswordViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ArmoryViewModel>();
        services.AddTransient<ShopViewModel>();
        services.AddTransient<DownloadsViewModel>();
        services.AddTransient<AccountServicesViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<NewsViewModel>();

        // ===== VIEWS =====
        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
