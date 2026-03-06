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
}
