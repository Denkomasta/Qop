namespace Sqeez.Api.Enums
{
    public enum UserRole { Student, Teacher, Admin }

    public enum AttemptStatus { Created, Started, Completed, Abandoned }

    public enum MediaType { Image, Video, Audio, Document }
    public enum ServiceError
    {
        None = 0,
        NotFound = 1,
        ValidationFailed = 2,
        Conflict = 3,
        Unauthorized = 4,
        Forbidden = 5,
        InternalError = 6,
        BadRequest = 7,
    }

    public enum BadgeMetric
    {
        ScorePercentage = 1,
        TotalScore = 2,
        PerfectAnswersCount = 3,
        TotalAttempts = 4,
    }

    public enum BadgeOperator
    {
        Equals = 1,             // ==
        GreaterThan = 2,        // >
        GreaterThanOrEqual = 3, // >=
        LessThan = 4,           // <
        LessThanOrEqual = 5,    // <=
        NotEquals = 6           // !=
    }
}