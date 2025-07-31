using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sklad.Domain.Models
{
    public class OutboundResource
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public virtual Resource Resource { get; set; }
        public int UnitOfMeasurementId { get; set; }
        public virtual UnitOfMeasurement UnitOfMeasurement { get; set; }
        public int GoodsIssueDocumentId { get; set; }
        public virtual GoodsIssueDocument GoodsIssueDocument { get; set; }
        public int Count { get; set; }
    }
}
