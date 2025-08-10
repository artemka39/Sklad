using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sklad.Contracts.Dtos;

namespace Sklad.Contracts.Requests
{
    public class CreateShipmentDocumentRequest
    {
        public int ClientId { get; set; }
        public List<ResourceDto> Resources { get; set; }
    }
}
