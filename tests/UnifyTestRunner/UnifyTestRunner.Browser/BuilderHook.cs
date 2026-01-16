using System.Reflection;
using Avalonia;
using Avalonia.Browser;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Capabilities.TestFramework;

namespace UnifyTestRunner.Browser;

public static class BuilderHook
{
  public static void AddExtensions(ITestApplicationBuilder builder, string[] args)
  {
    builder.AddNUnit(() => [Assembly.GetEntryAssembly()!]);
    //Program.BuildAvaloniaApp().WithInterFont().StartBrowserAppAsync("out");
  }
}
