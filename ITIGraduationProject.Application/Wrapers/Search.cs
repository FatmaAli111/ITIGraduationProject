using ITIGraduationProject.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Wrapers
{
    public static class Search
    {
        public static IQueryable<User> ApplySearch(IQueryable<User> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            return query.Where(u => u.Name.Contains(search));
        }
    }
}
