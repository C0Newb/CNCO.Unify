namespace CNCO.Unify.Notifications;

public static class UnifyRuntimeExtensions
{
  /// <summary>
  /// Attach the <see cref="NotificationRuntime"/> to the <see cref="UnifyRuntime"/>.
  /// </summary>
  /// <param name="unifyRuntime">Instance to link to.</param>
  /// <returns>Runtime instance.</returns>
  public static UnifyRuntime UseNotificationRuntime(this UnifyRuntime unifyRuntime)
  {
    var communicationsRuntime = NotificationRuntime.Create();
    unifyRuntime.AddRuntimeLink(communicationsRuntime);
    return unifyRuntime;
  }
}
