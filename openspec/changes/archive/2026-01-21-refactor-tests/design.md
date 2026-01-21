## 上下文

当前测试结构将所有测试组件定义混在测试文件底部，随着测试用例增加，文件变得难以维护。需要重构为按功能模块组织的清晰结构。

## 目标 / 非目标

- 目标：
  - 按功能模块拆分测试文件（生命周期、接口映射、元数据等）
  - 分离测试组件定义到独立目录
  - 创建共享测试基类和工具
  - 保持所有现有测试通过

- 非目标：
  - 不改变测试逻辑和断言
  - 不修改测试框架（继续使用 NUnit）
  - 不改变项目命名空间结构

## 决策

### 目录结构设计

采用以下结构：

```
tests/
├── Fixtures/                    # 共享测试基类和工具
│   └── ContainerFixture.cs      # Container 构建共享逻辑
├── TestComponents/              # 测试用组件定义
│   ├── Lifetime/
│   │   ├── TransientComponent.cs
│   │   ├── ScopedComponent.cs
│   │   └── SingletonComponent.cs
│   ├── Services/
│   │   ├── ServiceWithInterface.cs
│   │   └── MultiServiceComponent.cs
│   ├── Metadata/
│   │   └── ComponentWithMetadata.cs
│   ├── Owned/
│   │   └── OwnedComponent.cs
│   ├── Dependencies/
│   │   ├── Consumer.cs
│   │   └── TopLevelConsumer.cs
│   └── EdgeCases/
│       └── AbstractComponent.cs
├── Lifetime/                    # 生命周期测试
│   ├── TransientLifetimeTests.cs
│   ├── ScopedLifetimeTests.cs
│   └── SingletonLifetimeTests.cs
├── Services/                    # 服务接口映射测试
│   └── ServiceRegistrationTests.cs
├── Metadata/                    # 元数据测试
│   └── MetadataRegistrationTests.cs
├── Owned/                       # Owned实例测试
│   └── OwnedInstanceTests.cs
├── Dependencies/                # 依赖注入测试
│   └── DependencyInjectionTests.cs
├── EdgeCases/                   # 边界情况测试
│   └── EdgeCaseTests.cs
└── Helpers/                     # Helpers工具类测试
    └── HelpersTests.cs
```

### 考虑的替代方案

1. **按测试类型分组（单元/集成）**
   - 理由：符合传统测试分类
   - 未选择：当前所有测试都是集成测试，按类型分组无法解决模块化问题

2. **单文件保持，仅分离组件**
   - 理由：变更较小
   - 未选择：测试文件仍然过长，未解决可维护性根本问题

3. **使用抽象基类共享测试逻辑**
   - 理由：减少重复代码
   - 风险：可能导致测试继承层次复杂
   - 决策：仅在 `Fixtures/` 中提供简单的容器构建辅助，避免过度抽象

## 风险 / 权衡

- **命名空间变更风险** → 确保所有测试类的命名空间保持为 `Csanno.Tests`
- **测试组件查找风险** → 使用 `Assembly` 扫描自动发现所有测试组件
- **迁移工作量** → 分模块逐步迁移，每个模块完成后验证测试通过

## 迁移计划

### 步骤

1. 创建新目录结构（`Fixtures/`, `TestComponents/`, 模块目录）
2. 创建 `ContainerFixture.cs` 共享容器构建逻辑
3. 迁移测试组件定义到 `TestComponents/` 各子目录
4. 迁移测试用例到各模块目录，更新组件引用
5. 验证所有测试通过
6. 删除旧的测试文件

### 回滚

如果迁移出现问题，可以从 git 恢复原有测试文件。

## 待决问题

无
