using System.Diagnostics;
using System.Text;
using System.Text.Json;
using InternLoop.Models;

namespace InternLoop.Helpers;

/// <summary>
/// Helper class for interacting with the Cursor CLI (agent command)
/// </summary>
public class CursorCliHelper
{
	private const int DefaultTimeoutMs = 300000; // 5 minutes

	/// <summary>
	/// Gets the path to the agent executable
	/// </summary>
	private static string GetAgentPath()
	{
		// Check common installation locations
		var possiblePaths = new[]
		{
			// User-local installation (default on Windows)
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "cursor-agent", "agent.cmd"),
			// Alternative: agent.ps1
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "cursor-agent", "agent.ps1"),
			// Fallback to PATH
			"agent"
		};

		foreach (var path in possiblePaths)
		{
			if (path == "agent" || File.Exists(path))
			{
				return path;
			}
		}

		return "agent"; // Fallback to PATH lookup
	}

	/// <summary>
	/// Runs the Cursor CLI agent with the specified prompt
	/// </summary>
	/// <param name="prompt">The prompt to send to the agent</param>
	/// <param name="model">Optional model to use (e.g., "claude-4-sonnet")</param>
	/// <param name="mode">Optional mode: "agent" (default), "plan", or "ask"</param>
	/// <param name="force">If true, allows file modifications without confirmation</param>
	/// <param name="workingDirectory">Optional working directory for the agent</param>
	/// <param name="timeoutMs">Timeout in milliseconds (default: 5 minutes)</param>
	/// <returns>A CliResult containing the response or error information</returns>
	public static async Task<CliResult> RunAsync(
		string prompt,
		string? model = null,
		string? mode = null,
		bool force = false,
		string? workingDirectory = null,
		int timeoutMs = DefaultTimeoutMs)
	{
		try
		{
			var arguments = BuildArguments(model, mode, force);

			var agentPath = GetAgentPath();

			var startInfo = new ProcessStartInfo
			{
				FileName = agentPath,
				Arguments = arguments,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true,
				WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
				StandardOutputEncoding = Encoding.UTF8,
				StandardErrorEncoding = Encoding.UTF8
			};

			using var process = new Process { StartInfo = startInfo };
			var outputBuilder = new StringBuilder();
			var errorBuilder = new StringBuilder();

			process.OutputDataReceived += (sender, e) =>
			{
				if (e.Data != null)
					outputBuilder.AppendLine(e.Data);
			};

			process.ErrorDataReceived += (sender, e) =>
			{
				if (e.Data != null)
					errorBuilder.AppendLine(e.Data);
			};

			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			// Send prompt via stdin (required for print mode)
			await process.StandardInput.WriteLineAsync(prompt);
			process.StandardInput.Close();

			using var cts = new CancellationTokenSource(timeoutMs);
			try
			{
				await process.WaitForExitAsync(cts.Token);
			}
			catch (OperationCanceledException)
			{
				try
				{
					process.Kill(entireProcessTree: true);
				}
				catch { /* Ignore kill errors */ }

				return new CliResult
				{
					Success = false,
					ErrorMessage = $"Cursor CLI timed out after {timeoutMs}ms",
					ExitCode = -1
				};
			}

			var output = outputBuilder.ToString().Trim();
			var error = errorBuilder.ToString().Trim();

			if (process.ExitCode != 0)
			{
				return new CliResult
				{
					Success = false,
					Output = output,
					ErrorOutput = error,
					ExitCode = process.ExitCode,
					ErrorMessage = $"Cursor CLI exited with code {process.ExitCode}"
				};
			}

			// Parse JSON response
			return ParseJsonResponse(output, error);
		}
		catch (Exception ex)
		{
			return new CliResult
			{
				Success = false,
				ErrorMessage = $"Error calling Cursor CLI: {ex.Message}"
			};
		}
	}

	/// <summary>
	/// Builds the command-line arguments for the agent command
	/// </summary>
	private static string BuildArguments(string? model, string? mode, bool force)
	{
		var args = new StringBuilder();

		// Print mode for non-interactive use
		args.Append("-p ");

		// JSON output format for parsing
		args.Append("--output-format json ");

		// Optional model
		if (!string.IsNullOrWhiteSpace(model))
		{
			args.Append($"-m \"{model}\" ");
		}

		// Optional mode (agent, plan, ask)
		if (!string.IsNullOrWhiteSpace(mode))
		{
			args.Append($"--mode {mode} ");
		}

		// Force flag for file modifications
		if (force)
		{
			args.Append("-f ");
		}

		return args.ToString().Trim();
	}

	/// <summary>
	/// Parses the JSON response from Cursor CLI
	/// </summary>
	private static CliResult ParseJsonResponse(string output, string error)
	{
		if (string.IsNullOrWhiteSpace(output))
		{
			return new CliResult
			{
				Success = false,
				ErrorOutput = error,
				ErrorMessage = "Cursor CLI returned empty output"
			};
		}

		try
		{
			var response = JsonSerializer.Deserialize<CursorCliResponse>(output);

			if (response == null)
			{
				return new CliResult
				{
					Success = false,
					Output = output,
					ErrorMessage = "Failed to deserialize Cursor CLI response"
				};
			}

			if (response.IsError || response.Subtype != "success")
			{
				return new CliResult
				{
					Success = false,
					Output = response.Result,
					ErrorOutput = error,
					ExitCode = 1,
					ErrorMessage = $"Cursor CLI returned error: {response.Subtype}"
				};
			}

			return new CliResult
			{
				Success = true,
				Output = response.Result,
				ExitCode = 0
			};
		}
		catch (JsonException ex)
		{
			// If JSON parsing fails, return the raw output
			return new CliResult
			{
				Success = false,
				Output = output,
				ErrorOutput = error,
				ErrorMessage = $"Failed to parse Cursor CLI JSON response: {ex.Message}"
			};
		}
	}
}
