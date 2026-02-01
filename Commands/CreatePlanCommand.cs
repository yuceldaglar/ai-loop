using InternLoop.Commands;
using InternLoop.Helpers;
using System;

namespace InternLoop;

public class CreatePlanCommand : ICommand
{
	private readonly string _userPrompt;

	public CreatePlanCommand(string userPrompt)
	{
		_userPrompt = userPrompt;
	}

	public async Task ExecuteAsync()
	{
		Console.WriteLine("Creating a new plan...");
		Console.WriteLine($"User prompt: {_userPrompt}");

		var systemPrompt = """ 
			When creating an application follow a component based approach.
			There will be some simple low level components.
			And more advanced components can be composed by other components.
			Components can interact with each other by events and other techniques when necessary.
			Create a plan file 'plan.json' for the following user request '%p%'.
			Plan should be in the following json format:
			{
				"application_description": ,
				"architectural_decisions": [
					"decision 1",
					"decision 2"
				],
				"components": [
					{
						"dependencies": [
							"component_name_1",
							"component_name_2"
						],
						"component_name": <component_name>,
						"component_description": <component_description>,
						"component_detailed_design": <component_detailed_design>,
						"development_status": "NotStarted"
					}
				]
			}
		""";

		//var planPromptFilePath = "plan_prompt.txt";
		var prompt = systemPrompt.Replace("%p%", _userPrompt);
		//File.WriteAllText(planPromptFilePath, prompt);

		var result = await CopilotCliHelper.RunAsync(prompt);

		if (!result.Success)
		{
			Console.WriteLine(result.ErrorMessage ?? $"Copilot CLI exited with code {result.ExitCode}");
		}

		if (!string.IsNullOrWhiteSpace(result.Output))
		{
			Console.WriteLine(result.Output);
		}

		if (!string.IsNullOrWhiteSpace(result.ErrorOutput))
		{
			Console.Error.WriteLine(result.ErrorOutput);
		}
	}
}
