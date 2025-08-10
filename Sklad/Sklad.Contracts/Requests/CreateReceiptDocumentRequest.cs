using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sklad.Contracts.Dtos;

namespace Sklad.Contracts.Requests
{
    public class CreateReceiptDocumentRequest
    {
        public List<ResourceDto> Resources { get; set; }
    }
}
