using System.ComponentModel.DataAnnotations;

namespace Sqeez.Api.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class UtcDateTimeAttribute : ValidationAttribute
    {
        public UtcDateTimeAttribute()
        {
            ErrorMessage = UtcDateTimeValidator.ErrorMessageFormat;
        }

        public override bool IsValid(object? value)
        {
            return value switch
            {
                null => true,
                DateTime dateTime => UtcDateTimeValidator.IsUtc(dateTime),
                _ => false
            };
        }
    }
}
