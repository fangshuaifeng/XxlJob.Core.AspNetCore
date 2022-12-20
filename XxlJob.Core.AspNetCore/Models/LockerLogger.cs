using System;
using XxlJob.Core.Logger;

namespace XxlJob.Core.AspNetCore
{
    /// <summary>
    /// 带锁版日志执行器
    /// </summary>
    public class LockerLogger : ILockerLogger
    {
        private readonly object _lock = new object();
        private readonly IJobLogger _logger;
        public LockerLogger(IJobLogger _logger)
        {
            this._logger = _logger;
        }

        public void Log(string pattern, params object[] format)
        {
            lock (_lock)
            {
                _logger.Log(pattern, format);
            }
        }

        public void LogError(Exception ex)
        {
            lock (_lock)
            {
                _logger.LogError(ex);
            }
        }
    }
}
