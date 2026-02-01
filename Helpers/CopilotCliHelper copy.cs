// using System.Diagnostics;

// namespace InternLoop.Helpers;

// public class CopilotCliHelper
// {
// 	public static async Task<CliResult> RunAsync(string prompt, string? model = null)
// 	{
// 		try
// 		{
// 			var escapedPrompt = prompt.Replace("\"", "\\\"");
// 			var modelArg = model ?? Commands.CommandHelper.MODEL;
// 			ProcessStartInfo processStartInfo = CreateProcessStartInfo(escapedPrompt, modelArg);

// 			using var process = Process.Start(processStartInfo);
// 			if (process == null)
// 			{
// 				return new CliResult
// 				{
// 					Success = false,
// 					ErrorMessage = "Failed to start Copilot CLI process."
// 				};
// 			}

// 			var output = await process.StandardOutput.ReadToEndAsync();
// 			var error = await process.StandardError.ReadToEndAsync();

// 			await process.WaitForExitAsync();

// 			return new CliResult
// 			{
// 				Success = process.ExitCode == 0,
// 				Output = output,
// 				ErrorOutput = error,
// 				ExitCode = process.ExitCode
// 			};
// 		}
// 		catch (Exception ex)
// 		{
// 			return new CliResult
// 			{
// 				Success = false,
// 				ErrorMessage = $"Error calling Copilot CLI: {ex.Message}"
// 			};
// 		}
// 	}

// 	private static ProcessStartInfo CreateProcessStartInfo(string escapedPrompt, string modelArg)
// 	{
// 		var command = "gemini"; // "copilot"
// 		var args = $"-y -p \"{escapedPrompt}\""; //$"--allow-all-tools {modelArg} -p \"{escapedPrompt}\""
		
// 		return new ProcessStartInfo
// 		{
// 			FileName = "cmd.exe",
// 			Arguments = $"/c {command} {args}",
// 			UseShellExecute = false,
// 			RedirectStandardOutput = true,
// 			RedirectStandardError = true,
// 			CreateNoWindow = true,
// 			WorkingDirectory = Environment.CurrentDirectory
// 		};
// 	}
// }
