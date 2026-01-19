using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Extensions.TestFramework;

namespace UnifyTestRunner.Browser;

public static partial class TestApplicationBuilderExtensions
{
  internal class NUnitTestFrameworkAdapter : ITestFramework
  {
    public string Uid => "unify-nunit-browser";
    public string Version => "1.0.0";
    public string DisplayName => "NUnit Browser Adapter";
    public string Description => DisplayName;

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    public Task<CreateTestSessionResult> CreateTestSessionAsync(CreateTestSessionContext context)
    {
      Debugger.Break();
      return Task.FromResult(new CreateTestSessionResult() { IsSuccess = true });
    }

    public Task ExecuteRequestAsync(ExecuteRequestContext context)
    {
      context.Complete();
      Debugger.Break();
      return Task.CompletedTask;
    }

    public Task<CloseTestSessionResult> CloseTestSessionAsync(CloseTestSessionContext context)
    {
      Debugger.Break();
      return Task.FromResult(new CloseTestSessionResult() { IsSuccess = true });
    }
  }
}
