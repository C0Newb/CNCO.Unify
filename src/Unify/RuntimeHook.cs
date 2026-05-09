namespace CNCO.Unify;

/// <summary>
/// Hook that can run when <see cref="UnifyRuntime.Initialize"/> is called.
/// </summary>
public record RuntimeHook
{
  /// <summary>
  /// Name for the hook that is logged before being executed.
  /// Truncated to 75 characters.
  /// </summary>
  public required string Name { get; init; }

  /// <summary>
  /// Friendly description of the hook that is logged before being executed.
  /// Truncated to 250 characters.
  /// </summary>
  public string? Description { get; init; }

  /// <summary>
  /// The hook code that will be executed.
  /// </summary>
  public required Action<UnifyRuntime> Action { get; init; }
}
