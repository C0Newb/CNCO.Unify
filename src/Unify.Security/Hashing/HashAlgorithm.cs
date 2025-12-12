namespace CNCO.Unify.Security;

internal enum HashAlgorithm
{
  CRC16,
  CRC32,
  SHA1,
  SHA256,
  SHA384,
  SHA512,

  SHA3_256,
  SHA3_384,
  SHA3_512,

  BCrypt,
  PBKDF2,
  Argon2,
}
