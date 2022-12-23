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

            public JobHandlerWrapperInner(IJobBaseHandler handler) => _handler = handler;

            public Task<ReturnT> Execute(JobContext context) => _handler.Execute(context);
        }

        private readonly JobOptions _options;

        public DefaultJobFactory(IOptions<JobOptions> options, IEnumerable<IJobBaseHandler> jobHandlers)
        {
            _options = options.Value;

            foreach (var handler in jobHandlers) _options.AddJob(handler);
        }

        public IJobBaseHandler GetJobHandler(IServiceProvider provider, string handlerName)
        {
            if (!_options.JobHandlers.TryGetValue(handlerName, out var jobHandler)) return null;

            if (jobHandler.Job != null) return jobHandler.Job;

            if (jobHandler.JobType == null) return null;

            var job = provider.GetService(jobHandler.JobType);

            if (job is IDisposable) return new JobHandlerWrapperInner((IJobBaseHandler)job);

            return (IJobBaseHandler)(job ?? ActivatorUtilities.CreateInstance(provider, jobHandler.JobType));
        }
    }
}
