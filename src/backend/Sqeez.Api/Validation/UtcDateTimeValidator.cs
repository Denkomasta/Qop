namespace Sqeez.Api.Validation
{
    public static class UtcDateTimeValidator
    {
        public const string ErrorMessageFormat = "The {0} field must be a UTC date-time value. Use an ISO 8601 value ending with 'Z'.";

        public static bool IsUtc(DateTime value)
        {
            return value.Kind == DateTimeKind.Utc;
        }

        public static bool IsUtc(DateTime? value)
        {
            return !value.HasValue || IsUtc(value.Value);
        }

        public static string GetErrorMessage(string fieldName)
        {
            return string.Format(ErrorMessageFormat, fieldName);
        }
    }
}
