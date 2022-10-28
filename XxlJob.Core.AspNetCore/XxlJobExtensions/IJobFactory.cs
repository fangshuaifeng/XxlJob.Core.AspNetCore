using System;

namespace XxlJob.Core.AspNetCore.XxlJobExtensions
{
    public interface IJobFactory
    {
        IJobBaseHandler GetJobHandler(IServiceProvider provider, string handlerName);
    }
}
