using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Enums;

namespace ITIGraduationProject.Application.Repositories
{
    public interface IModerationReportRepository : IGenericRepo<ModerationReport>
    {
        Task<IEnumerable<ModerationReport>> GetByStatusAsync(ModerationReportStatus status);
        Task<IEnumerable<ModerationReport>> GetByTemplateAsync(Guid templateId);
    }
}
