using System;
using System.Threading.Tasks;
using Divergent.Finance.PaymentClient;
using Divergent.Sales.Messages.Events;
using ITOps.EndpointConfig;
using NServiceBus;
using NServiceBus.Logging;

namespace Divergent.Finance
{
    class Host
    {
        static readonly ILog Log = LogManager.GetLogger<Host>();
        IEndpointInstance endpoint;

        public static string EndpointName => "Divergent.Finance";

        public async Task Start()
        {
            try
            {
                var endpointConfiguration = new EndpointConfiguration(EndpointName)
                    .Configure();

                endpointConfiguration.RegisterComponents(registration =>
                    registration.ConfigureComponent<ReliablePaymentClient>(DependencyLifecycle.SingleInstance));

                endpoint = await Endpoint.Start(endpointConfiguration);
            }
            catch (Exception ex)
            {
                FailFast("Failed to start.", ex);
            }
        }

        public async Task Stop()
        {
            try
            {
                await endpoint?.Stop();
            }
            catch (Exception ex)
            {
                FailFast("Failed to stop correctly.", ex);
            }
        }

        async Task OnCriticalError(ICriticalErrorContext context)
        {
            try
            {
                await context.Stop();
            }
            finally
            {
                FailFast($"Critical error, shutting down: {context.Error}", context.Exception);
            }
        }

        void FailFast(string message, Exception exception)
        {
            try
            {
                Log.Fatal(message, exception);
            }
            finally
            {
                Environment.FailFast(message, exception);
            }
        }
    }
}
