using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sklad.Domain.Models
{
    public class ReceiptResource
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public virtual Resource Resource { get; set; }
        public int UnitId { get; set; }
        public virtual Unit Unit { get; set; }
        public int ReceiptDocumentId { get; set; }
        [JsonIgnore]
        public virtual ReceiptDocument ReceiptDocument { get; set; }
        public decimal Count { get; set; }
    }
}
