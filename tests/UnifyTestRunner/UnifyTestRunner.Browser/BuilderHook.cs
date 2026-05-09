using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Configurations;

namespace UnifyTestRunner.Browser;

public static class BuilderHook
{
#pragma warning disable TPEXP // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
  public static void AddExtensions(ITestApplicationBuilder builder, string[] args)
  {
    builder.AddNUnit();
    builder.Configuration.AddConfigurationSource(() =>
      new SimpleInMemoryConfigurationSource(
        new Dictionary<string, string>
        {
          // Empty is fine – the key point is avoiding disk access
          // Add real values if needed later
          ["testingPlatform:environment"] = "browser",
        }
      )
    );
    Program.BuildAvaloniaApp().WithInterFont().StartBrowserAppAsync("out");
  }

  public class SimpleInMemoryConfigurationSource(IReadOnlyDictionary<string, string> values)
    : IConfigurationSource
  {
    public int Order => 0;

    public string Uid => "UnifyTestRunner.Browser.TestConfigurationSource";

    public string Version => "1.0.0";

    public string DisplayName => "UnifyTestRunner.Browser Configuration Source";

    public string Description => "Unify Test Runner Browser Test Configuration Source";

    public Task<IConfigurationProvider> BuildAsync(CommandLineParseResult commandLineParseResult) =>
      Task.FromResult<IConfigurationProvider>(new SimpleInMemoryConfigurationProvider(values));

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);
  }

  public sealed class SimpleInMemoryConfigurationProvider(
    IReadOnlyDictionary<string, string> values
  ) : IConfigurationProvider
  {
    public Task LoadAsync() => Task.CompletedTask;

    public bool TryGet(string key, out string? value) => values.TryGetValue(key, out value);
  }
#pragma warning restore TPEXP // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
}
