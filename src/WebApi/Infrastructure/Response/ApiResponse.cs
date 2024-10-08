namespace Sindibad.SAD.WebTemplate.WebApi.Infrastructure.Response;
public class ApiResponse
{
    public bool Success { get; init; }

    public string[] Errors { get; init; } = [];
}

public class ApiResponse<TData> : ApiResponse
{
    public TData? Data { get; init; }
}
