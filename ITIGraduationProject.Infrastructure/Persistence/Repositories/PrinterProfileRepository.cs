using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Infrastructure.Persistence.Repositories
{
    public class PrinterProfileRepository : GenericRepo<PrinterProfile>, IPrinterProfileRepository
    {
        public PrinterProfileRepository(AppDbContext context) : base(context)
        {
        }
    }
}
