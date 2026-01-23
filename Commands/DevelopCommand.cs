namespace InternLoop;

public class DevelopCommand : ICommand
{
    public Task ExecuteAsync()
    {
        Console.WriteLine("Starting development mode...");
        return Task.CompletedTask;
    }
}
