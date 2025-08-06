using Sklad.Domain.Enums;
using Sklad.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sklad.Domain.Models
{
    public class UnitOfMeasurement : ICatalogEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CatalogEntityStateEnum State { get; set; }
    }
}
