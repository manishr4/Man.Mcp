using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Man.McpClient
{
    /// <summary>
    /// The main MCP client class that manages communication with an MCP server.
    /// MCP servers communicate via stdio (standard input/output), so we spawn a process
    /// and communicate through its stdin/stdout streams.
    /// </summary>
    public class McpClient
    {
        // The spawned server process (e.g., a Node.js MCP server)
        private Process process;

        // Stream for writing JSON-RPC requests to the server's stdin
        private StreamWriter stdin;

        // Stream for reading JSON-RPC responses from the server's stdout
        private StreamReader stdout;

        // Counter for generating unique request IDs
        // Each request needs a unique ID to match with its response
        private int requestId = 0;

        /// <summary>
        /// Connects to an MCP server by spawning it as a child process.
        /// The server communicates over stdio (standard input/output).
        /// </summary>
        /// <param name="serverCommand">The command to run the server (e.g., "npx", "node", "python")</param>
        /// <param name="args">Command-line arguments for the server</param>
        public async Task ConnectAsync(string serverCommand, string[] args = null)
        {
            // Configure the process to spawn the MCP server
            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = serverCommand,
                    Arguments = args != null ? string.Join(" ", args) : "",

                    // We need to manage I/O ourselves, not use the shell
                    UseShellExecute = false,

                    // Redirect all standard streams so we can communicate via stdin/stdout
                    RedirectStandardInput = true,   // We write JSON-RPC requests here
                    RedirectStandardOutput = true,  // We read JSON-RPC responses here
                    RedirectStandardError = true,   // Capture errors for debugging

                    // Don't show a console window for the server process
                    CreateNoWindow = true
                }
            };

            // Start the server process
            process.Start();

            // Get handles to the stdin and stdout streams for communication
            stdin = process.StandardInput;
            stdout = process.StandardOutput;

            // Perform the MCP initialization handshake
            // This is required before we can use any MCP features
            await InitializeAsync();
        }



        /// <summary>
        /// Performs the MCP initialization handshake.
        /// This is a two-step process:
        /// 1. Send an "initialize" request with our client capabilities
        /// 2. Send an "initialized" notification to confirm we're ready
        /// This must be done before any other MCP operations.
        /// </summary>
        private async Task InitializeAsync()
        {
            // Step 1: Send the initialize request
            // This tells the server about our client and what features we support
            var initRequest = new JsonRpcRequest
            {
                Id = ++requestId,  // Get next unique ID
                Method = "initialize",
                Params = new
                {
                    // MCP protocol version we're using
                    protocolVersion = "2024-11-05",

                    // Client capabilities (features we support)
                    // Empty for basic client, but could include: roots, sampling, etc.
                    capabilities = new { },

                    // Information about our client application
                    clientInfo = new
                    {
                        name = "basic-mcp-client",
                        version = "1.0.0"
                    }
                }
            };

            // Send the request and wait for response
            var response = await SendRequestAsync(initRequest);
            Console.WriteLine("Initialized: " + response);

            // Step 2: Send the initialized notification
            // This confirms we've received the server's capabilities and are ready to proceed
            // Notifications don't expect a response (no ID field)
            await SendNotificationAsync("notifications/initialized");
        }



        /// <summary>
        /// Sends a JSON-RPC request to the server and waits for a response.
        /// Communication happens via stdio: write JSON to stdin, read JSON from stdout.
        /// Each line is a complete JSON-RPC message.
        /// </summary>
        /// <param name="request">The request object to send</param>
        /// <returns>The response as a JSON string</returns>
        private async Task<string> SendRequestAsync(JsonRpcRequest request)
        {
            // Serialize the request object to JSON
            var json = JsonSerializer.Serialize(request);

            // Write the JSON as a single line to the server's stdin
            // MCP uses line-delimited JSON (one message per line)
            await stdin.WriteLineAsync(json);

            // Flush to ensure the message is sent immediately
            await stdin.FlushAsync();

            // Read one line from stdout - this will be the response
            // This blocks until the server sends a response
            var response = await stdout.ReadLineAsync();
            return response;
        }



        /// <summary>
        /// Sends a JSON-RPC notification to the server.
        /// Unlike requests, notifications don't have an ID and don't expect a response.
        /// Used for events like "initialized" or to inform the server of changes.
        /// </summary>
        /// <param name="method">The notification method name</param>
        /// <param name="parameters">Optional parameters for the notification</param>
        private async Task SendNotificationAsync(string method, object parameters = null)
        {
            // Notifications are like requests but without an "id" field
            var notification = new
            {
                jsonrpc = "2.0",
                method = method,
                @params = parameters  // @ prefix needed because "params" is a keyword in some contexts
            };

            // Serialize and send the notification
            var json = JsonSerializer.Serialize(notification);
            await stdin.WriteLineAsync(json);
            await stdin.FlushAsync();
        }



        /// <summary>
        /// Lists all tools available on the MCP server.
        /// Tools are callable functions that the server exposes (like APIs).
        /// Examples: file operations, database queries, web searches, etc.
        /// </summary>
        /// <returns>JSON string containing the list of available tools</returns>
        public async Task<string> ListToolsAsync()
        {
            // Create a tools/list request
            var request = new JsonRpcRequest
            {
                Id = ++requestId,
                Method = "tools/list",
                Params = new { }  // Empty params object
            };

            return await SendRequestAsync(request);
        }



        /// <summary>
        /// Calls a specific tool on the MCP server with the provided arguments.
        /// This is how you actually invoke server functionality.
        /// </summary>
        /// <param name="toolName">Name of the tool to call (from tools/list)</param>
        /// <param name="arguments">Arguments to pass to the tool (structure depends on the tool)</param>
        /// <returns>JSON string containing the tool's response</returns>
        public async Task<string> CallToolAsync(string toolName, object arguments)
        {
            // Create a tools/call request
            var request = new JsonRpcRequest
            {
                Id = ++requestId,
                Method = "tools/call",
                Params = new
                {
                    name = toolName,      // Which tool to invoke
                    arguments = arguments  // Tool-specific parameters
                }
            };

            return await SendRequestAsync(request);
        }



        /// <summary>
        /// Cleanly disconnects from the MCP server and releases resources.
        /// Closes the streams and terminates the server process.
        /// </summary>
        public void Disconnect()
        {
            // Close the I/O streams
            stdin?.Close();
            stdout?.Close();

            // Terminate the server process
            process?.Kill();

            // Release process resources
            process?.Dispose();
        }


    }
}
