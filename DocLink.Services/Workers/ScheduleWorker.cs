using DocLink.Services.Interfaces;
using System.Threading.Tasks;
using System;

namespace DocLink.Services.Workers
{
    public class ScheduleWorker : IScheduleWorker
    {
        public Task<bool> IsSlotAvailable(Guid doctorId, DateTime date)
        {
            // Sprawdzenie czy termin wolny w zewnętrznym systemie/kalendarzu
            return Task.FromResult(true);
        }

        public Task ReserveSlot(Guid doctorId, DateTime date)
        {
            // Logika blokowania slotu
            return Task.CompletedTask;
        }
    }
}