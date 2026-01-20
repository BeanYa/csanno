# 变更：重构测试，按功能模块拆分测试文件

## 为什么

当前测试文件存在以下问题：
1. 测试文件过长（`ComponentRegistrationTests.cs` 超过400行），测试组件定义混在测试文件底部，可维护性差
2. 测试组件与测试用例耦合在同一文件中，不利于复用
3. 缺乏清晰的模块化结构，难以快速定位特定功能的测试
4. 添加新测试时需要在大量代码中找到合适位置

## 变更内容

- 将测试文件按功能模块拆分到独立目录
- 将测试组件定义分离到单独的 `TestComponents/` 目录
- 创建共享的测试工具类/基类到 `Fixtures/` 目录
- 保持现有测试用例的逻辑和断言不变
- **非破坏性变更**：仅重构文件结构，不改变测试行为

## 影响

- 受影响规范：`testing`（测试结构规范）
- 受影响代码：
  - `tests/ComponentRegistrationTests.cs` → `tests/Lifetime/...`
  - `tests/ComponentRegistrationEdgeCasesTests.cs` → `tests/EdgeCases/...`
  - `tests/HelpersTests.cs` → `tests/Helpers/...`
