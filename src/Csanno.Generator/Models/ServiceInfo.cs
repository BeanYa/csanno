namespace Csanno.Generator.Models
{

    /// <summary>
    /// 服务映射信息
    /// </summary>
    internal sealed class ServiceInfo
    {
        public string ServiceType { get; set; } = string.Empty;
        public bool IsSelf { get; set; }  // 是否注册为自身类型
    }
}
