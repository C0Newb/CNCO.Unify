using CNCO.Unify.Communications.Http;
using CNCO.Unify.Communications.Http.Routing;
using Moq;

namespace UnifyTests.Communications.Http.Routing;

internal class MockControllerContext : IControllerContext
{
  private readonly Mock<IWebRequest> _webRequestMock;
  private readonly Mock<IWebResponse> _webResponseMock;
  private readonly Mock<IWebSocket> _webSocketMock;

  internal string LastResponseData = string.Empty;

  public IWebRequest WebRequest
  {
    get => _webRequestMock.Object;
  }

  public IWebResponse WebResponse
  {
    get => _webResponseMock.Object;
  }

  public IWebSocket WebSocket
  {
    get => _webSocketMock.Object;
  }

  public MockControllerContext()
  {
    _webRequestMock = new Mock<IWebRequest>();
    _webResponseMock = new Mock<IWebResponse>();
    _webSocketMock = new Mock<IWebSocket>();

    _webRequestMock.SetupAllProperties();
    _webResponseMock.SetupAllProperties();
    _webSocketMock.SetupAllProperties();

    _webResponseMock
      .Setup(m => m.Send(It.IsAny<string>()))
      .Callback<string>(data => LastResponseData = data);
  }
}
