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
            "update-plan" => new UpdatePlanCommand(),
            "build" => new BuildCommand(),
            "develop" => new DevelopCommand(),
            "switch-agent" => new Commands.SwitchAgentCommand(arguments),
            "test-deserialization" => new Commands.TestDeserializationCommand(),
            "test-cursor-cli" => new Commands.TestCursorCliCommand(),
            _ => throw new ArgumentException($"Unknown command: {commandName}")
        };
    }
}
