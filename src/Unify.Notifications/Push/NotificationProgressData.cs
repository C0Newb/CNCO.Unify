namespace CNCO.Unify.Notifications.Push;

/// <summary>
/// Progress bar properties.
/// </summary>
public class NotificationProgressData
{
  /// <summary>
  /// Current position.
  /// </summary>
  public int Value { get; set; }

  /// <summary>
  /// Max value for <see cref="Value"/>.
  /// </summary>
  public int MaxValue { get; set; }

  /// <summary>
  /// Progress bar is in a indeterminate state.
  /// </summary>
  /// <remarks>
  /// Buffering is an example of when this should be <see langword="true"/>.
  /// </remarks>
  public bool IsIndeterminate { get; set; } = false;

  /// <summary>
  /// Current status of the operation which is displayed underneath the progress bar.
  /// </summary>
  /// <remarks>
  /// This should reflect what is currently happening, such as "Downloading..." or "Installing..."
  /// </remarks>
  public string? Status { get; set; }

  /// <summary>
  /// Optional title string.
  /// </summary>
  public string? Title { get; set; }

  /// <summary>
  /// Optional string to be displayed instead of the default percentage string.
  /// </summary>
  /// <remarks>
  /// If not provided, the current percentage, calculated by <see cref="Value"/>, will be displayed.
  /// </remarks>
  public string? DisplayedValue { get; set; }


  public NotificationProgressData(int? value = 0, int? max = 0)
  {
    Value = value ?? 0;
    MaxValue = max ?? 100;
  }

  /// <summary>
  /// Calculates the percentage of the given value relative to the maximum value.
  /// </summary>
  /// <returns>The percentage of the given value relative to the maximum value.</returns>
  public double GetPercentage()
  {
    if (Value == MaxValue)
      return 1;
    return (double)Math.Min(Value, MaxValue) / Math.Max(MaxValue, Value);
  }
}
