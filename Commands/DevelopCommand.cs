using InternLoop.Helpers;
using InternLoop.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InternLoop;

public class DevelopCommand : ICommand
{

	private readonly string _userPrompt;

	public DevelopCommand(string userPrompt)
	{
		_userPrompt = userPrompt;
	}

	public async Task ExecuteAsync()
	{
		Console.WriteLine($"User prompt: {_userPrompt}");

		Console.WriteLine("Creating a change plan...");
		await CreateChangePlanAsync();

		Console.WriteLine("Implementing the plan...");
		await ImplementChangePlan();
	}

	public async Task CreateChangePlanAsync()
	{
		var systemPrompt = """
			We are creating a software application.
			Application is a collection of components.
			Component descriptions and architecture can be found in the '.ai/plan.json' file.
			Now user has give us the following request: '%p%'.
			Create a change plan and put in this file '.ai/change-plan.json' file.
			Change plan should be in the following json format:
			{
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

		var prompt = systemPrompt.Replace("%p%", _userPrompt);

		var result = await AgentManager.Instance.CurrentAgent.RunAsync(prompt);

		if (!result.Success)
		{
			Console.WriteLine(result.ErrorMessage ?? $"{AgentManager.Instance.CurrentAgent.Name} exited with code {result.ExitCode}");
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



	//-----------------------------------------------------
	public async Task ImplementChangePlan()
	{
		Console.WriteLine("Implementing the change plan...");

		const string planFilePath = ".ai/plan.json";
		const string changePlanFilePath = ".ai/change-plan.json";

		if (!File.Exists(planFilePath))
		{
			Console.WriteLine($"Error: {planFilePath} not found.");
			return;
		}

		try
		{
			// Read plan.json
			var planJson = await File.ReadAllTextAsync(planFilePath);
			var plan = JsonSerializer.Deserialize<PlanModel>(planJson, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
				Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
			});

			if (plan == null || plan.Components == null || plan.Components.Count == 0)
			{
				Console.WriteLine("No components found in the plan.");
				return;
			}

			// Read plan.json
			var changePlanJson = await File.ReadAllTextAsync(changePlanFilePath);
			var changePlan = JsonSerializer.Deserialize<PlanModel>(changePlanJson, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
				Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
			});

			if (changePlan == null || changePlan.Components == null || changePlan.Components.Count == 0)
			{
				Console.WriteLine("Change plan not found!");
				return;
			}

			bool componentsBuilt = true;

			// Keep looping while we can build components
			while (componentsBuilt)
			{
				componentsBuilt = false;

				// Loop through all components
				foreach (var component in changePlan.Components)
				{
					// Skip already completed components
					if (component.DevelopmentStatus == DevelopmentStatus.Completed)
					{
						continue;
					}

					// Check if component has no dependencies or all dependencies are completed
					bool canBuild = component.Dependencies == null ||
									component.Dependencies.Count == 0 ||
									component.Dependencies.All(dep =>
										plan.Components.Any(c =>
											c.ComponentName == dep &&
											c.DevelopmentStatus == DevelopmentStatus.Completed));

					if (canBuild)
					{
						component.DevelopmentStatus = DevelopmentStatus.Completed;
						Console.WriteLine($"Building component: {component.ComponentName}");

						componentsBuilt = await BuildComponent(plan, component);

						Console.WriteLine($"Component built! {component.ComponentName}");

						// Save plan.json after each component is built
						var updatedJson = JsonSerializer.Serialize(changePlan, new JsonSerializerOptions
						{
							WriteIndented = true,
							PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
							Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
						});
						await File.WriteAllTextAsync(changePlanFilePath, updatedJson);

						// Break to restart the loop and check for newly buildable components
						break;
					}
				}
			}

			// Check if there are still incomplete components
			var incompleteComponents = plan.Components.Where(c => c.DevelopmentStatus != DevelopmentStatus.Completed).ToList();
			if (incompleteComponents.Any())
			{
				Console.WriteLine("\nWarning: Some components could not be built due to unmet dependencies:");
				foreach (var component in incompleteComponents)
				{
					Console.WriteLine($"  - {component.ComponentName}");
				}
			}
			else
			{
				Console.WriteLine("\nAll components built successfully!");
			}
		}
		catch (JsonException ex)
		{
			Console.WriteLine($"Error parsing json: {ex.Message}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error during build: {ex.Message}");
		}
	}

	private async Task<bool> BuildComponent(PlanModel plan, ComponentModel component)
	{
		// Simulate building the component
		Console.WriteLine($"Building component: {component.ComponentName}");

		var existingComponent = plan.Components.Exists(component => component.ComponentName == component.ComponentName);
		var componentPrompt = existingComponent
			? $"We are updating the component '{component.ComponentName}' with the following description and design:\nDescription: {component.ComponentDescription}\nDetailed Design: {component.ComponentDetailedDesign}\nDependencies: {(component.Dependencies != null ? string.Join(", ", component.Dependencies) : "None")}"
			: $"We are building a new component '{component.ComponentName}' with the following description and design:\nDescription: {component.ComponentDescription}\nDetailed Design: {component.ComponentDetailedDesign}\nDependencies: {(component.Dependencies != null ? string.Join(", ", component.Dependencies) : "None")}";

		var systemPrompt = $""" 
			We have following architectural design decisions:
			{string.Join("\r\n", plan.ArchitecturalDesicions)}

			{componentPrompt}

			Then update the '.ai/plan.json' file with the new component information and mark it as completed.
		""";

		var prompt = systemPrompt;
		var result = await AgentManager.Instance.CurrentAgent.RunAsync(prompt);

		if (!result.Success)
		{
			Console.WriteLine(result.ErrorMessage ?? $"{AgentManager.Instance.CurrentAgent.Name} exited with code {result.ExitCode}");
			return false;
		}

		if (!string.IsNullOrWhiteSpace(result.Output))
		{
			Console.WriteLine(result.Output);
		}

		if (!string.IsNullOrWhiteSpace(result.ErrorOutput))
		{
			Console.Error.WriteLine(result.ErrorOutput);
		}

		return true;
	}
}
