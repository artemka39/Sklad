using Sklad.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sklad.Domain.Models
{
    public class ShipmentDocument
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int ClientId { get; set; }
        public virtual Client Client { get; set; }
        public DateTime Date { get; set; }
        public DocumentStateEnum State { get; set; }
        public ICollection<ShipmentResource> ShipmentResources { get; set; }
    }
}
