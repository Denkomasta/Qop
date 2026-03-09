//using Microsoft.EntityFrameworkCore;
//using Sqeez.Api.Data;
//using Sqeez.Api.DTOs;
//using Sqeez.Api.Enums;
//using Sqeez.Api.Models.Academics;
//using Sqeez.Api.Services.Interfaces;

//namespace Sqeez.Api.Services.EnrollmentService
//{
//    public class EnrollmentService : BaseService<EnrollmentService>, IEnrollmentService
//    {
//        public EnrollmentService(SqeezDbContext context, ILogger<EnrollmentService> logger)
//            : base(context, logger) { }

//        public async Task<ServiceResult<bool>> EnrollStudentAsync(EnrollStudentDto dto)
//        {
//            _logger.LogInformation("Enrolling student {StudentId} in subject {SubjectId}", dto.StudentId, dto.SubjectId);

//            // Check if enrollment already exists
//            var exists = await _context.Enrollments.AnyAsync(e => e.StudentId == dto.StudentId && e.SubjectId == dto.SubjectId);
//            if (exists)
//                return ServiceResult<bool>.Failure("Student already enrolled in this subject.", ServiceError.Conflict);

//            var enrollment = new Enrollment
//            {
//                StudentId = dto.StudentId,
//                SubjectId = dto.SubjectId,
//                EnrolledAt = DateTime.UtcNow,
//                IsActive = true,
//                Mark = 0
//            };

//            _context.Enrollments.Add(enrollment);
//            await _context.SaveChangesAsync();
//            return ServiceResult<bool>.Ok(true);
//        }

//        public async Task<ServiceResult<PagedResponse<EnrollmentDto>>> GetSubjectEnrollmentsAsync(long subjectId, int page, int size)
//        {
//            var query = _context.Enrollments
//                .Include(e => e.Student)
//                .Where(e => e.SubjectId == subjectId)
//                .AsNoTracking();

//            int total = await query.CountAsync();
//            var data = await query
//                .OrderBy(e => e.Student.Username)
//                .Skip((page - 1) * size)
//                .Take(size)
//                .Select(e => new EnrollmentDto(
//                    e.Id,
//                    e.StudentId,
//                    e.Student.Username,
//                    e.Mark,
//                    e.EnrolledAt,
//                    e.IsActive
//                ))
//                .ToListAsync();

//            return ServiceResult<PagedResponse<EnrollmentDto>>.Ok(new PagedResponse<EnrollmentDto>
//            {
//                Data = data,
//                TotalCount = total,
//                PageNumber = page,
//                PageSize = size
//            });
//        }

//        public async Task<ServiceResult<bool>> UpdateMarkAsync(long enrollmentId, int newMark)
//        {
//            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
//            if (enrollment == null)
//                return ServiceResult<bool>.Failure("Enrollment not found.", ServiceError.NotFound);

//            enrollment.Mark = newMark;
//            await _context.SaveChangesAsync();
//            return ServiceResult<bool>.Ok(true);
//        }
//    }
//}