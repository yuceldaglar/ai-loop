using InternLoop;
using InternLoop.Helpers;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Welcome to InternLoop!");
Console.WriteLine($"Current agent: {AgentManager.Instance.CurrentAgent.Name}");
Console.WriteLine();
Console.WriteLine("Available commands:");
Console.WriteLine("  create-plan - Create a new plan");
Console.WriteLine("  update-plan - Update an existing plan");
Console.WriteLine("  build - Build the project");
Console.WriteLine("  develop - Start development mode");
Console.WriteLine("  switch-agent [cursor|copilot] - Switch AI agent or show current");
Console.WriteLine();

string? commandName = args.Length > 0 ? args[0] : null;
string commandArgs = args.Length > 1 ? string.Join(" ", args[1..]) : string.Empty;

while (true)
{
    if (commandName == null)
    {
        Console.Write("Enter command (or 'exit' to quit): ");
        var s = Console.ReadLine();
        var parts = ParseCommandLine(s);
        commandName = parts.Length > 0 ? parts[0] : null;
        commandArgs = parts.Length > 1 ? string.Join(" ", parts[1..]) : string.Empty;
    }

    if (string.IsNullOrWhiteSpace(commandName))
    {
        Console.WriteLine("Command cannot be empty.");
        commandName = null;
        continue;
    }

    if (commandName.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Goodbye!");
        break;
    }

    try
    {
        var command = CommandParser.Parse(commandName, commandArgs);
        await command.ExecuteAsync();
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }

    if (args.Length > 0)
    {
        break;
    }

    commandName = null;
}

static string[] ParseCommandLine(string? input)
{
    if (string.IsNullOrWhiteSpace(input))
        return Array.Empty<string>();

    var parts = new List<string>();
    var currentPart = new System.Text.StringBuilder();
    bool inQuotes = false;

    for (int i = 0; i < input.Length; i++)
    {
        char c = input[i];

        if (c == '"')
        {
            inQuotes = !inQuotes;
        }
        else if (char.IsWhiteSpace(c) && !inQuotes)
        {
            if (currentPart.Length > 0)
            {
                parts.Add(currentPart.ToString());
                currentPart.Clear();
            }
        }
        else
        {
            currentPart.Append(c);
        }
    }

    if (currentPart.Length > 0)
    {
        parts.Add(currentPart.ToString());
    }

    return parts.ToArray();
}
