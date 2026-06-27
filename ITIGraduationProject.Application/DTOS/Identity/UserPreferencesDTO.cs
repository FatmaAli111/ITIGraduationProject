using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.DTOS
{
    public class UserPreferencesDTO
    {
        public string FavoriteColors { get; set; }
        public string BannedColors { get; set; }
        public string StyleType { get; set; }
        public string Interests { get; set; }
        public string DesignPreference { get; set; }
    }
}
