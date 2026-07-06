namespace HedefTakip.Shared.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? MessageCode { get; set; }
    public string? Message { get; set; }
    public string? UserMessage { get; set; }
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string? userMessage = null) => new()
    {
        Success = true,
        Data = data,
        UserMessage = userMessage
    };

    public static ApiResponse<T> Fail(string messageCode, string message, string? userMessage = null) => new()
    {
        Success = false,
        MessageCode = messageCode,
        Message = message,
        UserMessage = userMessage
    };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse OkNoData(string? userMessage = null) => new()
    {
        Success = true,
        UserMessage = userMessage
    };

    public static new ApiResponse Fail(string messageCode, string message, string? userMessage = null) => new()
    {
        Success = false,
        MessageCode = messageCode,
        Message = message,
        UserMessage = userMessage
    };
}
