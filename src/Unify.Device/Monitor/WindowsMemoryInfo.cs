using System.Runtime.InteropServices;

namespace CNCO.Unify.Device.Monitor;

internal class WindowsMemoryInfo
{
  public static MemoryStatus GetStatus()
  {
    MemoryStatusEx memStatus = new MemoryStatusEx();
    // Crucial: Initialize dwLength to the size of the structure
    memStatus.dwLength = (uint)Marshal.SizeOf(typeof(MemoryStatusEx));

    if (GlobalMemoryStatusEx(ref memStatus))
    {
      return new()
      {
        MemoryLoad = memStatus.dwMemoryLoad,
        TotalPhysical = memStatus.ullTotalPhys / 1024 / 1024,
        AvailablePhysical = memStatus.ullAvailPhys / 1024 / 1024,
      };
    }
    return new();
  }

  public sealed record MemoryStatus
  {
    public uint MemoryLoad { get; init; }
    public ulong TotalPhysical { get; init; }
    public ulong AvailablePhysical { get; init; }
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
  private struct MemoryStatusEx
  {
    public uint dwLength;
    public uint dwMemoryLoad;
    public ulong ullTotalPhys;
    public ulong ullAvailPhys;
    public ulong ullTotalPageFile;
    public ulong ullAvailPageFile;
    public ulong ullTotalVirtual;
    public ulong ullAvailVirtual;
    public ulong ullAvailExtendedVirtual;
  }

  // Import the native GlobalMemoryStatusEx function
  [return: MarshalAs(UnmanagedType.Bool)]
  [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
  private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer);
}
