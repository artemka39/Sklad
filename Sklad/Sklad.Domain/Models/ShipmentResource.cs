using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sklad.Domain.Models
{
    public class ShipmentResource
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public virtual Resource Resource { get; set; }
        public int UnitId { get; set; }
        public virtual Unit Unit { get; set; }
        public int ShipmentDocumentId { get; set; }
        [JsonIgnore]
        public virtual ShipmentDocument ShipmentDocument { get; set; }
        public decimal Count { get; set; }
    }
}
