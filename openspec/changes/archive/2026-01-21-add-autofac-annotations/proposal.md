# 变更：添加 Autofac 注解式组件注册

## 为什么

Autofac 是一个强大的 .NET 依赖注入容器，但传统配置方式需要在启动时手动编写大量注册代码。通过引入注解式（特性）声明方式，可以让组件注册更加声明式、类型安全，并减少样板代码。

## 变更内容

- 添加 Autofac NuGet 包依赖
- 创建自定义特性用于声明组件注册
- 实现组件扫描器，自动扫描并注册带特性的组件
- 支持所有 Autofac 生命周期选项
- 支持构造函数依赖自动解析
- 提供程序集扫描 API

## 影响

- 受影响规范: component-registration (新增)
- 受影响代码:
  - 添加 src/ComponentAttributes.cs - 组件特性定义
  - 添加 src/ComponentScanner.cs - 组件扫描器
  - 添加 src/RegistrationExtensions.cs - Autofac 扩展方法
  - 修改 src/Csanno.csproj - 添加 Autofac 依赖
  - 添加 tests/ComponentRegistrationTests.cs - 测试用例

## 向后兼容性

此变更是新增功能，不影响现有代码。所有现有功能保持不变。
