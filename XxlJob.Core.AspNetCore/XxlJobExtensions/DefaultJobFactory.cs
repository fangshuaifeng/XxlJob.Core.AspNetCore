using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XxlJob.Core.Model;

namespace XxlJob.Core.AspNetCore.XxlJobExtensions
{
    public class DefaultJobFactory : IJobFactory
    {
        //
        // 摘要:
        //     禁止被用于Dispose
        private class JobHandlerWrapperInner : IJobBaseHandler
        {
            private readonly IJobBaseHandler _handler;

            public JobHandlerWrapperInner(IJobBaseHandler handler)
            {
                _handler = handler;
            }

            public Task<ReturnT> Execute(JobContext context)
            {
                return _handler.Execute(context);
            }
        }

        private readonly JobOptions _options;

        public DefaultJobFactory(IOptions<JobOptions> options, IEnumerable<IJobBaseHandler> jobHandlers)
        {
            _options = options.Value;
            foreach (IJobBaseHandler jobHandler in jobHandlers)
            {
                _options.AddJob(jobHandler);
            }
        }

        public IJobBaseHandler GetJobHandler(IServiceProvider provider, string handlerName)
        {
            if (!_options.JobHandlers.TryGetValue(handlerName, out var value))
            {
                return null;
            }

            if (value.Job != null)
            {
                return value.Job;
            }

            if (value.JobType == null)
            {
                return null;
            }

            object service = provider.GetService(value.JobType);
            if (service is IDisposable)
            {
                return new JobHandlerWrapperInner((IJobBaseHandler)service);
            }

            return (IJobBaseHandler)(service ?? ActivatorUtilities.CreateInstance(provider, value.JobType));
        }
    }
}
