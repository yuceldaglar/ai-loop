using InternLoop.Helpers;

namespace InternLoop.Commands;

/// <summary>
/// Command to switch between AI agents or display the current agent
/// </summary>
public class SwitchAgentCommand : ICommand
{
    private readonly string? _agentName;

    public SwitchAgentCommand(string arguments)
    {
        _agentName = string.IsNullOrWhiteSpace(arguments) ? null : arguments.Trim();
    }

    public Task ExecuteAsync()
    {
        var manager = AgentManager.Instance;

        // If no argument provided, display current agent
        if (string.IsNullOrEmpty(_agentName))
        {
            Console.WriteLine($"Current agent: {manager.CurrentAgent.Name}");
            Console.WriteLine();
            Console.WriteLine("Available agents:");
            foreach (var agent in manager.AvailableAgents)
            {
                var marker = agent == manager.CurrentAgentType ? " (active)" : "";
                Console.WriteLine($"  - {agent.ToString().ToLowerInvariant()}{marker}");
            }
            return Task.CompletedTask;
        }

        // Try to parse the agent name
        if (!Enum.TryParse<AgentType>(_agentName, ignoreCase: true, out var agentType))
        {
            Console.WriteLine($"Error: Unknown agent '{_agentName}'");
            Console.WriteLine();
            Console.WriteLine("Available agents:");
            foreach (var agent in manager.AvailableAgents)
            {
                Console.WriteLine($"  - {agent.ToString().ToLowerInvariant()}");
            }
            return Task.CompletedTask;
        }

        // Check if already using this agent
        if (agentType == manager.CurrentAgentType)
        {
            Console.WriteLine($"Already using agent: {manager.CurrentAgent.Name}");
            return Task.CompletedTask;
        }

        // Switch to the new agent
        manager.SwitchAgent(agentType);
        Console.WriteLine($"Switched to agent: {manager.CurrentAgent.Name}");

        return Task.CompletedTask;
    }
}
