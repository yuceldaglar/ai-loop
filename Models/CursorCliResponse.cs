using System.Text.Json.Serialization;

namespace InternLoop.Models;

/// <summary>
/// Represents the JSON response from Cursor CLI when using --output-format json
/// </summary>
public class CursorCliResponse
{
	[JsonPropertyName("type")]
	public string Type { get; set; } = string.Empty;

	[JsonPropertyName("subtype")]
	public string Subtype { get; set; } = string.Empty;

	[JsonPropertyName("is_error")]
	public bool IsError { get; set; }

	[JsonPropertyName("duration_ms")]
	public int DurationMs { get; set; }

	[JsonPropertyName("duration_api_ms")]
	public int DurationApiMs { get; set; }

	[JsonPropertyName("result")]
	public string Result { get; set; } = string.Empty;

	[JsonPropertyName("session_id")]
	public string SessionId { get; set; } = string.Empty;

	[JsonPropertyName("request_id")]
	public string? RequestId { get; set; }
}
