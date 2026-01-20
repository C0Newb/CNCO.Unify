using System;
using System.Resources;
using System.Security;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CNCO.Unify.Communications.Http;
using CNCO.Unify.Notifications;
using CNCO.Unify.Notifications.Push;
using CNCO.Unify.Notifications.Push.Actions;
using CNCO.Unify.Notifications.Push.Eventing;
using CNCO.Unify.Notifications.Push.Imaging;

namespace UnifyDemo.App.Views;

public partial class MainView : UserControl
{
  private int count = 0;
  private IPushNotification _notification;

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

  private void BtnSend_Click(object? sender, RoutedEventArgs e)
  {
    try
    {
      SendNotification();
    }
    catch (Exception ex)
    {
      App.Log.Error("Error sending notification!", ex);
    }
  }

  private void BtnSendWithProgressbar_Click(object? sender, RoutedEventArgs e)
  {
    try
    {
      SendNotification(true);
    }
    catch (Exception ex)
    {
      App.Log.Error("Error sending notification!", ex);
    }
  }

  private void BtnClearAll_Click(object? sender, RoutedEventArgs e)
  {
    try
    {
      NotificationRuntime.NotificationManager.ClearAll();
    }
    catch (Exception ex)
    {
      App.Log.Error("Error clearing notifications!", ex);
    }
  }

  private void SendNotification(bool showProgressBar = false)
  {
    _ = _notification?.Cancel();

    var textBox = new NotificationTextBox() { Hint = "Type something...", Title = "Text message" };

    var button1 = new NotificationButton() { Contents = "Click me!" };
    button1.ActionActivated += (action, value) =>
      Dispatcher.UIThread.InvokeAsync(() => lbl.Content = $"Button '1' clicked! Value: {value}");
    var button2 = new NotificationButton() { Contents = "Send", TextBox = textBox };
    button2.ActionActivated += (action, value) =>
      Dispatcher.UIThread.InvokeAsync(() => lbl.Content = $"Button '2' clicked! Value: {value}");

    var comboBox = new NotificationComboBox()
    {
      Choices = ["Option 1", "Option 2", "Option 3"],
      SelectedIndex = 0,
      Hint = "Choose an option...",
    };

    var icon = Properties.Resources.NotificationIcon;
    var image = Properties.Resources.NotificationImage;

    _notification = new PushNotification("Test Notification")
    {
      Contents = new()
      {
        Text = $"Howdy, from hell!{(count <= 0 ? string.Empty : $" Count: {count}")}",
        Actions = [textBox, button1, button2, comboBox],
        Image = new NotificationImage(image) { AlternativeText = "Logon background photo." },
        AttributionText = "CNCO.Unify Demo App",
        Icon = new NotificationImage(icon) { AlternativeText = "CNCO.Unify Logo" },
      },
    };

    if (showProgressBar)
    {
      _notification.Contents.ProgressData = new NotificationProgressData()
      {
        Title = "Progress Example",
        Status = "Downloading...",
        IsIndeterminate = true,
        MaxValue = 100,
        Value = 45,
        DisplayedValue = "45%",
      };
    }

    _notification.NotificationActivated += (pushNotification, args) => SendNotification();

    _notification.NotificationDismissed += (pushNotification, reason) =>
      Dispatcher.UIThread.InvokeAsync(() => lbl.Content = $"Notification dismissed: {reason}");

    _notification.NotificationFailed += (pushNotification, reason, details) =>
      Dispatcher.UIThread.InvokeAsync(() =>
        lbl.Content = $"Notification failed: {reason} - {details}"
      );

    _ = _notification.SendAsync();
    _ = Dispatcher.UIThread.Invoke(() => lbl.Content = $"Notification {count} activated!");
    count++;
  }
}
