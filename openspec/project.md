# Project Context

## Purpose
C# SDK/库项目，提供可复用的组件和功能模块。

## Tech Stack
- **Language:** C# (.NET)
- **Testing Framework:** NUnit
- **Package Management:** NuGet

## Project Conventions

### Code Style
- **Naming Conventions:** 遵循标准 .NET 命名约定
  - PascalCase for public members (classes, methods, properties)
  - _camelCase for private fields
  - IPascalCase for interfaces
- **Nullable Reference Types:** 已启用，倾向于显式处理空值
- **Async/Await:** 优先使用异步模式，避免 `.Result` 和 `.Wait()`
- **Documentation:** 使用 XML 文档注释 (`///`) 公开 API

### Architecture Patterns
- 按功能模块组织代码结构
- 接口驱动设计，便于测试和扩展
- 依赖注入原则

### Testing Strategy
- 使用 **NUnit** 作为单元测试框架
- 测试命名: `MethodName_Scenario_ExpectedResult`
- 保持测试独立和可重复运行

### Git Workflow
- **主分支:** `main`
- **Commit 约定:** 使用 conventional commit 格式
  - `feat:` 新功能
  - `fix:` Bug 修复
  - `refactor:` 代码重构
  - `docs:` 文档更新
  - `test:` 测试相关
  - `chore:` 构建/工具配置

## Domain Context
本项目为 C# SDK/库，专注于提供高质量、可复用的组件。

## Important Constraints
- 保持 API 向后兼容，破坏性变更需经过评审
- 遵循 .NET 版本兼容性要求
- 遵循 OpenSpec 规范驱动开发流程

## External Dependencies
- NuGet 包依赖
- .NET SDK
