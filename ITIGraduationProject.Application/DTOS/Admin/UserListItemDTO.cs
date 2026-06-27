using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.DTOS.Admin
{
    public class UserListItemDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
