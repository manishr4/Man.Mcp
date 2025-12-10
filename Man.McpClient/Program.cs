using Man.McpClient;

/// <summary>
/// Example program demonstrating how to use the MCP client.
/// This connects to an MCP server, lists its tools, and optionally calls them.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Create a new MCP client instance
        var client = new McpClient();

        try
        {
            // Connect to an MCP server
            // This example uses npx to run a JavaScript-based MCP server
            // The "-y" flag auto-confirms the package installation
            Console.WriteLine("Connecting to MCP server...");
            await client.ConnectAsync("npx", new[] { "-y", "@modelcontextprotocol/server-everything" });

            // After successful connection and initialization, we can interact with the server

            // List all available tools
            // This shows what capabilities the server provides
            Console.WriteLine("\nListing tools...");
            var tools = await client.ListToolsAsync();
            Console.WriteLine(tools);

            // Example: Call a specific tool
            // Uncomment and modify this based on what tools your server provides
            // You'll need to know the tool name and what arguments it expects
            // Console.WriteLine("\nCalling tool...");
            // var result = await client.CallToolAsync("echo", new { message = "Hello MCP!" });
            // Console.WriteLine(result);

            // Wait for user input before disconnecting
            Console.WriteLine("\nPress any key to disconnect...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            // Handle any errors that occur during connection or communication
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            // Always disconnect to clean up resources
            // This closes streams and terminates the server process
            client.Disconnect();
        }
    }
}