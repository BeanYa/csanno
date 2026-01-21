## 上下文

Csanno 是一个为 Autofac 提供注解式组件注册功能的类库。项目当前版本为 0.1.0，需要发布到 NuGet.org 供其他开发者使用。NuGet.org 对包的元数据有严格要求，缺少必要信息的包会被拒绝。

## 目标 / 非目标

- 目标：
  - 配置完整的 NuGet 包元数据
  - 支持自动化打包和发布
  - 提供符号包用于调试
  - 符合 NuGet.org 最佳实践

- 非目标：
  - 不改变项目的核心功能
  - 不修改现有的 API 设计

## 决策

### 包元数据配置

采用 MSBuild 属性直接配置，而非使用 `.nuspec` 文件：
- **理由**：SDK 风格项目原生支持，配置更集中
- **权衡**：`.nuspec` 更灵活但维护成本高

### 符号包配置

使用嵌入式符号包（`snupkg`）：
- **理由**：现代 .NET 推荐，符号与包分离
- **权衡**：需要额外的符号索引配置

### 发布方式

使用 GitHub Actions 自动化发布：
- **理由**：已在 GitHub 托管，CI/CD 集成方便
- **权衡**：需要配置 NuGet API 密钥

### 版本管理

手动控制版本号：
- **理由**：简单直接，适合小项目
- **未来考虑**：可引入 GitVersion 等工具

## 风险 / 权衡

- **NuGet API 密钥泄露** → 使用 GitHub Secrets 存储密钥
- **包命名冲突** → `Csanno` 已确认可用
- **许可证问题** → 使用 MIT 许可证，简单宽松

## 迁移计划

### 步骤

1. 更新 `.csproj` 添加完整包元数据
2. 添加 `LICENSE` 文件
3. 配置 GitHub Actions 工作流
4. 本地测试打包
5. 发布到 NuGet.org

### 验证

```bash
# 本地打包测试
dotnet pack src/Csanno.csproj -c Release

# 验证包内容
dotnet nuget verify Csanno.0.1.0.nupkg
```

## 待决问题

无
