using System.Threading;
using XxlJob.Core.Logger;

namespace XxlJob.Core.AspNetCore
{
    /// <summary>
    /// 任务执行上下文
    /// </summary>
    public class JobContext
    {
        /// <summary>
        /// 带Lock锁
        /// </summary>
        public ILockerLogger JobLogger { get; }
        /// <summary>
        /// 任务参数
        /// </summary>
        public string JobParameter { get; }
        /// <summary>
        /// 广播模式分片器
        /// </summary>
        public BroadCastModel BroadCast { get; set; }

        public CancellationToken CancellationToken { get; }

        public JobContext(ILockerLogger jobLogger, string jobParameter, BroadCastModel broadCast, CancellationToken cancellationToken)
        {
            JobLogger = jobLogger;
            JobParameter = jobParameter;
            BroadCast = broadCast;
            CancellationToken = cancellationToken;
        }
    }
}
