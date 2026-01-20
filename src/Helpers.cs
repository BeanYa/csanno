namespace Csanno;

/// <summary>
/// 示例辅助类
/// </summary>
public static class Helpers
{
    /// <summary>
    /// 获取问候语
    /// </summary>
    /// <param name="name">名称</param>
    /// <returns>问候语</returns>
    public static string Greet(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));
        }

        return $"Hello, {name}!";
    }
}
