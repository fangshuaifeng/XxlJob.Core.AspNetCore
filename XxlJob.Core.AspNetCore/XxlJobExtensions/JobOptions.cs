using System;
using System.Collections.Generic;
using System.Reflection;

namespace XxlJob.Core.AspNetCore.XxlJobExtensions
{
    public class JobOptions
    {
        public readonly struct JobHandlerStruct
        {
            public IJobBaseHandler? Job { get; }

            public Type? JobType { get; }

            public JobHandlerStruct(IJobBaseHandler? job, Type? jobType)
            {
                Job = job;
                JobType = jobType;
            }
        }

        private readonly Dictionary<string, JobHandlerStruct> _jobHandlers = new Dictionary<string, JobHandlerStruct>();

        public IReadOnlyDictionary<string, JobHandlerStruct> JobHandlers => _jobHandlers;

        public void AddJob<TJob>() where TJob : class, IJobBaseHandler =>
            AddJob(typeof(TJob).GetCustomAttribute<JobHandlerAttribute>()?.Name ?? typeof(TJob).Name, typeof(TJob));

        public void AddJob(Type jobType) =>
            AddJob(jobType.GetCustomAttribute<JobHandlerAttribute>()?.Name ?? jobType.Name, jobType);

        public void AddJob<TJob>(string jobName) where TJob : class, IJobBaseHandler =>
            AddJob(jobName, typeof(TJob));

        public void AddJob(string jobName, Type jobType)
        {
            if (!typeof(IJobBaseHandler).IsAssignableFrom(jobType))
                throw new ArgumentException($"{jobType.FullName} not implement {typeof(IJobBaseHandler).FullName}", nameof(jobType));

            if (jobType.IsAbstract)
                throw new ArgumentException($"{jobType.FullName} should not abstract.", nameof(jobType));

            if (!jobType.IsClass)
                throw new ArgumentException($"{jobType.FullName} must be class.", nameof(jobType));

            if (_jobHandlers.ContainsKey(jobName))
                throw new Exception($"Same IJobHandler' name: [{jobName}]");

            _jobHandlers.Add(jobName, new JobHandlerStruct(null, jobType));
        }

        public void AddJob(IJobBaseHandler job)
        {
            string text = job.GetType().GetCustomAttribute<JobHandlerAttribute>()?.Name ?? job.GetType().Name;
            if (_jobHandlers.ContainsKey(text))
            {
                throw new Exception("Same IJobHandler' name: [" + text + "], are you register repeatedly?");
            }

            _jobHandlers.Add(text, new JobHandlerStruct(job, null));
        }
    }
}
