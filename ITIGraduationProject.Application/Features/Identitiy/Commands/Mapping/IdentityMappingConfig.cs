using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITIGraduationProject.Application.DTOS;
using ITIGraduationProject.Application.Features.Identitiy.Commands.Models;
using Mapster;

namespace ITIGraduationProject.Application.Features.Identitiy.Commands.Mapping
{

        public class IdentityMappingConfig : IRegister
        {
            public void Register(TypeAdapterConfig config)
            {
                config.NewConfig<RegisterCommand, RegisterRequestDTO>();
            }
        }
    
}
