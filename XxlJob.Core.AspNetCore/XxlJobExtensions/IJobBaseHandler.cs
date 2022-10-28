using System.Threading.Tasks;
using XxlJob.Core.Model;

namespace XxlJob.Core.AspNetCore
{
    public interface IJobBaseHandler
    {
        Task<ReturnT> Execute(JobContext context);
    }
}
