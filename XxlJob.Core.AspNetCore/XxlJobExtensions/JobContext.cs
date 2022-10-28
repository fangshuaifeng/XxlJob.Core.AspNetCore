using System.Threading;
using XxlJob.Core.Logger;

namespace XxlJob.Core.AspNetCore
{
    /// <summary>
    /// 任务执行上下文
    /// </summary>
    public class JobContext
    {
        public IJobLogger JobLogger { get; }

        public string JobParameter { get; }

        public BroadCastModel BroadCast { get; set; }

        public CancellationToken CancellationToken { get; }

        public JobContext(IJobLogger jobLogger, string jobParameter, BroadCastModel broadCast, CancellationToken cancellationToken)
        {
            JobLogger = jobLogger;
            JobParameter = jobParameter;
            BroadCast = broadCast;
            CancellationToken = cancellationToken;
        }
    }
}
