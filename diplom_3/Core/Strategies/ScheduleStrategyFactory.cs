using diplom_3.Core.Interfaces;

namespace diplom_3.Core.Strategies
{
    public static class ScheduleStrategyFactory
    {
        public static IScheduleStrategy Create(string strategyName = "Simple")
        {
            return strategyName?.ToLower() switch
            {
                "simple" => new SimpleConflictStrategy(),
                _ => new SimpleConflictStrategy()
            };
        }
    }
}
