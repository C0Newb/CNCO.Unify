using System.Text;

namespace CNCO.Unify.Communications.Http {
    /// <summary>
    /// Event args when a <see cref="WebSocket"/> message is received.
    /// </summary>
    public class WebSocketMessageReceivedEventArgs : EventArgs {

        /// <summary>
        /// Message received.
        /// </summary>
        /// <remarks>
        /// Use <see cref="ToString"/> to get the UTF8 encoded string representation of this.
        /// </remarks>
        public byte[] Message;

        /// <summary>
        /// WebSocket that received the message.
        /// </summary>
        public WebSocket WebSocket { get; }


        public WebSocketMessageReceivedEventArgs(WebSocket webSocket, byte[] message) {
            WebSocket = webSocket;
            Message = message;
        }

        /// <summary>
        /// Returns a UTF8 encoded string of <see cref="Message"/>.
        /// </summary>
        /// <returns>UTF8 encoded string of <see cref="Message"/>.</returns>
        public override string ToString() => Encoding.UTF8.GetString(Message);
    }
}
