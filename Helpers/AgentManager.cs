using System.Text.Json;
using InternLoop.Models;

namespace InternLoop.Helpers;

/// <summary>
/// Available agent types
/// </summary>
public enum AgentType
{
    Cursor,
    Copilot
}

/// <summary>
/// Singleton manager for handling agent selection and persistence
/// </summary>
public class AgentManager
{
    private static readonly Lazy<AgentManager> _instance = new(() => new AgentManager());
    private static readonly string ConfigFileName = "config.json";

    private AgentType _currentAgentType;
    private readonly Dictionary<AgentType, IAgentHelper> _agents;

    /// <summary>
    /// Gets the singleton instance of AgentManager
    /// </summary>
    public static AgentManager Instance => _instance.Value;

    /// <summary>
    /// Gets the currently selected agent type
    /// </summary>
    public AgentType CurrentAgentType => _currentAgentType;

    /// <summary>
    /// Gets the currently selected agent helper
    /// </summary>
    public IAgentHelper CurrentAgent => _agents[_currentAgentType];

    /// <summary>
    /// Gets all available agent types
    /// </summary>
    public IEnumerable<AgentType> AvailableAgents => _agents.Keys;

    private AgentManager()
    {
        _agents = new Dictionary<AgentType, IAgentHelper>
        {
            { AgentType.Cursor, new CursorAgentHelper() },
            { AgentType.Copilot, new CopilotAgentHelper() }
        };

        LoadConfig();
    }

    /// <summary>
    /// Switches to the specified agent and persists the selection
    /// </summary>
    /// <param name="agentType">The agent type to switch to</param>
    public void SwitchAgent(AgentType agentType)
    {
        if (!_agents.ContainsKey(agentType))
        {
            throw new ArgumentException($"Unknown agent type: {agentType}");
        }

        _currentAgentType = agentType;
        SaveConfig();
    }

    /// <summary>
    /// Gets the config file path in the application directory
    /// </summary>
    private static string GetConfigPath()
    {
        var appDir = AppContext.BaseDirectory;
        return Path.Combine(appDir, ConfigFileName);
    }

    /// <summary>
    /// Loads the configuration from the config file
    /// </summary>
    private void LoadConfig()
    {
        var configPath = GetConfigPath();

        if (File.Exists(configPath))
        {
            try
            {
                var json = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<AppConfig>(json);

                if (config != null && Enum.TryParse<AgentType>(config.CurrentAgent, true, out var agentType))
                {
                    _currentAgentType = agentType;
                    return;
                }
            }
            catch (Exception)
            {
                // If loading fails, use default
            }
        }

        // Default to Cursor
        _currentAgentType = AgentType.Cursor;
    }

    /// <summary>
    /// Saves the current configuration to the config file
    /// </summary>
    private void SaveConfig()
    {
        var configPath = GetConfigPath();

        var config = new AppConfig
        {
            CurrentAgent = _currentAgentType.ToString()
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(config, options);
        File.WriteAllText(configPath, json);
    }
}
