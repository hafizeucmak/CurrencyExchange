using Polly.Extensions.Http;
using Polly;

namespace CurrencyExchange.API.ResiliencePolicies
{
    public static class PollyPolicies
    {
        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 2,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, timespan) =>
                    {
                        Console.WriteLine($"Circuit broken! Breaking for {timespan.TotalSeconds} seconds.");
                    },
                    onReset: () => Console.WriteLine("Circuit closed! Requests flow again."),
                    onHalfOpen: () => Console.WriteLine("Circuit in test mode: one request allowed."));
        }

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds.");
                    });
        }
    }
}
