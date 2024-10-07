using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Security.Principal;

namespace CNCO.Unify.Communications.Http {
    /// <summary>
    /// Handles a <see cref="System.Net.WebSockets.WebSocket"/>.
    /// </summary>
    public class WebSocket : IWebSocket {
        private readonly WebSocketContext _webSocketContext;

        #region Public properties
        public CookieCollection CookieCollection => _webSocketContext.CookieCollection;
        public NameValueCollection Headers => _webSocketContext.Headers;
        public bool IsAuthenticated => _webSocketContext.IsAuthenticated;
        public bool IsLocal => _webSocketContext.IsLocal;
        public bool IsSecureConnection => _webSocketContext.IsSecureConnection;
        public string Origin => _webSocketContext.Origin;
        public Uri RequestUri => _webSocketContext.RequestUri;
        public string SecWebSocketKey => _webSocketContext.SecWebSocketKey;
        public IEnumerable<string> SecWebSocketProtocols => _webSocketContext.SecWebSocketProtocols;
        public string SecWebSocketVersion => _webSocketContext.SecWebSocketVersion;
        public IPrincipal? User => _webSocketContext.User;
        public System.Net.WebSockets.WebSocket Socket => _webSocketContext.WebSocket;

        public bool IsOpen => Socket != null && (Socket.State == WebSocketState.Connecting || Socket.State == WebSocketState.Open);

        public IWebRequest WebRequest { get; }

        public Task ListenerTask { get; }
        #endregion


        #region Events
        public event EventHandler<WebSocketMessageReceivedEventArgs>? MessageReceived;

        public event EventHandler<WebSocketConnectionClosedEventArgs>? ConnectionClosed;
        #endregion


        #region Constructors
        private WebSocket(IWebRequest webRequest, WebSocketContext webSocketContext) {
            _webSocketContext = webSocketContext;
            WebRequest = webRequest;

            ListenerTask = Task.Factory.StartNew(ListenToWebSocket, TaskCreationOptions.LongRunning);
        }
        #endregion


        /// <summary>
        /// Creates a WebSocket given a request and initiates the connection.
        /// </summary>
        /// <param name="httpListenerContext">Context containing the WebSocket handshake request.</param>
        /// <param name="webRequest">Web request that initiated the WebSocket handshake.</param>
        /// <param name="subProtocol">The supported WebSocket sub-protocol.</param>
        /// <param name="keepAliveInterval">The WebSocket keep-alive interval in milliseconds.</param>
        /// <param name="receiveBufferSize">The receive buffer size in bytes.</param>
        public static WebSocket CreateWebSocketConnection(HttpListenerContext httpListenerContext, IWebRequest webRequest, string? subProtocol = null, int? receiveBufferSize = null, TimeSpan? keepAliveInterval = null) {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(5000); // Give it 5 seconds to connect

            WebSocketContext? webSocketContext = null;
            Task.Run(async () => {

                if (receiveBufferSize != null && keepAliveInterval != null)
                    webSocketContext = await httpListenerContext.AcceptWebSocketAsync(subProtocol, receiveBufferSize.Value, keepAliveInterval.Value);

                else if (keepAliveInterval != null)
                    webSocketContext = await httpListenerContext.AcceptWebSocketAsync(subProtocol, keepAliveInterval.Value);
                else
                    webSocketContext = await httpListenerContext.AcceptWebSocketAsync(subProtocol);
            }, cancellationTokenSource.Token).Wait();

            if (webSocketContext == null)
                throw new WebSocketException("Failed to finalize WebSocket connection handshake.");

            return new WebSocket(webRequest, webSocketContext);
        }

        private async Task ListenToWebSocket() {
            // Process messages
            bool connectionAlive = true;
            List<byte> webSocketPayload = new List<byte>(1024 * 4);
            byte[] tempMessage = new byte[1024 * 4];

            while (connectionAlive) {
                webSocketPayload.Clear();

                WebSocketReceiveResult? webSocketResponse;
                do {
                    webSocketResponse = await Socket.ReceiveAsync(tempMessage, CancellationToken.None);
                    webSocketPayload.AddRange(new ArraySegment<byte>(tempMessage, 0, webSocketResponse.Count));
                } while (webSocketResponse.EndOfMessage == false);


                switch (webSocketResponse.MessageType) {
                    case WebSocketMessageType.Binary:
                    case WebSocketMessageType.Text:
                        var messageArg = new WebSocketMessageReceivedEventArgs(this, webSocketPayload.ToArray());
                        MessageReceived?.Invoke(this, messageArg);
                        break;

                    case WebSocketMessageType.Close:
                        connectionAlive = false;
                        var closedArgs = new WebSocketConnectionClosedEventArgs(this, webSocketResponse);
                        ConnectionClosed?.Invoke(this, closedArgs);
                        await Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Client request closure.", CancellationToken.None);
                        break;
                }
            }

            CommunicationsRuntime.Current.RuntimeLog.Debug(
                $"{GetType().Name}::{nameof(Process)}",
                $"Closed socket {WebRequest.RouteTemplate?.Template ?? RequestUri.PathAndQuery}"
            );
        }
    }
}
