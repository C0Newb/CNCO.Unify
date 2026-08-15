using System;
using CNCO.Unify.Communications.Http.Routing;

namespace UnifyDemo.App.Web;

[Controller("test")]
public class TestController : Controller
{
  [HttpAny]
  [HttpGet("/{info}")]
  public string Test(string? info) =>
    $"This is a test .. with {(string.IsNullOrEmpty(info) ? "no info" : info)}";

  [HttpAny("multiple/{guid}/{number}/{string}")]
  public string TestWithMultiple(Guid guid, int number, string @string) =>
    $"This is a test with multiple parameters: guid={guid}, number={number}, string={@string}";
}
