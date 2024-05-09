using CNCO.Unify.Communications.Http;
using System.Net;
using System.Net.Sockets;

namespace UnifyTests.Communications.Http {
    [TestFixture]
    public class WebServerTests {
        private static int GetRandomOpenPort() {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }


        [Test]
        public void WebServer_StartAndStop_Success() {
            var webServer = new WebServer();

            webServer.Start();
            Assert.That(webServer.Running(), Is.True, "WebServer should be running after Start.");

            webServer.Stop();
            Assert.That(webServer.Running(), Is.False, "WebServer should not be running after Stop.");
        }


        [Test]
        public void WebServer_InitializeWithEndpointsOptions() {
            var options = new WebServerOptions() {
                Endpoints = ["localhost:12354", "https://example.com/", "http://127.0.0.1"],
            };

            var webServer = new WebServer(options);
            webServer.Listen("https://*:8800");

            var endpoints = webServer.GetEndpoints();
            Assert.That(endpoints, Has.Length.EqualTo(4), "WebServer is listening to 4 addresses");
            Assert.That(endpoints,
                        Is.EquivalentTo(new[] { "http://localhost:12354/", "https://example.com/", "http://127.0.0.1/", "https://*:8800/" }),
                        "WebServer is listening to the proper endpoints");

            webServer.Dispose();
        }

        /// <summary>
        /// Tests whether <see cref="WebServer"/> can receive HTTP GET requests and the router reacts on them.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task CanReceiveHTTPMethod_GET() {
            var uniqueCode = Guid.NewGuid().ToString();
            var port = GetRandomOpenPort();
            string address = $"http://127.0.0.1:{port}";

            var webServer = new WebServer();
            webServer.Listen(address);
            webServer.Get("/test", (request, response) => response.Send(uniqueCode));
            webServer.Start();

            using (var httpClient = new HttpClient()) {
                var response = await httpClient.GetAsync($"{address}/test");
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.That(content, Is.EqualTo(uniqueCode), "WebServer should handle the HTTP request.");
            }

            webServer.Stop();
            webServer.Dispose();
        }

        [Test]
        public async Task CanReceiveHTTPMethod_POST() {
            var uniqueCode = Guid.NewGuid().ToString();
            var port = GetRandomOpenPort();
            string address = $"http://127.0.0.1:{port}";

            var webServer = new WebServer();
            webServer.Listen(address);
            webServer.Post("/test", (request, response) => response.Send(uniqueCode));
            webServer.Start();

            using (var httpClient = new HttpClient()) {
                var response = await httpClient.PostAsync($"{address}/test", new StringContent(""));
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.That(content, Is.EqualTo(uniqueCode), "WebServer should handle the HTTP POST request.");
            }

            webServer.Stop();
            webServer.Dispose();
        }

        [Test]
        public async Task CanReceiveHTTPMethod_PUT() {
            var uniqueCode = Guid.NewGuid().ToString();
            var port = GetRandomOpenPort();
            string address = $"http://127.0.0.1:{port}";

            var webServer = new WebServer();
            webServer.Listen(address);
            webServer.Put("/test", (request, response) => response.Send(uniqueCode));
            webServer.Start();

            using (var httpClient = new HttpClient()) {
                var response = await httpClient.PutAsync($"{address}/test", new StringContent(""));
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.That(content, Is.EqualTo(uniqueCode), "WebServer should handle the HTTP PUT request.");
            }

            webServer.Stop();
            webServer.Dispose();
        }

        [Test]
        public async Task CanReceiveHTTPMethod_DELETE() {
            var uniqueCode = Guid.NewGuid().ToString();
            var port = GetRandomOpenPort();
            string address = $"http://127.0.0.1:{port}";

            var webServer = new WebServer();
            webServer.Listen(address);
            webServer.Delete("/test", (request, response) => response.Send(uniqueCode));
            webServer.Start();

            using (var httpClient = new HttpClient()) {
                var response = await httpClient.DeleteAsync($"{address}/test");
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.That(content, Is.EqualTo(uniqueCode), "WebServer should handle the HTTP DELETE request.");
            }

            webServer.Stop();
            webServer.Dispose();
        }

