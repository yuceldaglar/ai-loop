namespace InternLoop.Helpers;

public class CliResult
{
	public bool Success { get; set; }
	public string? Output { get; set; }
	public string? ErrorOutput { get; set; }
	public int ExitCode { get; set; }
	public string? ErrorMessage { get; set; }
}
