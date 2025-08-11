using Sklad.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sklad.Contracts.Requests;
using Sklad.Contracts.Responses;
using Sklad.Contracts.Dtos;

namespace Sklad.Application.Interfaces
{
    public interface IStorageService
    {
        Task<List<Balance>> GetStorageBalanceAsync(int? resourceId, int? unitId);
    }
}
