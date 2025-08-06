using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sklad.Domain.Models
{
    public class GoodsReceiptDocument
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public DateTime Date { get; set; }
        public ICollection<InboundResource> InboundResources { get; set; }
    }
}