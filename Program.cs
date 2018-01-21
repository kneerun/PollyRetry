using Polly;
using Polly.Timeout;
using System;
using System.Threading;

namespace RetryPolicy
{
    class Program
    {
        static void Main(string[] args)
        {
            Policy
                .Handle<TimeoutRejectedException>()
                .WaitAndRetry(
                    retryCount: 5,
                    sleepDurationProvider: x => TimeSpan.FromSeconds(3),
                    onRetry: (exception, duration, retryCount, context) =>
                    {
                        Console.WriteLine($"MightFail failed. Retry #{retryCount}");
                    })
                .Execute(MightFail);

            Console.WriteLine("Finished succesfully. Goodbye");
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
    }
}
