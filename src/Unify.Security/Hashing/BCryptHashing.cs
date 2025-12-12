using System.Runtime.InteropServices;
using System.Security;

namespace CNCO.Unify.Security;

public static class BCryptHashing
{
  /// <summary>
  /// Generates a BCrypt hash from a string given a work factor.
  /// </summary>
  /// <param name="password">Data to hash</param>
  /// <param name="workFactor">BCrypt work factor</param>
  /// <returns>The hashed password.</returns>
  public static string GenerateHash(SecureString password, int workFactor)
  {
    nint bstr = Marshal.SecureStringToBSTR(password);
    try
    {
      return BCrypt.Net.BCrypt.EnhancedHashPassword(Marshal.PtrToStringBSTR(bstr), workFactor);
    }
    finally
    {
      Marshal.ZeroFreeBSTR(bstr);
    }
  }

  /// <inheritdoc cref="GenerateHash(SecureString, int)"/>
  public static string GenerateHash(string password, int workFactor) => BCrypt.Net.BCrypt.EnhancedHashPassword(password, workFactor);


  /// <summary>
  /// Used to verify a BCrypt hash was derived from a given password
  /// </summary>
  /// <param name="password">Data to verify</param>
  /// <param name="hash">Hash to verify against</param>
  /// <returns>Whether the hash is generated from the password.</returns>
  public static bool GenerateHash(SecureString password, string hash)
  {
    nint bstr = Marshal.SecureStringToBSTR(password);
    try
    {
      return BCrypt.Net.BCrypt.EnhancedVerify(Marshal.PtrToStringBSTR(bstr), hash);
    }
    finally
    {
      Marshal.ZeroFreeBSTR(bstr);
    }
  }

  /// <inheritdoc cref="GenerateHash(SecureString, string)"/>
  public static bool GenerateHash(string password, string hash) => BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
}