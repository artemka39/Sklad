using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sklad.Contracts.Dtos
{
    public class DocumentFilterDto
    {
        public List<int> DocumentNumbers { get; set; } = new List<int>();
        public List<int> ResourceIds { get; set; } = new List<int>();
        public List<int> UnitOfMeasurementIds { get; set; } = new List<int>();

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
