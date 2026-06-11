using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Infrastructure.Persistence.Repositories
{
    public class ModerationReportRepository : GenericRepo<ModerationReport>, IModerationReportRepository
    {
        public ModerationReportRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ModerationReport>> GetByStatusAsync(ModerationReportStatus status)
        {
            return await _context.ModerationReports
                .AsNoTracking()
                .Where(r => r.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<ModerationReport>> GetByTemplateAsync(Guid templateId)
        {
            return await _context.ModerationReports
                .AsNoTracking()
                .Where(r => r.TargetTemplateId == templateId)
                .ToListAsync();
        }
    }
}
