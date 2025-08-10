using Sklad.Contracts.Responses;
using Sklad.Domain.Enums;
using Sklad.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sklad.Application.Interfaces
{
    public interface IUnitService
    {
        Task<List<Unit>> GetUnitsAsync(CatalogEntityStateEnum? state);
        Task<OperationResult<Unit>> CreateUnitAsync(Unit unit);
        Task<OperationResult<Unit>> UpdateUnitAsync(Unit unit);
        Task<OperationResult> DeleteUnitAsync(int unit);
        Task<OperationResult> ArchiveUnitAsync(Unit unit);
    }
}
