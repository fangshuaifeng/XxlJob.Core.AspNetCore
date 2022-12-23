using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using XxlJob.Core.Logger;
using XxlJob.Core.Model;
using XxlJob.Core.TaskExecutors;

namespace XxlJob.Core.AspNetCore.XxlJobExtensions
{
    public class DefaultBeanTaskExecutor : ITaskExecutor
    {
        private readonly IJobFactory _handlerFactory;

        private readonly IJobLogger _jobLogger;

        private readonly IServiceScopeFactory _factory;

        public string GlueType => "BEAN";

        public DefaultBeanTaskExecutor(IJobFactory handlerFactory, IJobLogger jobLogger, IServiceScopeFactory factory)
        {
            _handlerFactory = handlerFactory;
            _jobLogger = jobLogger;
            _factory = factory;
        }

        public async Task<ReturnT> Execute(TriggerParam triggerParam, CancellationToken cancellationToken)
        {
            if (triggerParam.ExecutorHandler == null)
                return ReturnT.Failed($"job handler of job {triggerParam.JobId} is null.");

            var scope = _factory.CreateAsyncScope();

            await using var _ = scope.ConfigureAwait(false);

            var handler = _handlerFactory.GetJobHandler(scope.ServiceProvider, triggerParam.ExecutorHandler);

            if (handler == null) return ReturnT.Failed($"job handler [{triggerParam.ExecutorHandler}] not found.");

            try
            {
                return await handler.Execute(new JobContext(new LockerLogger(_jobLogger)
                                                                        , triggerParam.ExecutorParams
                                                                        , new BroadCastModel(triggerParam.BroadcastIndex, triggerParam.BroadcastTotal)
                                                                        , cancellationToken)).ConfigureAwait(false);
            }
            finally
            {
                // ReSharper disable SuspiciousTypeConversion.Global
                switch (handler)
                {
                    case IAsyncDisposable ad:
                        await ad.DisposeAsync().ConfigureAwait(false);
                        break;
                    case IDisposable d:
                        d.Dispose();
                        break;
                }
                // ReSharper restore SuspiciousTypeConversion.Global
            }
        }
    }
}
