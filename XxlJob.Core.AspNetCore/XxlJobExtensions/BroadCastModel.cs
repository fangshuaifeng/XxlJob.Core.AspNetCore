namespace XxlJob.Core.AspNetCore
{
    /// <summary>
    /// 分片信息
    /// </summary>
    public class BroadCastModel
    {
        public BroadCastModel(int index, int total)
        {
            Index = index;
            Total = total;
        }

        /// <summary>
        /// 分片索引
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 分片总数
        /// </summary>
        public int Total { get; set; }
    }
}
