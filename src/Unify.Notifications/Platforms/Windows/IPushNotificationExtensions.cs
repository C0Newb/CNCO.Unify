#if WINDOWS_TOAST_NOTIFICATIONS
using CNCO.Unify.Notifications.Push.Actions;
using System.Text;

namespace CNCO.Unify.Notifications.Platforms.Windows;

public static class INotificationActionExtensions
{
  public const string ActionDelimiter = "|";

  public static string GetWindowsComponentId(this INotificationAction action)
  {
    byte[] bytes = Encoding.UTF8.GetBytes(action.Id);
    return $"{action.GetType().Name}{ActionDelimiter}{Convert.ToBase64String(bytes)}";
  }
}
#endif
