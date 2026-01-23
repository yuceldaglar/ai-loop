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
            "test-deserialization" => new Commands.TestDeserializationCommand(),
            _ => throw new ArgumentException($"Unknown command: {commandName}")
        };
    }
}
