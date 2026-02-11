namespace InternLoop;

public class CommandParser
{
    public static ICommand Parse(string commandName)
    {
        return Parse(commandName, string.Empty);
    }

    public static ICommand Parse(string commandName, string arguments)
    {
        return commandName.ToLowerInvariant() switch
        {
            "create-plan" => new CreatePlanCommand(arguments),
            "build" => new BuildCommand(),
            "develop" => new DevelopCommand(arguments),
            "switch-agent" => new Commands.SwitchAgentCommand(arguments),
            _ => throw new ArgumentException($"Unknown command: {commandName}")
        };
    }
}
