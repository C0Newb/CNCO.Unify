using CNCO.Unify.Communications.Http;
using System.Text.Json;
using HttpVerb = CNCO.Unify.Communications.Http.HttpVerb;

namespace UnifyTests.Communications.Http.Routing {
    [TestFixture]
    public class ControllerTests {
        private Router Router { get; set; }
        private TestController Controller { get; set; } = new TestController();
        private MockControllerContext Context => (MockControllerContext)Controller.Context;

        private IWebResponse Response => Controller.Context.WebResponse;

        [OneTimeSetUp]
        public void SetUp() {
            Router = new Router(true);

            // Something, maybe
            Controller.Context = new MockControllerContext();
        }

        private static WebRequest GetWebRequest(string route, HttpVerb method, System.Net.Cookie[]? cookies = null) {
            Uri uri = new Uri($"http://localhost:1234/testcontroller/{route.TrimStart('/')}");
            WebRequest request = new WebRequest(uri, cookies ?? []) {
                Verb = method
            };

            return request;
        }

        #region Route methods
        [Test]
        public void Controller_All_IsHandled() {
            IEnumerable<HttpVerb> methods = [
                HttpVerb.Connect,
                HttpVerb.Delete,
                HttpVerb.Get,
                HttpVerb.Head,
                HttpVerb.Options,
                HttpVerb.Patch,
                HttpVerb.Put,
                HttpVerb.Trace,
            ];

            var request = GetWebRequest("all", HttpVerb.Get);
            foreach (HttpVerb method in methods) {
                string expectedResponse = $"all-{method.ToString().ToLower()}";

                request.Verb = method;
                Router.Process(request, Response);

                string actualResponse = Context.LastResponseData.ToLower();
                Assert.That(actualResponse, Is.EqualTo(expectedResponse));
            }
        }

        [Test]
        [TestCase(HttpVerb.Connect)]
        [TestCase(HttpVerb.Connect, true)]
        [TestCase(HttpVerb.Delete)]
        [TestCase(HttpVerb.Delete, true)]
        [TestCase(HttpVerb.Get)]
        [TestCase(HttpVerb.Get, true)]
        [TestCase(HttpVerb.Head)]
        [TestCase(HttpVerb.Head, true)]
        [TestCase(HttpVerb.Options)]
        [TestCase(HttpVerb.Options, true)]
        [TestCase(HttpVerb.Patch)]
        [TestCase(HttpVerb.Patch, true)]
        [TestCase(HttpVerb.Post)]
        [TestCase(HttpVerb.Post, true)]
        [TestCase(HttpVerb.Put)]
        [TestCase(HttpVerb.Put, true)]
        [TestCase(HttpVerb.Trace)]
        [TestCase(HttpVerb.Trace, true)]
        public void Controller_SpecificMethod_IsHandled(HttpVerb method, bool useMethodAsRoute = false) {
            string expectedResponse = method.ToString().ToLower();

            string route = useMethodAsRoute ? method.ToString().ToLower() : "";
            var request = GetWebRequest(route, method);
            Router.Process(request, Response);

            string actualResponse = Context.LastResponseData.ToLower();
            Assert.That(actualResponse, Is.EqualTo(expectedResponse));
        }
        #endregion

        #region Route parameters
        [Test]
        [TestCase(ParameterType.String)]
        [TestCase(ParameterType.String, true)]
        [TestCase(ParameterType.UShort)]
        [TestCase(ParameterType.Int)]
        [TestCase(ParameterType.Int, true)]
        [TestCase(ParameterType.Decimal)]
        [TestCase(ParameterType.Double)]
        [TestCase(ParameterType.Float)]
        [TestCase(ParameterType.Long)]
        [TestCase(ParameterType.BigInteger)]
        [TestCase(ParameterType.DateTime)]
        [TestCase(ParameterType.DateTime, true)]
        [TestCase(ParameterType.Guid)]
        [TestCase(ParameterType.Guid, true)]
        public void RouteParameter_SingleParameter_RespondsWithParameter(ParameterType parameterType, bool useCurlyBrace = false) {
            string parameterValue = "";
            string routePrefix = useCurlyBrace? "curly/" : "";
            switch (parameterType) {
                case ParameterType.String:
                    parameterValue = "abc123-test";
                    break;
                case ParameterType.UShort:
                    parameterValue = (ushort.MaxValue - 1).ToString();
                    routePrefix = "ushort/";
                    break;
                case ParameterType.Int:
                    parameterValue = (int.MaxValue - 1).ToString();
                    routePrefix = "int/";
                    break;
                case ParameterType.Decimal:
                    parameterValue = "1234567890987654321.12399";
                    routePrefix = "decimal/";
                    break;
                case ParameterType.Double:
                    parameterValue = "123456.12345";
                    routePrefix = "double/";
                    break;
                case ParameterType.Float:
                    parameterValue = "123456.125";
                    routePrefix = "float/";
                    break;
                case ParameterType.Long:
                    parameterValue = (long.MaxValue-1).ToString();
                    routePrefix = "long/";
                    break;
                case ParameterType.BigInteger:
                    parameterValue = "12345678909876543210123456789098765432101234567890987654321012345678909876543211234567890987654321012345678909876543210123456789098765432101234567890987654321";
                    routePrefix = "bigInteger/";
                    break;
                case ParameterType.DateTime:
                    parameterValue = "2024-01-28T15:38:20.0123000";
                    routePrefix = "date/";
                    break;
                case ParameterType.Guid:
                    parameterValue = Guid.NewGuid().ToString();
                    routePrefix = "guid/";
                    break;
            }

            var request = GetWebRequest(routePrefix + parameterValue, HttpVerb.Get);
            Router.Process(request, Response);

            Assert.That(Context.LastResponseData, Is.EqualTo(parameterValue));
        }

        [Test]
        [TestCase]
        [TestCase(true)]
        public void RouteParameter_MultipleParameters_RespondsWithParameter(bool useCurlyBrace = false) {
            string stringValue = "abc123";
            int intValue = 123456;
            DateTime dateTimeValue = DateTime.Parse("2024-01-28T15:38:20.0123000");
            Guid guidValue = Guid.NewGuid();
            var expectedJson = TestController.FormatUrlParameters(stringValue, intValue, dateTimeValue, guidValue);

            string route = $"{stringValue}/{guidValue}/2024-01-28T15:38:20.0123000/{intValue}";
            var request = GetWebRequest((useCurlyBrace? "curly/" : "") + route, HttpVerb.Get);
            Router.Process(request, Response);

            Assert.That(Context.LastResponseData, Is.EqualTo(expectedJson));
        }
        #endregion

        public enum ParameterType {
            String,
            UShort,
            Int,
            Decimal,
            Double,
            Float,
            Long,
            BigInteger,
            DateTime,
            Guid
        }
    }
}
