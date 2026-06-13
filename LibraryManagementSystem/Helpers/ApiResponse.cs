namespace LibraryAPI.Helpers;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public int StatusCode { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success", int statusCode = 200)
        => new() { Success = true, Message = message, Data = data, StatusCode = statusCode };

    public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400, List<string>? errors = null)
        => new() { Success = false, Message = message, StatusCode = statusCode, Errors = errors };
}

public class PagedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}