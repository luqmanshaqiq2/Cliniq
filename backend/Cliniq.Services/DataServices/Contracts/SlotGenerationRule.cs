using System;
using System.Collections.Generic;

namespace Cliniq.Services.DataServices.Contracts
{
    public sealed record SlotGenerationRule(
        IReadOnlyCollection<DayOfWeek> DaysOfWeek,
        TimeOnly StartTimeOfDayUtc,
        TimeOnly EndTimeOfDayUtc,
        TimeSpan SlotDuration);
}
