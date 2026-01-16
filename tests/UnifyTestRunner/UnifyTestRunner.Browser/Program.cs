using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using UnifyTestRunner;

internal sealed partial class Program
{
  public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>();
}
