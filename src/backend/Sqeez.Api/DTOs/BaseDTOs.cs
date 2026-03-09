using Sqeez.Api.Enums;

namespace Sqeez.Api.DTOs
{
    public record ServiceResult<T>(
        bool Success,
        T? Data,
        string? ErrorMessage,
        ServiceError ErrorCode = ServiceError.None)
    {
        public static ServiceResult<T> Ok(T data) =>
            new(true, data, null, ServiceError.None);

        public static ServiceResult<T> Failure(string message, ServiceError code) =>
            new(false, default, message, code);
    }

    public class PagedResponse<T>
    {
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    public class PagedFilterDto
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}
