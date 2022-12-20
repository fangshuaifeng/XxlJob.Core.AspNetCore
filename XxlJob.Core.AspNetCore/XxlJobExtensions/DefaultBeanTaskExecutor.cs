using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
            {
                return ReturnT.Failed($"job handler of job {triggerParam.JobId} is null.");
            }

            AsyncServiceScope asyncServiceScope = _factory.CreateAsyncScope();
            ConfiguredAsyncDisposable _ = asyncServiceScope.ConfigureAwait(continueOnCapturedContext: false);
            try
            {
                IJobBaseHandler handler = _handlerFactory.GetJobHandler(asyncServiceScope.ServiceProvider, triggerParam.ExecutorHandler);
                ReturnT result;
                if (handler == null)
                {
                    result = ReturnT.Failed("job handler [" + triggerParam.ExecutorHandler + "] not found.");
                }
                else
                {
                    try
                    {
                        result = await handler.Execute(new JobContext(new LockerLogger(_jobLogger)
                                                                        , triggerParam.ExecutorParams
                                                                        , new BroadCastModel(triggerParam.BroadcastIndex, triggerParam.BroadcastTotal)
                                                                        , cancellationToken)
                            ).ConfigureAwait(continueOnCapturedContext: false);
                    }
                    finally
                    {
                        IAsyncDisposable asyncDisposable = handler as IAsyncDisposable;
                        if (asyncDisposable != null)
                        {
                            await asyncDisposable.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
                        }
                        else
                        {
                            (handler as IDisposable)?.Dispose();
                        }
                    }
                }

                return result;
            }
            finally
            {
                await _.DisposeAsync();
            }
        }
    }
}
