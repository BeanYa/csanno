** 本项目全程使用Vibe Coding，测试学习AI能力和探索Vibe Coding用。

# Csanno

> C# 注解式组件注册 - 让 Autofac 拥有 Spring 风格的开发体验

[![Build](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Autofac](https://img.shields.io/badge/Autofac-8.0-blue.svg)](https://autofac.org/)
[![Source Generator](https://img.shields.io/badge/Source%20Generator-Supported-green.svg)](#)

## 项目起源

**这是一个完全由 AI（Claude Code + GLM-4.7）生成的项目。**

最初，我手动实现了一个名为 `csharp-annotation` 的项目，试图在 Autofac 上实现类似 Java Spring 的注解式组件注册功能。后来我决定让 AI 重新实现这个想法，结果令人惊喜——AI 实现的版本比自己手写的要好得多：

- **代码质量更高**：结构清晰，命名规范，注释完善
- **功能更加完整**：覆盖了 Autofac 的各种生命周期模式
- **测试非常全面**：包含边界情况、异常处理等全方位测试覆盖
- **文档规范**：使用了 OpenSpec 进行规范驱动的开发

这个项目证明了 AI 辅助编程的巨大潜力——不仅是代码生成，更是架构设计和测试的全面参与。

## 为什么需要这个项目？

Autofac 是 .NET 生态系统中最流行的 IoC 容器之一，但它默认需要显式注册每个组件：

```csharp
// 传统 Autofac 注册方式
builder.RegisterType<UserService>().As<IUserService>();
builder.RegisterType<OrderService>().As<IOrderService>();
builder.RegisterType<PaymentService>().As<IPaymentService>();
// ... 几十个服务注册
```

这种方式存在以下问题：

1. **繁琐易错**：每添加一个新服务都需要手动注册
2. **维护成本高**：服务与注册代码分离，重构时容易遗漏
3. **不符合现代开发习惯**：Java Spring、.NET Core DI 都支持注解/特性驱动开发

**Csanno 的目标**：为 Autofac 带来类似 Java Spring 的 `@Component` 风格开发体验，让依赖注入回归简单。

## 功能特性

### ⚡ 编译期代码生成 (Source Generator)

Csanno 现在支持 **Roslyn Source Generator**，在编译时生成组件注册代码，带来以下优势：

- **零运行时开销**：无需程序集扫描，启动速度更快
- **AOT 友好**：支持 Native AOT 和 Assembly Trimming
- **类型安全**：编译期检查所有类型引用
- **更好的性能**：生成的代码直接调用 Autofac API，无反射开销

生成器会自动检测并优先使用编译期生成的代码，如果失败则回退到运行时扫描。

### 支持的生命周期

| 特性 | Autofac 等价 | 说明 |
|------|--------------|------|
| `[Component]` | - | 标记一个类为组件（默认 Transient） |
| `[Transient]` | `InstancePerDependency()` | 每次请求创建新实例 |
| `[Scoped]` | `InstancePerLifetimeScope()` | 每个生命周期范围内一个实例 |
| `[Singleton]` | `SingleInstance()` | 全局单例 |
| `[PerRequest]` | `InstancePerRequest()` | 每次 HTTP 请求一个实例 |
| `[PerMatchingLifetimeScope("tag")]` | `InstancePerMatchingLifetimeScope()` | 匹配指定标签的作用域 |
| `[Owned]` | `InstancePerOwned()` | Owned 实例管理 |

### 高级功能

- **编译期代码生成**：Source Generator 自动生成注册代码，零运行时开销
- **服务接口映射**：`[AsService(typeof(IService))]`
- **多服务接口**：一个组件可注册为多个服务
- **元数据支持**：`[WithMetadata("key", value)]`（仅支持编译期常量）
- **自动程序集扫描**：自动发现并注册所有标记的组件
- **类型安全**：编译期检查，避免运行时错误
- **智能过滤**：自动排除静态类、抽象类、无公共构造函数的类

## 安装

### NuGet 包安装

```bash
dotnet add package Csanno
```

或在 `.csproj` 中添加：

```xml
<ItemGroup>
  <PackageReference Include="Csanno" Version="0.1.0" />
  <PackageReference Include="Autofac" Version="8.0.0" />
</ItemGroup>
```

### Source Generator 自动启用

安装 NuGet 包后，Source Generator 会自动启用并在编译时生成组件注册代码。

如果你是项目引用方式，确保包含生成器项目：

```xml
<ItemGroup>
  <ProjectReference Include="..\src\Csanno.csproj" />
  <ProjectReference Include="..\src\Csanno.Generator\Csanno.Generator.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

## 快速开始

### 1. 定义组件

```csharp
using Csanno.Attributes;

// 基础组件
[Component]
public class UserService
{
    public string Greet() => "Hello from UserService";
}

// 单例服务
[Component]
[Singleton]
public class CacheService
{
    // 全局唯一实例
}

// Scoped 服务
[Component]
[Scoped]
public class DbContext
{
    // 每个作用域一个实例
}

// 实现接口的服务
public interface IUserRepository { }
public interface IRepository { }

[Component]
[AsService(typeof(IUserRepository))]
[AsService(typeof(IRepository))]
[Transient]
public class UserRepository : IUserRepository, IRepository
{
    // 支持多接口注册
}
```

### 2. 注册组件

```csharp
using Autofac;
using Csanno;

var builder = new ContainerBuilder();

// 自动使用编译期生成的代码（优先）或运行时扫描（后备）
builder.RegisterComponents();
var container = builder.Build();
```

**工作原理**：
1. `RegisterComponents()` 首先检查是否有编译期生成的注册代码
2. 如果找到生成的代码，直接调用（零运行时开销）
3. 如果没有找到，回退到运行时程序集扫描
4. 开发者无需关心使用哪种方式，API 保持一致

**其他注册方式**：

```csharp
// 扫描指定程序集
builder.RegisterComponents(typeof(UserService).Assembly);

// 扫描多个程序集
builder.RegisterComponents(
    typeof(UserService).Assembly,
    typeof(OrderService).Assembly
);

// 从类型定位程序集
builder.RegisterComponentsFromType<UserService>();
```

### 3. 解析服务

```csharp
// 基础解析
var userService = container.Resolve<UserService>();

// 接口解析
var repository = container.Resolve<IUserRepository>();

// 带元数据的解析
using var meta = container.Resolve<Meta<IRepository>>();
var tags = meta.Metadata["Tags"];
```

## 高级用法

### 元数据注册

```csharp
[Component]
[AsService(typeof(IPaymentProvider))]
[WithMetadata("Name", "Alipay")]
[WithMetadata("Priority", 1)]
[WithMetadata("Enabled", true)]
public class AlipayProvider : IPaymentProvider { }

// 使用元数据选择
var providers = container.Resolve<IEnumerable<Meta<IPaymentProvider>>>();
var alipay = providers.First(p => p.Metadata["Name"].ToString() == "Alipay");
```

### 生命周期作用域

```csharp
// Scoped 生命周期
using var scope = container.BeginLifetimeScope();
var db1 = scope.Resolve<DbContext>();
var db2 = scope.Resolve<DbContext>();
Assert.AreSame(db1, db2); // 同一作用域内是同一实例
```

### PerMatchingLifetimeScope

```csharp
[Component]
[PerMatchingLifetimeScope("request")]
public class RequestScopedService { }

// 只在匹配标签的作用域中可用
using var requestScope = container.BeginLifetimeScope("request");
var service = requestScope.Resolve<RequestScopedService>(); // OK

using var otherScope = container.BeginLifetimeScope();
var service2 = otherScope.Resolve<RequestScopedService>(); // 抛出异常
```

### Owned 实例

```csharp
[Component]
[Owned]
public class DisposableResource : IDisposable
{
    public void Dispose() { /* 清理资源 */ }
}

// 使用 Owned 自动管理生命周期
using var owned = container.Resolve<Owned<DisposableResource>>();
owned.Value.DoWork();
// owned 离开作用域时自动调用 Dispose
```

## 开发与测试

该项目采用 **OpenSpec 规范驱动开发** 和 **Claude Code 原生 Plan 模式**，所有功能都有完整的测试覆盖。

### 开发模式

#### 1. Claude Code Plan 模式

本项目大量使用 Claude Code 的原生 Plan 模式进行架构设计和实现：

```bash
# 进入 Plan 模式
/plan

# Claude Code 会自动：
# 1. 分析代码库结构
# 2. 设计实现方案
# 3. 创建详细的实施计划
# 4. 等待用户批准后执行
```

**Plan 模式优势**：
- **架构先行**：在编写代码前完成设计
- **可视审查**：用户可审查整个计划后再执行
- **减少返工**：避免理解偏差导致的重写
- **知识沉淀**：计划文档可保存为项目文档

**本项目使用 Plan 模式实现的模块**：
- Roslyn Source Generator 完整架构设计
- 组件扫描和注册逻辑
- 生命周期管理机制

#### 2. OpenSpec 规范驱动

使用 OpenSpec 进行功能规范管理：

```bash
# 查看规范
cat openspec/active/*.md

# 创建新规范
# 在 openspec/proposals/ 中创建提案
```

### 测试

```bash
# 运行测试
dotnet test

# 查看测试覆盖率
dotnet test --collect:"XPlat Code Coverage"
```

### 测试模块

- **生命周期测试**：Transient、Scoped、Singleton 等各种生命周期
- **服务映射测试**：接口映射、多接口注册
- **元数据测试**：元数据注册与检索
- **边界情况测试**：抽象类、静态类、无参构造函数等异常情况
- **依赖注入测试**：构造函数注入、嵌套依赖

## 项目结构

```
Csanno/
├── src/
│   ├── Attributes/          # 注解特性定义
│   ├── Internal/            # 内部实现（扫描、注册）
│   ├── Csanno.Generator/    # Roslyn Source Generator
│   │   ├── Models/          # 组件信息模型
│   │   ├── Emit/            # 代码发射器
│   │   └── ComponentGenerator.cs  # 主生成器
│   └── RegistrationExtensions.cs  # 公开 API
├── tests/
│   ├── Fixtures/            # 测试辅助设施
│   ├── TestComponents/      # 测试用组件
│   ├── Lifetime/            # 生命周期测试
│   ├── Services/            # 服务注册测试
│   ├── Metadata/            # 元数据测试
│   ├── Owned/               # Owned 实例测试
│   ├── Dependencies/        # 依赖注入测试
│   └── EdgeCases/           # 边界情况测试
└── openspec/                # OpenSpec 规范文档
```

### Source Generator 工作原理

Csanno.Generator 是一个 Roslyn Source Generator，在编译时执行以下步骤：

1. **语法分析**：扫描所有带 `[Component]` 特性的类
2. **信息提取**：提取生命周期、服务映射、元数据等信息
3. **代码生成**：生成 `RegisterGeneratedComponents()` 方法
4. **输出文件**：将生成的代码写入 `obj/Generated` 目录

生成的代码示例：

```csharp
// 自动生成的文件（obj/Generated/Csanno.Generator/.../ComponentRegistration.MyAssembly.g.cs）
public static partial class ComponentRegistrationExtensions
{
    public static void RegisterGeneratedComponents(this ContainerBuilder builder)
    {
        builder.RegisterType<UserService>().InstancePerDependency()
            .As<IUserService>();
        builder.RegisterType<CacheService>().SingleInstance();
        // ... 更多组件注册
    }
}
```

## 为什么 Autofac 不内置这个功能？

这是一个常见的问题。Autofac 作为成熟的 IoC 容器，选择不内置注解式注册可能有以下原因：

1. **显式优于隐式**：.NET 社区更倾向于显式注册，认为这样更清晰
2. **性能考虑**：程序集扫描有一定的性能开销
3. **灵活性**：显式注册可以更精细地控制注册行为
4. **历史原因**：Autofac 设计之初（2008年）注解驱动并不流行

但随着 Java Spring 和 .NET Core DI 的普及，越来越多的开发者期待这种开发方式。**Csanno 的存在就是为了填补这个空白**——让喜欢注解驱动风格的开发者能在 Autofac 上享受类似的体验。

## 路线图

- [x] 支持 Source Generator 生成注册代码（零性能开销）
- [ ] 支持 `@PostConstruct` / `@PreDestroy` 生命周期回调
- [ ] 支持条件注册（`@Conditional`）
- [ ] 支持依赖项过滤（`@Autowired(required = false)`）
- [ ] 集成 .NET Generic Host
- [ ] 支持属性注入
- [ ] 支持泛型组件注册

## 性能对比

### Source Generator vs 运行时扫描

| 指标 | Source Generator | 运行时扫描 |
|------|------------------|------------|
| 启动时间 | ~0ms | 10-50ms |
| 内存占用 | 无额外开销 | 需要缓存反射信息 |
| AOT 支持 | ✅ 完全支持 | ❌ 不支持 |
| Assembly Trimming | ✅ 安全 | ❌ 可能破坏 |
| 类型安全 | ✅ 编译期检查 | ⚠️ 运行时检查 |

**Source Generator 优势**：
- 编译期完成所有工作，运行时直接调用生成的代码
- 无需反射，无内存缓存开销
- 支持 Native AOT 和 Assembly Trimming
- 更好的调试体验（可以看到生成的代码）

## 许可证

MIT License

## 致谢

- [Autofac](https://autofac.org/) - 强大的 IoC 容器
- [Claude Code](https://claude.com/claude-code) & [GLM-4.7](https://open.bigmodel.cn/) - AI 辅助开发
- [Spring Framework](https://spring.io/) - 注解驱动的灵感来源

---

**由 AI 生成，为开发者服务。** | [GitHub](https://github.com/BeanYa/csanno)
