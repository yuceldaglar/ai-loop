using InternLoop.Helpers;
using InternLoop.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InternLoop;

public class BuildCommand : ICommand
{
	public async Task ExecuteAsync()
	{
		Console.WriteLine("Building the project...");

		const string planFilePath = "plan.json";

		if (!File.Exists(planFilePath))
		{
			Console.WriteLine($"Error: {planFilePath} not found.");
			return;
		}

		try
		{
			// Read plan.json
			var jsonContent = await File.ReadAllTextAsync(planFilePath);
			var plan = JsonSerializer.Deserialize<PlanModel>(jsonContent, new JsonSerializerOptions
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

			// First, build the empty project ***
			if (plan.Components.All(c => c.DevelopmentStatus == DevelopmentStatus.NotStarted))
			{
				await BuildEmptyProject(plan);
			}

			bool componentsBuilt = true;

			// Keep looping while we can build components
			while (componentsBuilt)
			{
				componentsBuilt = false;

				// Loop through all components
				foreach (var component in plan.Components)
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
						Console.WriteLine($"Built component: {component.ComponentName}");

						componentsBuilt = await BuildComponent(plan, component);

						// Save plan.json after each component is built
						var updatedJson = JsonSerializer.Serialize(plan, new JsonSerializerOptions
						{
							WriteIndented = true,
							PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
							Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
						});
						await File.WriteAllTextAsync(planFilePath, updatedJson);

						// Break to restart the loop and check for newly buildable components
						break;
					}
				}

				// ************************************************
				// ************************************************
				// ************************************************
				// ************************************************
				break; // Temporary: Remove this line to enable full build process
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
			Console.WriteLine($"Error parsing plan.json: {ex.Message}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error during build: {ex.Message}");
		}
	}

	private async Task<bool> BuildEmptyProject(PlanModel plan)
	{
		// Simulate building the component
		Console.WriteLine($"Creating empty project!");

		var systemPromt = $"""
			We have following architectural design decisions:
			{string.Join("\r\n", plan.ArchitecturalDesicions)}

			If there is no project create an empty project for the following application description:
			{plan.ApplicationDescription}
		""";

		var prompt = systemPromt;

		var result = await CopilotCliHelper.RunAsync(prompt);

		if (!result.Success)
		{
			Console.WriteLine(result.ErrorMessage ?? $"Copilot CLI exited with code {result.ExitCode}");
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

	private async Task<bool> BuildComponent(PlanModel plan, ComponentModel component)
	{
		// Simulate building the component
		Console.WriteLine($"Building component: {component.ComponentName}");

		var systemPromt = """ 
			We have following architectural design decisions:
			%a%

			Based on these decisions, please help to build the component:
			Component Name: %component_name%
			Component Description: %component_description%
			Component Detailed Design: %component_detailed_design%
			Component Dependencies: %component_dependencies%
		""";

		var prompt = systemPromt
			.Replace("%a%", string.Join("\r\n", plan.ArchitecturalDesicions))
			.Replace("%component_name%", component.ComponentName)
			.Replace("%component_description%", component.ComponentDescription)
			.Replace("%component_detailed_design%", component.ComponentDetailedDesign)
			.Replace("%component_dependencies%", component.Dependencies != null ? string.Join(", ", component.Dependencies) : "None");

		var result = await CopilotCliHelper.RunAsync(prompt);

		if (!result.Success)
		{
			Console.WriteLine(result.ErrorMessage ?? $"Copilot CLI exited with code {result.ExitCode}");
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
