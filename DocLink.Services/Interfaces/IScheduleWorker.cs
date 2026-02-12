using System;
using System.Threading.Tasks;

namespace DocLink.Services.Interfaces
{
    public interface IScheduleWorker
    {
        Task<bool> IsSlotAvailable(Guid doctorId, DateTime date);
        Task ReserveSlot(Guid doctorId, DateTime date);
    }
}