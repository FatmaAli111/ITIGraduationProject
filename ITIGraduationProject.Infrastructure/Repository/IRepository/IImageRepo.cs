using ITIGraduationProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Infrastructure.IRepository
{
    public interface IImageRepo
    {
        Task<Image> Upload(Image image);
    }
}