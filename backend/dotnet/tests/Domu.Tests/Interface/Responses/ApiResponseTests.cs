using System.Text.Json;
using Domu.Api.Interface.Responses;

namespace Domu.Tests.Interface.Responses;

public sealed class ApiResponseTests
{
    [Fact]
    public void Serialize_UsesDataAsTheEnvelopeProperty()
    {
        var response = new ApiResponse<string[]>(["first", "second"]);

        var json = JsonSerializer.Serialize(response);

        Assert.Equal("{\"data\":[\"first\",\"second\"]}", json);
    }
}
