using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Domain.Enums
{
    [Flags]
    public enum PrintMethodType
    {
        DirectToGarment = 1,
        ScreenPrinting = 2,
        HeatTransfer = 4,
        Sublimation = 8,
        Embroidery = 16
    }
}
