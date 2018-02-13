using Polly;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RetryPolicy
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Finished succesfully. Goodbye");
        }

        static void TestPolicy()
        {
            Policy
                .Handle<TimeoutRejectedException>()
            .WaitAndRetry(
                retryCount: 5,
                sleepDurationProvider: new RetryDurationProvider(RetryWaitDuration).RetrySleepProvider,
                onRetry: (exception, duration, retryCount, context) =>
                {
                    Console.WriteLine($"MightFail failed. Retry #{retryCount}");
                })
            .Execute(MightFail);
        }

        static void MightFail()
        {
            Policy.Timeout(seconds: 4,
                timeoutStrategy: TimeoutStrategy.Pessimistic,
                onTimeout: (context, duration, task) =>
                {
                    Console.WriteLine($"MightTakeAWhile Timeout. took: {duration}");
                })
                .Execute(MightTakeAWhile);
        }

        static void MightTakeAWhile()
        {
            Random rand = new Random(2); 

            // lot either 1 or 2
            int num = rand.Next(minValue: 1, maxValue: 3); 
            var sleepSeconds = num * 3;
            Console.WriteLine($"In MightTakeAWhile. will take {sleepSeconds} sec.");
            
            Thread.Sleep(TimeSpan.FromSeconds(sleepSeconds));
        }

        private static IEnumerable<TimeSpan> RetryWaitDuration()
        {
            yield return TimeSpan.FromSeconds(1);
            yield return TimeSpan.FromSeconds(10);
            yield return TimeSpan.FromMinutes(1);
            yield return TimeSpan.FromMinutes(5);
            yield return TimeSpan.FromMinutes(15);
            yield return TimeSpan.FromHours(1);
        }
    }

    class RetryDurationProvider
    {
        private IEnumerator<TimeSpan> it; 

        public RetryDurationProvider(Func<IEnumerable<TimeSpan>> func)
        {
            it = func().GetEnumerator();
        }

        public TimeSpan RetrySleepProvider(int dummy)
        {
            if (it.MoveNext())
                return it.Current;

            return TimeSpan.FromSeconds(1);
        }
    }
}
