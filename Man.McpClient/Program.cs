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
            // This example uses dotnet to run the C#-based MCP server
            Console.WriteLine("Connecting to local Man.McpServer...");
            //await client.ConnectAsync("dotnet", new[] { "run", "--project", "../../Man.McpServer/Man.McpServer.csproj" });
            await client.ConnectAsync("dotnet", new[] { "run", "--project", "E:/MyWorkingFolder/POC/MCP/Man.Mcp/Man.McpServer/Man.McpServer.csproj" });

            // After successful connection and initialization, we can interact with the server

            // List all available tools
            // This shows what capabilities the server provides
            Console.WriteLine("\nListing tools...");
            var tools = await client.ListToolsAsync();
            Console.WriteLine(tools);

            //Example: Call a specific tool
            //     Uncomment and modify this based on what tools your server provides
            //     You'll need to know the tool name and what arguments it expects
            Console.WriteLine("\nCalling tool...");
            var result = await client.CallToolAsync("get_random_number", new { message = "Please give a random number between 1 and 999." });
            Console.WriteLine(result);

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