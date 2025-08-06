using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sklad.Contracts.Dtos
{
    public class ResourceDto
    {
        public int ResourceId { get; set; }
        public int UnitOfMeasurementId { get; set; }
        public int Count { get; set; }
    }
}
