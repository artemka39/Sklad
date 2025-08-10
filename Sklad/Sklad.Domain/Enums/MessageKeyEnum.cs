using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sklad.Domain.Enums
{
    public enum MessageKeyEnum
    {
        Created,
        NameRequired,
        AddressRequired,
        CreationFailed,
        Updated,
        UpdateFailed,
        Deleted,
        DeletionFailed,
        NotFound,
        AlreadyExists,
        Archived,
        ArchiveFailed,
        AlreadyArchived,
        InUse,
        NoResourcesProvided,
        Signed,
        AlreadySigned,
        SigningFailed,
        Withdrawn,
        AlreadyWithdrawn,
        WithdrawalFailed,
        NotEnoughResource
    }
}
