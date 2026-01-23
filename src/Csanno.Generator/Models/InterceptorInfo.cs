using System.Collections.Generic;

namespace Csanno.Generator.Models;

/// <summary>
/// 拦截器信息模型
/// </summary>
internal sealed class InterceptorInfo
{
    /// <summary>
    /// 完整类型名称
    /// </summary>
    public string FullTypeName { get; set; } = string.Empty;

    /// <summary>
    /// 类名
    /// </summary>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// 绑定的注解类型列表
    /// </summary>
    public List<string> BoundAttributeTypes { get; set; } = new();
}
