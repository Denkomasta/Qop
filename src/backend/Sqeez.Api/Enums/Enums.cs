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
        InternalError = 6
    }
}