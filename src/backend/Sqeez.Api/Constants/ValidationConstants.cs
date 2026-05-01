namespace Sqeez.Api.Constants
{
    public static class ValidationConstants
    {
        public const int NameMaxLength = 50;
        public const int UsernameMinLength = 3;
        public const int UsernameMaxLength = 20;
        public const int EmailMaxLength = 254;
        public const int PasswordMaxLength = 128;
        public const int TokenMaxLength = 512;
        public const int SearchTermMaxLength = 100;
        public const int TitleMaxLength = 150;
        public const int DescriptionMaxLength = 1000;
        public const int LongTextMaxLength = 4000;
        public const int UrlMaxLength = 2048;
        public const int AcademicYearMaxLength = 20;
        public const int SectionMaxLength = 20;
        public const int SubjectCodeMaxLength = 30;
        public const int DepartmentMaxLength = 100;
        public const int PhoneNumberMaxLength = 20;
        public const int LanguageCodeMaxLength = 10;
        public const int MaxPageSize = 100;
        public const int MaxBulkIds = 1000;
        public const int MaxQuizRetries = 100;
        public const int MaxQuestionDifficulty = 1000;
        public const int MaxQuestionTimeLimitSeconds = 86400;
        public const int MaxResponseTimeMs = 3600000;
        public const int MaxXpBonus = 100000;
        public const double MaxBadgeRuleTarget = 1000000;
        public const int MaxUploadSizeMb = 100;
        public const int MaxActiveSessionsPerUser = 20;
        public const int MinMark = 1;
        public const int MaxMark = 5;

        public const string EmailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        public const string PersonNameRegex = @"^[a-zA-Z \-\u00E1\u00E9\u00ED\u00F3\u00FA\u00FD\u010D\u010F\u011B\u0148\u0159\u0161\u0165\u017E\u00C1\u00C9\u00CD\u00D3\u00DA\u00DD\u010C\u010E\u011A\u0147\u0158\u0160\u0164\u017D]+$";
        public const string UsernameRegex = @"^[a-zA-Z0-9_\-\u00E1\u00E9\u00ED\u00F3\u00FA\u00FD\u010D\u010F\u011B\u0148\u0159\u0161\u0165\u017E\u00C1\u00C9\u00CD\u00D3\u00DA\u00DD\u010C\u010E\u011A\u0147\u0158\u0160\u0164\u017D]+$";
        public const string DepartmentRegex = @"^[a-zA-Z0-9_ \-\u00E1\u00E9\u00ED\u00F3\u00FA\u00FD\u010D\u010F\u011B\u0148\u0159\u0161\u0165\u017E\u00C1\u00C9\u00CD\u00D3\u00DA\u00DD\u010C\u010E\u011A\u0147\u0158\u0160\u0164\u017D.,&]+$";
        public const string FlexiblePhoneRegex = @"^\+?[0-9\s\-()]{7,15}$";
        public const string InternationalPhoneRegex = @"^00[1-9][0-9]{0,2}[0-9]{7,12}$";
        public const string PasswordComplexityRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$";
    }
}
