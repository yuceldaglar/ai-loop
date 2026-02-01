using System.Text;
using GitHub.Copilot.SDK;

namespace InternLoop.Helpers;

public class CopilotCliHelper
{
	public static async Task<CliResult> RunAsync(string prompt, string? model = null)
	{
		try
		{
			var modelToUse = model ?? Commands.CommandHelper.MODEL;

			await using var client = new CopilotClient(new CopilotClientOptions
			{
				Cwd = Environment.CurrentDirectory
			});
			await client.StartAsync();

			await using var session = await client.CreateSessionAsync(new SessionConfig
			{
				Model = modelToUse
			});

			var outputBuilder = new StringBuilder();
			var errorBuilder = new StringBuilder();
			var done = new TaskCompletionSource();

			session.On(evt =>
			{
				switch (evt)
				{
					case AssistantMessageEvent msg:
						outputBuilder.Append(msg.Data.Content);
						break;
					case SessionErrorEvent err:
						errorBuilder.AppendLine(err.Data.Message);
						break;
					case SessionIdleEvent:
						done.TrySetResult();
						break;
				}
			});

			await session.SendAsync(new MessageOptions { Prompt = prompt });
			await done.Task;

			var output = outputBuilder.ToString();
			var error = errorBuilder.ToString();

			return new CliResult
			{
				Success = string.IsNullOrEmpty(error),
				Output = output,
				ErrorOutput = string.IsNullOrEmpty(error) ? null : error,
				ExitCode = string.IsNullOrEmpty(error) ? 0 : 1
			};
		}
		catch (Exception ex)
		{
			return new CliResult
			{
				Success = false,
				ErrorMessage = $"Error calling Copilot SDK: {ex.Message}"
			};
		}
	}
}
