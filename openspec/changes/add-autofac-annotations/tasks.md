# 任务清单

## 依赖顺序

任务按顺序执行，前面的任务是后面任务的前置条件。

## 任务

### 1. 添加 Autofac 依赖

**描述**: 在项目中添加 Autofac NuGet 包依赖

**文件**:
- `src/Csanno.csproj`

**验证**:
- [x] 项目可以成功构建
- [x] `dotnet restore` 无错误

### 2. 创建核心特性

**描述**: 实现所有组件注册特性

**文件**:
- `src/Attributes/ComponentAttribute.cs` - 组件标记特性
- `src/Attributes/TransientAttribute.cs` - Transient 生命周期
- `src/Attributes/ScopedAttribute.cs` - Scoped 生命周期
- `src/Attributes/SingletonAttribute.cs` - Singleton 生命周期
- `src/Attributes/PerRequestAttribute.cs` - PerRequest 生命周期
- `src/Attributes/PerMatchingLifetimeScopeAttribute.cs` - 匹配作用域生命周期
- `src/Attributes/OwnedAttribute.cs` - 拥有实例生命周期
- `src/Attributes/AsServiceAttribute.cs` - 服务接口映射特性

**验证**:
- [x] 所有特性可以编译
- [x] 特性应用在测试类上无语法错误

### 3. 创建内部模型

**描述**: 实现组件注册信息模型

**文件**:
- `src/Internal/ComponentRegistration.cs` - 注册信息记录
- `src/Internal/InstanceLifetime.cs` - 生命周期枚举

**验证**:
- [x] 模型可以编译
- [x] 记录类型可以正确创建实例

### 4. 实现组件扫描器

**描述**: 实现扫描程序集并识别组件的逻辑

**文件**:
- `src/Internal/ComponentScanner.cs` - 核心扫描逻辑

**功能**:
- [x] 扫描程序集中的所有类型
- [x] 识别带 `[Component]` 特性的类型
- [x] 解析生命周期特性（按优先级）
- [x] 解析服务映射特性
- [x] 过滤无效类型（抽象类、接口、静态类、无公共构造函数）

**验证**:
- [x] 单元测试覆盖扫描逻辑
- [x] 可以正确识别简单组件
- [x] 可以正确识别带生命周期的组件
- [x] 可以正确识别带服务映射的组件

### 5. 实现注册扩展方法

**描述**: 实现 Autofac ContainerBuilder 扩展方法

**文件**:
- `src/RegistrationExtensions.cs` - 公开 API

**功能**:
- [x] `RegisterComponents(params Assembly[])` - 注册指定程序集
- [x] `RegisterComponents()` - 注册调用程序集
- [x] `RegisterComponentsFromType<T>()` - 从类型注册程序集

**验证**:
- [x] 扩展方法可以编译
- [x] 方法在 ContainerBuilder 上可用

### 6. 实现注册逻辑

**描述**: 将扫描结果转换为 Autofac 注册

**文件**:
- `src/RegistrationExtensions.cs` - 注册执行器（集成在扩展方法中）

**功能**:
- [x] 处理每种生命周期类型
- [x] 处理服务类型映射
- [ ] 处理元数据（待实现）
- [x] 处理 Owned 类型

**验证**:
- [x] 单元测试覆盖注册逻辑
- [x] Transient 组件每次返回新实例
- [x] Scoped 组件在同一作用域返回同一实例
- [x] Singleton 组件全局返回同一实例

### 7. 编写集成测试

**描述**: 测试完整的注册和解析流程

**文件**:
- `tests/ComponentRegistrationTests.cs`

**测试场景**:
- [x] 基础组件注册和解析
- [x] 服务接口映射
- [x] 生命周期行为验证
- [x] 构造函数依赖注入
- [x] 嵌套依赖解析
- [x] 多个服务接口注册
- [ ] 元数据注册和检索（待实现）
- [ ] Owned 实例注册（待测试）
- [ ] PerMatchingLifetimeScope 注册（待测试）

### 8. 编写边界测试

**描述**: 测试边界情况和错误处理

**文件**:
- `tests/ComponentRegistrationEdgeCasesTests.cs`

**测试场景**:
- [x] 抽象类被正确排除
- [x] 值类型被正确排除
- [x] 静态类被正确排除
- [x] 无公共构造函数的类被正确排除
- [ ] 无效的服务类型映射抛出异常（由编译器处理）
- [x] 未注册的依赖抛出 DependencyResolutionException
- [x] 多个生命周期特性时的优先级

### 9. 代码审查和优化

**描述**: 审查代码并进行必要的优化

**检查项**:
- [x] 代码符合 .NET 命名约定
- [x] XML 文档注释完整
- [x] 无编译警告
- [x] 无代码分析器问题
- [x] 性能无明显问题

### 10. 更新项目文档

**描述**: 更新项目文档说明新功能

**文件**:
- `README.md` (如存在)

**内容**:
- [ ] 添加组件注册功能说明
- [ ] 添加使用示例
- [ ] 添加 API 文档链接

## 实施总结

**已完成**: 所有核心功能已实现并通过测试
- 19 个测试全部通过
- 0 个编译警告
- 0 个代码分析器问题

**待后续实现**:
- 元数据注册支持
- 更多生命周期场景的测试覆盖
- 项目文档更新
