using Rivo.Application.Common.Interfaces;

namespace Rivo.Infrastructure.Common;

public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
