using System.Text.Json.Serialization;

namespace Domu.Api.Interface.Responses;

public sealed record ApiResponse<T>([property: JsonPropertyName("data")] T Data);
