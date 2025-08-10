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
    public interface IResourceService
    {
        Task<List<Resource>> GetResourcesAsync(CatalogEntityStateEnum? state);
        Task<OperationResult<Resource>> CreateResourceAsync(Resource resource);
        Task<OperationResult<Resource>> UpdateResourceAsync(Resource resource);
        Task<OperationResult> DeleteResourceAsync(int resourceId);
        Task<OperationResult> DeleteMultipleResourceAsync(int[] resourceIds);
        Task<OperationResult> ArchiveResourceAsync(Resource resource);
    }
}
