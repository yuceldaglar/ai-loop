using InternLoop.Helpers;

namespace InternLoop.Commands;

public class TestCursorCliCommand : ICommand
{
	public async Task ExecuteAsync()
	{
		Console.WriteLine("=== Testing Cursor CLI Helper ===\n");

		// Simple test prompt - using ask mode to avoid file modifications
		const string testPrompt = "What is 2 + 2? Reply with just the number.";

		Console.WriteLine($"Test prompt: \"{testPrompt}\"");
		Console.WriteLine("Mode: ask (read-only, no file modifications)");
		Console.WriteLine(new string('-', 50));
		Console.WriteLine();

		Console.WriteLine("Calling Cursor CLI...\n");

		var result = await CursorCliHelper.RunAsync(
			prompt: testPrompt,
			mode: "ask"
		);

		Console.WriteLine("=== Result ===");
		Console.WriteLine($"Success: {result.Success}");
		Console.WriteLine($"Exit Code: {result.ExitCode}");

		if (result.Success)
		{
			Console.WriteLine($"\nOutput:\n{result.Output}");
		}
		else
		{
			Console.WriteLine($"\nError Message: {result.ErrorMessage}");

			if (!string.IsNullOrEmpty(result.ErrorOutput))
			{
				Console.WriteLine($"\nError Output:\n{result.ErrorOutput}");
			}

			if (!string.IsNullOrEmpty(result.Output))
			{
				Console.WriteLine($"\nRaw Output:\n{result.Output}");
			}
		}

		Console.WriteLine("\n" + new string('=', 50));
	}
}
