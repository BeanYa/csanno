# 变更：移除示例辅助类

## 为什么

当前 `src/Helpers.cs` 中存在一个示例辅助类 `Helpers`，仅用于演示目的：

1. **非核心功能**：`Helpers.Greet()` 方法与项目的 Autofac 注解式组件注册功能无关
2. **测试依赖**：`tests/Helpers/HelpersTests.cs` 引用了 `src/Helpers.cs`，违反了测试独立性原则
3. **用户混淆**：作为类库发布时，用户可能会误以为这是库的一部分
4. **维护负担**：需要为无关功能编写和维护测试

## 变更内容

- 从 `src/Csanno.csproj` 中排除 `Helpers.cs`（或删除该文件）
- 删除 `tests/Helpers/HelpersTests.cs` 测试文件
- 更新项目文档，移除对 Helpers 的引用

## 影响

- 受影响规范：无（这是清理性变更）
- 受影响代码：
  - 删除 `src/Helpers.cs`
  - 删除 `tests/Helpers/HelpersTests.cs`
- 用户影响：无（这是示例代码，不是库的公开 API）
