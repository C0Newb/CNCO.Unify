using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Capabilities.TestFramework;

namespace UnifyTestRunner.Browser;

public static partial class TestApplicationBuilderExtensions
{
  public static void AddNUnit(
    this ITestApplicationBuilder testApplicationBuilder,
    Func<IEnumerable<Assembly>> getTestAssemblies
  ) =>
    testApplicationBuilder.RegisterTestFramework(
      _ => new TestFrameworkCapabilities(),
      (capabilities, serviceProvider) => new NUnitTestFrameworkAdapter()
    );
}
