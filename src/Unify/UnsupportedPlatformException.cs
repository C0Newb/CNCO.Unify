using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CNCO.Unify;

public class UnsupportedPlatformException : Exception
{
  private UnsupportedPlatformException(string message)
    : base(message) { }

  [DoesNotReturn]
  public static void ThrowForType(Type type) =>
    throw new UnsupportedPlatformException($"Type '{type.Name}' is unsupported on this platform!");

  [DoesNotReturn]
  public static void ThrowFromMethodInType(
    Type classType,
    [CallerMemberName] string? methodName = ""
  ) => throw FromMethodInType(classType, methodName);

  public static UnsupportedPlatformException FromMethodInType(
    Type classType,
    string? methodName = ""
  ) =>
    throw new UnsupportedPlatformException(
      $"{classType.Name}::{methodName} is unsupported on this platform!"
    );

  [DoesNotReturn]
  public static void ThrowFromMethod([CallerMemberName] string? methodName = "") =>
    throw FromMethod(methodName);

  public static UnsupportedPlatformException FromMethod(string? methodName = "") =>
    new UnsupportedPlatformException($"Method '{methodName}' is unsupported on this platform!");

  [DoesNotReturn]
  public static void ThrowWithMessage(string message) => throw FromMessage(message);

  public static UnsupportedPlatformException FromMessage(string message) =>
    new UnsupportedPlatformException(message);
}
