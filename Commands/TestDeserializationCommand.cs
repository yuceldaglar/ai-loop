using System.Text.Json;
using System.Text.Json.Serialization;
using InternLoop.Models;

namespace InternLoop.Commands;

public class TestDeserializationCommand : ICommand
{
    public async Task ExecuteAsync()
    {
        Console.WriteLine("=== Testing plan.json Deserialization ===\n");
        
        const string planFilePath = "plan.json";
        var binPlanPath = @"bin\Debug\net10.0\plan.json";
        
        string planJsonPath = File.Exists(planFilePath) ? planFilePath : binPlanPath;
        
        if (!File.Exists(planJsonPath))
        {
            Console.WriteLine($"ERROR: plan.json not found at {planFilePath} or {binPlanPath}");
            return;
        }
        
        Console.WriteLine($"Reading plan.json from: {planJsonPath}\n");
        
        var jsonContent = await File.ReadAllTextAsync(planJsonPath);
        Console.WriteLine($"JSON content length: {jsonContent.Length} characters\n");
        
        // Test 1: Current approach (INCORRECT - will fail)
        Console.WriteLine("Test 1: PropertyNameCaseInsensitive = true ONLY (INCORRECT)");
        Console.WriteLine(new string('-', 70));
        
        var options1 = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        try
        {
            var plan1 = JsonSerializer.Deserialize<PlanModel>(jsonContent, options1);
            
            if (plan1 == null)
            {
                Console.WriteLine("? FAILED: Deserialization returned null");
            }
            else
            {
                var appDescStatus = string.IsNullOrEmpty(plan1.ApplicationDescription) ? "EMPTY ?" : "OK ?";
                var archDecStatus = plan1.ArchitecturalDesicions.Count == 0 ? "EMPTY ?" : $"{plan1.ArchitecturalDesicions.Count} items ?";
                var compStatus = plan1.Components.Count == 0 ? "EMPTY ?" : $"{plan1.Components.Count} items ?";
                
                Console.WriteLine($"  ApplicationDescription: {appDescStatus}");
                Console.WriteLine($"  ArchitecturalDesicions: {archDecStatus}");
                Console.WriteLine($"  Components: {compStatus}");
                
                Console.WriteLine("\n  ? PROBLEMS:");
                Console.WriteLine("    1. Missing PropertyNamingPolicy.SnakeCaseLower");
                Console.WriteLine("    2. Missing JsonStringEnumConverter for enum handling");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"  ? JSON ERROR: {ex.Message}");
        }
        
        Console.WriteLine("\n" + new string('-', 70) + "\n");
        
        // Test 2: Correct approach with all required settings
        Console.WriteLine("Test 2: CORRECT Configuration");
        Console.WriteLine(new string('-', 70));
        
        var options2 = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
        
        try
        {
            var plan2 = JsonSerializer.Deserialize<PlanModel>(jsonContent, options2);
            
            if (plan2 == null)
            {
                Console.WriteLine("? FAILED: Deserialization returned null");
            }
            else
            {
                var appDescStatus = string.IsNullOrEmpty(plan2.ApplicationDescription) ? "EMPTY ?" : "OK ?";
                var archDecStatus = plan2.ArchitecturalDesicions.Count == 0 ? "EMPTY ?" : $"{plan2.ArchitecturalDesicions.Count} items ?";
                var compStatus = plan2.Components.Count == 0 ? "EMPTY ?" : $"{plan2.Components.Count} items ?";
                
                Console.WriteLine($"  ApplicationDescription: {appDescStatus}");
                Console.WriteLine($"  ArchitecturalDesicions: {archDecStatus}");
                Console.WriteLine($"  Components: {compStatus}");
                
                if (!string.IsNullOrEmpty(plan2.ApplicationDescription) && 
                    plan2.ArchitecturalDesicions.Count > 0 && 
                    plan2.Components.Count > 0)
                {
                    Console.WriteLine("\n  ? SUCCESS: All properties deserialized correctly!");
                    
                    Console.WriteLine("\n  Sample data:");
                    Console.WriteLine($"    App Description: {plan2.ApplicationDescription.Substring(0, Math.Min(60, plan2.ApplicationDescription.Length))}...");
                    Console.WriteLine($"    Architectural Decisions: {plan2.ArchitecturalDesicions.Count}");
                    
                    if (plan2.Components.Count > 0)
                    {
                        var comp = plan2.Components[0];
                        Console.WriteLine($"    First Component: {comp.ComponentName}");
                        Console.WriteLine($"      Status: {comp.DevelopmentStatus} (enum deserialized correctly!)");
                        Console.WriteLine($"      Dependencies: [{string.Join(", ", comp.Dependencies)}]");
                    }
                }
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"  ? JSON ERROR: {ex.Message}");
        }
        
        Console.WriteLine("\n" + new string('=', 70));
        Console.WriteLine("\nSUMMARY:");
        Console.WriteLine("  The plan.json requires THREE configuration settings:");
        Console.WriteLine("\n  1. PropertyNameCaseInsensitive = true");
        Console.WriteLine("     (Allows case-insensitive matching)");
        Console.WriteLine("\n  2. PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower");
        Console.WriteLine("     (Converts snake_case JSON to PascalCase C# properties)");
        Console.WriteLine("\n  3. Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }");
        Console.WriteLine("     (Handles string enum values like \"NotStarted\")");
        Console.WriteLine(new string('=', 70));
    }
}