        [Test]
        public async Task CanReceiveHTTPMethod_HEAD() {
            var port = GetRandomOpenPort();
            string address = $"http://127.0.0.1:{port}";

            var webServer = new WebServer();
            webServer.Listen(address);
            webServer.Head("/test", (request, response) => {
                response.AddHeader("X-Test-Header", "HeadRequest");
                response.End();
            });
            webServer.Start();

            using (var httpClient = new HttpClient()) {
                var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, $"{address}/test"));

                // Assert
                Assert.That(response.Headers.Contains("X-Test-Header"), Is.True, "WebServer should handle the HTTP HEAD request.");
            }

            webServer.Stop();
            webServer.Dispose();
        }

        [Test]
        public async Task CanReceiveHTTPMethod_OPTIONS() {
            var uniqueCode = Guid.NewGuid().ToString();
            var port = GetRandomOpenPort();
            string address = $"http://127.0.0.1:{port}";

            var webServer = new WebServer();
            webServer.Listen(address);
            webServer.Options("/test", (request, response) => response.Send(uniqueCode));
            webServer.Start();

            using (var httpClient = new HttpClient()) {
                var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Options, $"{address}/test"));
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.That(content, Is.EqualTo(uniqueCode), "WebServer should handle the HTTP OPTIONS request.");
            }

            webServer.Stop();
            webServer.Dispose();
        }

        [Test]
        public async Task CanReceiveHTTPMethod_TRACE() {
            var uniqueCode = Guid.NewGuid().ToString();
            var port = GetRandomOpenPort();
            string address = $"http://127.0.0.1:{port}";

            var webServer = new WebServer();
            webServer.Listen(address);
            webServer.Trace("/test", (request, response) => response.Send(uniqueCode));
            webServer.Start();

            using (var httpClient = new HttpClient()) {
                var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Trace, $"{address}/test"));
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.That(content, Is.EqualTo(uniqueCode), "WebServer should handle the HTTP TRACE request.");
            }

            webServer.Stop();
            webServer.Dispose();
        }

        [Test]
        public async Task CanReceiveHTTPMethod_PATCH() {
            var uniqueCode = Guid.NewGuid().ToString();
            var port = GetRandomOpenPort();
            string address = $"http://127.0.0.1:{port}";

            var webServer = new WebServer();
            webServer.Listen(address);
            webServer.Patch("/test", (request, response) => response.Send(uniqueCode));
            webServer.Start();

            using (var httpClient = new HttpClient()) {
                var response = await httpClient.PatchAsync($"{address}/test", new StringContent(""));
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.That(content, Is.EqualTo(uniqueCode), "WebServer should handle the HTTP PATCH request.");
            }

            webServer.Stop();
            webServer.Dispose();
        }


        /// <summary>
        /// Checks whether the "all" route works with WebServer
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task CanReceiveHTTPMethod_All() {
            var uniqueCode = Guid.NewGuid().ToString();
            var port = GetRandomOpenPort();
            string address = $"http://127.0.0.1:{port}";

            var methods = new List<HttpMethod>() {
                HttpMethod.Get,
                HttpMethod.Post,
                HttpMethod.Patch,
                HttpMethod.Delete,
                HttpMethod.Head,
            };


            var webServer = new WebServer();
            webServer.Listen(address);

            // Register a route for all HTTP methods
            webServer.All("/test", (request, response) => {
                response.AddHeader("X-Test-Header", "AllRequest");
                response.Send(uniqueCode);
            });

            webServer.Start();

            // Perform requests for different HTTP methods
            foreach (var method in methods) {
                using (var httpClient = new HttpClient()) {
                    var request = new HttpRequestMessage(method, $"{address}/test");
                    var response = await httpClient.SendAsync(request);
                    var content = await response.Content.ReadAsStringAsync();

                    if (method != HttpMethod.Head)
                        Assert.That(content, Is.EqualTo(uniqueCode), $"WebServer should handle the HTTP {method.Method} request.");
                    else
                        Assert.That(response.Headers.Contains("X-Test-Header"), Is.True, $"WebServer should handle the HTTP {method.Method} request.");
                }
            }

            webServer.Stop();
            webServer.Dispose();
        }
    }
}
