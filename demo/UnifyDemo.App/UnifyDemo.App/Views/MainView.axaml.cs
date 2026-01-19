using System;
using System.Security;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CNCO.Unify.Communications.Http;
using CNCO.Unify.Communications.Mdns;
using CNCO.Unify.Notifications;
using CNCO.Unify.Notifications.Push;
using CNCO.Unify.Notifications.Push.Eventing;

namespace UnifyDemo.App.Views;

public partial class MainView : UserControl
{
  private int count = 0;

  public MainView()
  {
    InitializeComponent();

    try
    {
      var router = new Router(false);
      App.WebServer = new WebServer(router, new WebServerOptions() { Port = 8008 });

      try
      {
        App.WebServer.Start();
      }
      catch (SecurityException)
      {
        App.Log.Error("Failed to start webserver due to access violation.");
        App.Log.Info($"Attempted to bind to: {string.Join(", ", App.WebServer.GetEndpoints())}");
      }

      router.Any(
        "test",
        (request, response) =>
        {
          response.Status(299);
          response.Send("test");
        }
      );
    }
    catch (Exception ex)
    {
      App.Log.Error("Error initializing web server:");
      App.Log.Error(ex.Message);
    }
  }

  private void BtnSave_Click(object? sender, RoutedEventArgs e)
  {
    SendNotification();
  }

  private void SendNotification()
  {
    var notification = new PushNotification("Test Notification")
    {
      Contents = new()
      {
        Text = $"Howdy, from hell!{(count <= 0 ? string.Empty : $" Count: {count}")}",
      },
    };

    notification.NotificationActivated += (
      IPushNotification pushNotification,
      NotificationActivationArguments args
    ) => SendNotification();

    _ = NotificationRuntime.NotificationManager.SendAsync(notification);
    count++;
  }
}
