using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.TemplateDTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Templates.Commands.Models
{
    /// <summary>
    /// Generates 3 AI templates based on the authenticated user's onboarding preferences.
    /// Called right after onboarding completes.
    /// </summary>
    public record GenerateOnboardingTemplatesCommand
        : IRequest<Response<List<TemplateDto>>>;
}