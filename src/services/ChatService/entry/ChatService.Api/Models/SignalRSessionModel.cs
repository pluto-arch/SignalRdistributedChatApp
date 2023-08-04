namespace ChatService.Api.Models
{
    public class SignalRSessionModel
    {
        /// <summary>
        /// 服务器机器名称
        /// </summary>
        public string MechineName { get; set; }

        /// <summary>
        /// 实例名称
        /// </summary>
        public string ServiceInstrance { get; set; }

        /// <summary>
        /// 连接id
        /// </summary>
        public string ConnectionId{get;set;}
    }
}