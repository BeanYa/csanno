namespace Csanno.Generator.Models
{

    /// <summary>
    /// 元数据信息
    /// </summary>
    internal sealed class MetadataInfo
    {
        public string Key { get; set; } = string.Empty;
        public string ValueExpression { get; set; } = string.Empty;  // AOT 友好的 C# 表达式
    }
}
