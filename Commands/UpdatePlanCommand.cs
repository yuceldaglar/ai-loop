namespace InternLoop;

public class UpdatePlanCommand : ICommand
{
    public Task ExecuteAsync()
    {
        Console.WriteLine("Updating the plan...");
        return Task.CompletedTask;
    }
}
