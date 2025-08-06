using Sklad.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sklad.Contracts.Requests
{
    public class UpdateGoodsIssueDocumentRequest
    {
        public int DocumentId { get; set; }
        public int ClientId { get; set; }
        public List<ResourceDto> Resources { get; set; }
    }
}
