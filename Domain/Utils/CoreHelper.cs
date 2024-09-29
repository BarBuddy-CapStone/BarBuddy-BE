
namespace Domain.Utils
{
    public class CoreHelper
    {
        public static DateTimeOffset SystemTimeNow => TimeHelper.ConvertToUtcPlus7(DateTimeOffset.Now);
    }
}
