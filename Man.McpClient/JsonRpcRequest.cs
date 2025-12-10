using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Man.McpClient
{
    /// <summary>
    /// Represents a JSON-RPC 2.0 request message.
    /// MCP uses JSON-RPC 2.0 as its communication protocol over stdio (standard input/output).
    /// Each request has a unique ID to match responses, a method name, and optional parameters.
    /// </summary>
    public class JsonRpcRequest
    {
        /// <summary>
        /// The JSON-RPC protocol version. Always "2.0" for JSON-RPC 2.0.
        /// </summary>
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        /// <summary>
        /// Unique identifier for this request. The server will include this ID in its response
        /// so we can match responses to requests in async scenarios.
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// The name of the method to invoke on the server.
        /// For MCP, this could be "initialize", "tools/list", "tools/call", etc.
        /// </summary>
        [JsonPropertyName("method")]
        public string Method { get; set; }

        /// <summary>
        /// Parameters to pass to the method. This is typically an object or array.
        /// The structure depends on the specific method being called.
        /// </summary>
        [JsonPropertyName("params")]
        public object Params { get; set; }
    }
}
