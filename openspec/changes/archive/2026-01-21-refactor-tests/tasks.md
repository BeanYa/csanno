## 1. 创建目录结构

- [x] 1.1 创建 `tests/Fixtures/` 目录
- [x] 1.2 创建 `tests/TestComponents/` 及子目录（Lifetime, Services, Metadata, Owned, Dependencies, EdgeCases）
- [x] 1.3 创建测试模块目录（Lifetime, Services, Metadata, Owned, Dependencies, EdgeCases, Helpers）

## 2. 创建共享辅助设施

- [x] 2.1 创建 `tests/Fixtures/ContainerFixture.cs`，包含容器构建的共享逻辑

## 3. 迁移测试组件定义

- [x] 3.1 迁移生命周期组件（SimpleComponent, TransientComponent, ScopedComponent, SingletonComponent）到 `TestComponents/Lifetime/`
- [x] 3.2 迁移服务接口组件（ServiceWithInterface, MultiServiceComponent, IService等）到 `TestComponents/Services/`
- [x] 3.3 迁移元数据组件（ComponentWithMetadata, ComponentWithMultipleMetadataTypes等）到 `TestComponents/Metadata/`
- [x] 3.4 迁移 Owned 组件（OwnedComponent, DisposableOwnedComponent）到 `TestComponents/Owned/`
- [x] 3.5 迁移依赖注入组件（Consumer, TopLevelConsumer）到 `TestComponents/Dependencies/`
- [x] 3.6 迁移边界情况组件（AbstractComponent, StaticComponent等）到 `TestComponents/EdgeCases/`

## 4. 迁移生命周期测试

- [x] 4.1 创建 `tests/Lifetime/TransientLifetimeTests.cs`，迁移 Transient 生命周期测试
- [x] 4.2 创建 `tests/Lifetime/ScopedLifetimeTests.cs`，迁移 Scoped 生命周期测试
- [x] 4.3 创建 `tests/Lifetime/SingletonLifetimeTests.cs`，迁移 Singleton 生命周期测试
- [x] 4.4 创建 `tests/Lifetime/BasicComponentTests.cs`，迁移基础组件注册测试

## 5. 迁移服务接口测试

- [x] 5.1 创建 `tests/Services/ServiceRegistrationTests.cs`，迁移服务接口映射相关测试

## 6. 迁移元数据测试

- [x] 6.1 创建 `tests/Metadata/MetadataRegistrationTests.cs`，迁移元数据注册相关测试

## 7. 迁移 Owned 实例测试

- [x] 7.1 创建 `tests/Owned/OwnedInstanceTests.cs`，迁移 Owned 实例相关测试

## 8. 迁移依赖注入测试

- [x] 8.1 创建 `tests/Dependencies/DependencyInjectionTests.cs`，迁移构造函数依赖和嵌套依赖测试

## 9. 迁移边界情况测试

- [x] 9.1 创建 `tests/EdgeCases/EdgeCaseTests.cs`，迁移所有边界情况测试
- [x] 9.2 迁移 PerMatchingLifetimeScope 相关测试到独立文件或保持合并

## 10. 迁移 Helpers 测试

- [x] 10.1 创建 `tests/Helpers/HelpersTests.cs`，迁移 Helpers 工具类测试

## 11. 验证和清理

- [x] 11.1 运行所有测试确保通过
- [x] 11.2 删除旧的测试文件（`ComponentRegistrationTests.cs`, `ComponentRegistrationEdgeCasesTests.cs`, `HelpersTests.cs`）
- [x] 11.3 最终验证所有测试通过

## 12. 更新项目文件（如需要）

- [x] 12.1 检查并更新 `.csproj` 文件，确保新测试文件被包含（SDK 风格项目自动包含）
