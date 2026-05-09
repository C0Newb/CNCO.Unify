using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CNCO.Unify;
using CNCO.Unify.Communications;
using CNCO.Unify.Communications.Http;
using CNCO.Unify.Logging;
using CNCO.Unify.Notifications;
using CNCO.Unify.Security;
using UnifyDemo.App.ViewModels;
using UnifyDemo.App.Views;

namespace UnifyDemo.App;

public partial class App : Application
{
  public static IWebServer? WebServer;
  public static ILogger Log => UnifyRuntime.ApplicationLog;

  public override void Initialize() => AvaloniaXamlLoader.Load(this);

  public static void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
  {
    WebServer?.Dispose();
    _ = UnifyRuntime.Current.ShutdownAsync().GetAwaiter();
  }

  public override void OnFrameworkInitializationCompleted()
  {
    UnifyRuntime? runtime = null;
    try
    {
      // Unify setup
      runtime = UnifyRuntime
        .Create("Unify.DemoApp")
        .UseCommunicationsRuntime()
        .UseNotificationRuntime()
        .UseSecurityRuntime();

      if (Log is SinkLogger sink)
      {
        sink.AddLogger(new EmergencyLogEventHanlder());
      }
    }
    catch
    {
      // ignore for now ...
    }

    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      desktop.Exit += OnExit;

      // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
      // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
      DisableAvaloniaDataAnnotationValidation();
      desktop.MainWindow = new MainWindow { DataContext = new MainViewModel() };
    }
    else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
    {
      singleViewPlatform.MainView = new MainView { DataContext = new MainViewModel() };
    }

    try
    {
      runtime?.Initialize();
    }
    catch
    {
      // ignore for now ...
    }
    base.OnFrameworkInitializationCompleted();
  }

  private static void DisableAvaloniaDataAnnotationValidation()
  {
    // Get an array of plugins to remove
    var dataValidationPluginsToRemove = BindingPlugins
      .DataValidators.OfType<DataAnnotationsValidationPlugin>()
      .ToArray();

    // remove each entry found
    foreach (var plugin in dataValidationPluginsToRemove)
    {
      _ = BindingPlugins.DataValidators.Remove(plugin);
    }
  }
}
