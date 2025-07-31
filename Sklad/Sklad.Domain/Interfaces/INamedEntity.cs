using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sklad.Domain.Interfaces
{
    public interface INamedEntity
    {
        public string Name { get; set; }
    }
}
