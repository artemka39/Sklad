using Sklad.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sklad.Domain.Models
{
    public class Unit : ICatalogEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CatalogEntityStateEnum State { get; set; }
    }
}
