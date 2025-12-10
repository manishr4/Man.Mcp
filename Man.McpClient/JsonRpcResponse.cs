using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Man.McpClient
{
    /// <summary>
    /// Represents a JSON-RPC 2.0 response message from the server.
    /// Every request generates a response with either a result or an error.
    /// </summary>
    public class JsonRpcResponse
    {
        /// <summary>
        /// The JSON-RPC protocol version.
        /// </summary>
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; }

        /// <summary>
        /// The ID from the original request. This allows us to match responses to requests.
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// The result of the method call if successful. Null if there was an error.
        /// JsonElement allows us to handle different return types flexibly.
        /// </summary>
        [JsonPropertyName("result")]
        public JsonElement? Result { get; set; }

        /// <summary>
        /// Error information if the method call failed. Null if successful.
        /// Contains error code and message according to JSON-RPC 2.0 spec.
        /// </summary>
        [JsonPropertyName("error")]
        public JsonElement? Error { get; set; }
    }
}
