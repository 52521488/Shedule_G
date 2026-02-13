using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diplom_2.Core.Strategies
{

        public static class ScheduleStrategyFactory
        {
            public static IScheduleStrategy Create(string strategyName = "Simple")
            {
                switch (strategyName.ToLower())
                {
                    case "simple":
                        return new SimpleConflictStrategy();
                    // case "strict":
                    //     return new StrictConflictStrategy();
                    default:
                        return new SimpleConflictStrategy();
                }
            }
        }
}
