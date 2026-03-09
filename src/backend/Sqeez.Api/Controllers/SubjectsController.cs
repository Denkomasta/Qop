using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqeez.Api.DTOs;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Controllers
{
    [Authorize]
    [Route("api/subjects")]
    public class SubjectsController : ApiBaseController
    {
        private readonly ISubjectService _subjectService;
        private readonly IEnrollmentService _enrollmentService;
        // private readonly IQuizService _quizService;

        public SubjectsController(
            ISubjectService subjectService,
            IEnrollmentService enrollmentService
            // IQuizService quizService
            )
        {
            _subjectService = subjectService;
            _enrollmentService = enrollmentService;
            // _quizService = quizService;
        }

        /// <summary>
        /// GET /api/subjects
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetAllSubjects([FromQuery] SubjectFilterDto filter)
        {
            var result = await _subjectService.GetAllSubjectsAsync(filter);
            return HandleServiceResult(result);
        }

        /// <summary>
        /// GET /api/subjects/5
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetSubjectById(long id)
        {
            var result = await _subjectService.GetSubjectByIdAsync(id);
            return HandleServiceResult(result);
        }

        /// <summary>
        /// POST /api/subjects
        /// </summary>
        [Authorize(Roles = "Admin")] // Only staff can create subjects
        [HttpPost]
        public async Task<ActionResult> CreateSubject([FromBody] CreateSubjectDto dto)
        {
            var result = await _subjectService.CreateSubjectAsync(dto);
            return HandleServiceResult(result);
        }

        /// <summary>
        /// PATCH /api/subjects/5
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}")]
        public async Task<ActionResult> PatchSubject(long id, [FromBody] PatchSubjectDto dto)
        {
            var result = await _subjectService.PatchSubjectAsync(id, dto);
            return HandleServiceResult(result);
        }

        /// <summary>
        /// DELETE /api/subjects/5
        /// Performs a Smart Delete (Hard delete if empty, Soft delete if active)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSubject(long id)
        {
            var result = await _subjectService.DeleteSubjectAsync(id);
            return HandleServiceResult(result);
        }

        /// <summary>
        /// GET /api/subjects/5/enrollments
        /// </summary>
        [HttpGet("{subjectId}/enrollments")]
        public async Task<ActionResult> GetEnrollmentsForSubject(long subjectId, [FromQuery] EnrollmentFilterDto filter)
        {
            // Force the filter to only look at this specific subject
            filter.SubjectId = subjectId;
            var result = await _enrollmentService.GetAllEnrollmentsAsync(filter);
            return HandleServiceResult(result);
        }

        /// <summary>
        /// POST /api/subjects/5/enrollments
        /// </summary>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost("{subjectId}/enrollments")]
        public async Task<ActionResult> EnrollStudents(long subjectId, [FromBody] AssignStudentsDto dto)
        {
            var result = await _enrollmentService.EnrollStudentsInSubjectAsync(subjectId, dto);
            return HandleServiceResult(result);
        }

        /// <summary>
        /// DELETE /api/subjects/5/enrollments
        /// </summary>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpDelete("{subjectId}/enrollments")]
        public async Task<ActionResult> UnenrollStudents(long subjectId, [FromBody] RemoveStudentsDto dto)
        {
            var result = await _enrollmentService.UnenrollStudentsFromSubjectAsync(subjectId, dto);
            return HandleServiceResult(result);
        }

        // ==========================================
        // 3. QUIZZES (SUB-RESOURCES)
        // ==========================================

        /// <summary>
        /// POST /api/subjects/5/quizzes
        /// Creates a new quiz attached to this subject.
        /// </summary>
        //[Authorize(Roles = "Admin,Teacher")]
        //[HttpPost("{subjectId}/quizzes")]
        //public async Task<ActionResult> AddQuizToSubject(long subjectId, [FromBody] CreateQuizDto dto)
        //{
        //    // Assuming your QuizService takes the subjectId and the quiz details
        //    // var result = await _quizService.CreateQuizAsync(subjectId, dto);
        //    // return HandleResult(result);

        //    return StatusCode(501, new { message = "Quiz service not implemented yet." });
        //}

        /// <summary>
        /// DELETE /api/subjects/5/quizzes/12
        /// Deletes a specific quiz from this subject.
        /// </summary>
        //[Authorize(Roles = "Admin,Teacher")]
        //[HttpDelete("{subjectId}/quizzes/{quizId}")]
        //public async Task<ActionResult> RemoveQuizFromSubject(long subjectId, long quizId)
        //{
        //    // var result = await _quizService.DeleteQuizAsync(subjectId, quizId);
        //    // return HandleResult(result);

        //    return StatusCode(501, new { message = "Quiz service not implemented yet." });
        //}
    }
}