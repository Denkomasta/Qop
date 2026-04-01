using CsvHelper.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Sqeez.Api.Models.Import
{
    public class MasterImportDto
    {
        public string ClassName { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;

        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        [RegularExpression(@"^[a-zA-Z \-áéíóúýčďěňřšťžÁÉÍÓÚÝČĎĚŇŘŠŤŽ]+$", ErrorMessage = "First name can only contain letters, spaces, and dashes.")]
        public string StudentFirstName { get; set; } = string.Empty;
        [RegularExpression(@"^[a-zA-Z \-áéíóúýčďěňřšťžÁÉÍÓÚÝČĎĚŇŘŠŤŽ]+$", ErrorMessage = "Last name can only contain letters, spaces, and dashes.")]
        public string StudentLastName { get; set; } = string.Empty;
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
        public string StudentEmail { get; set; } = string.Empty;
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$", ErrorMessage = "Password does not meet complexity requirements.")]
        public string StudentPassword { get; set; } = string.Empty;
    }

    public sealed class MasterImportMap : ClassMap<MasterImportDto>
    {
        public MasterImportMap()
        {
            Map(m => m.ClassName).Name("Class Name");
            Map(m => m.AcademicYear).Name("Academic Year").Optional();

            Map(m => m.SubjectName).Name("Subject Name").Optional();
            Map(m => m.SubjectCode).Name("Subject Code").Optional();

            Map(m => m.StudentFirstName).Name("First Name");
            Map(m => m.StudentLastName).Name("Last Name");
            Map(m => m.StudentEmail).Name("Email");
            Map(m => m.StudentPassword).Name("Password").Optional();
        }
    }

    public class ImportResultDto
    {
        public int RecordsImported { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool HasRowErrors => Errors.Any();
    }

    public class BulkOperationResult<T>
    {
        public List<T> Created { get; set; } = new();
        public List<T> Existing { get; set; } = new();
        public List<string> SkippedMessages { get; set; } = new();
    }
}