using Sqeez.Api.Data;

namespace Sqeez.Api.Services
{
    public abstract class BaseService<TService>
    {
        protected readonly SqeezDbContext _context;
        protected readonly ILogger<TService> _logger;

        protected BaseService(SqeezDbContext context, ILogger<TService> logger)
        {
            _context = context;
            _logger = logger;
        }
    }
}