using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Avalonia.ReactiveUI;
using UnifyTestRunner;

[assembly: SupportedOSPlatform("browser")]

namespace UnifyTestRunner.Browser
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      await AppBuilder.Configure<App>().WithInterFont().UseReactiveUI().StartBrowserAppAsync("out");
    }
  }
}
