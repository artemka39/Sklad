using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sklad.Contracts.Dtos;

namespace Sklad.Contracts.Requests
{
    public class CreateGoodsIssueDocumentRequest
    {
        public int ClientId { get; set; }
        public List<ResourceDto> Resources { get; set; }
    }
}
