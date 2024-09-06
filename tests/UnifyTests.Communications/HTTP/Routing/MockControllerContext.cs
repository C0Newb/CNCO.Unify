using CNCO.Unify.Communications.Http;
using CNCO.Unify.Communications.Http.Routing;
using Moq;

namespace UnifyTests.Communications.Http.Routing {
    internal class MockControllerContext : IControllerContext {
        private Mock<IWebRequest> _webRequestMock;
        private Mock<IWebResponse> _webResponseMock;

        internal string LastResponseData = string.Empty;

        public IWebRequest WebRequest {
            get => _webRequestMock.Object;
        }

        public IWebResponse WebResponse {
            get => _webResponseMock.Object;
        }

        public MockControllerContext() {
            _webRequestMock = new Mock<IWebRequest>();
            _webResponseMock = new Mock<IWebResponse>();

            _webRequestMock.SetupAllProperties();
            _webResponseMock.SetupAllProperties();

            _webResponseMock.Setup(m => m.Send(It.IsAny<string>()))
                .Callback<string>(data => LastResponseData = data);
        }
    }
}
