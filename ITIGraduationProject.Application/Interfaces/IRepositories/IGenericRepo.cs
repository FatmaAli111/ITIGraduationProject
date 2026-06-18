using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Repositories
{
    public interface IGenericRepo<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);

        IQueryable<T> GetTableNoTracking();

        IQueryable<T> GetTableAsTracking();

        Task<T> AddAsync(T entity);

        Task AddRangeAsync(ICollection<T> entities);

        void Update(T entity);

        void UpdateRange(ICollection<T> entities);

        void Delete(T entity);

        void DeleteRange(ICollection<T> entities);
    }
}
