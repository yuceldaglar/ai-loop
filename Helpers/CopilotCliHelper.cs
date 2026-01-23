using System.Diagnostics;

namespace InternLoop.Helpers;

public class CopilotCliHelper
{
	public static async Task<CopilotCliResult> RunAsync(string prompt, string? model = null)
	{
		try
		{
			var escapedPrompt = prompt.Replace("\"", "\\\"");
			var modelArg = model ?? Commands.CommandHelper.MODEL;
			var processStartInfo = new ProcessStartInfo
			{
				FileName = "copilot",
				Arguments = $"--allow-all-tools {modelArg} -p \"{escapedPrompt}\"",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
				WorkingDirectory = Environment.CurrentDirectory
			};

			using var process = Process.Start(processStartInfo);
			if (process == null)
			{
				return new CopilotCliResult
				{
					Success = false,
					ErrorMessage = "Failed to start Copilot CLI process."
				};
			}

			var output = await process.StandardOutput.ReadToEndAsync();
			var error = await process.StandardError.ReadToEndAsync();

			await process.WaitForExitAsync();

			return new CopilotCliResult
			{
				Success = process.ExitCode == 0,
				Output = output,
				ErrorOutput = error,
				ExitCode = process.ExitCode
			};
		}
		catch (Exception ex)
		{
			return new CopilotCliResult
			{
				Success = false,
				ErrorMessage = $"Error calling Copilot CLI: {ex.Message}"
			};
		}
	}
}

public class CopilotCliResult
{
	public bool Success { get; set; }
	public string? Output { get; set; }
	public string? ErrorOutput { get; set; }
	public int ExitCode { get; set; }
	public string? ErrorMessage { get; set; }
}
