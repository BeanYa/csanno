# 贡献指南

感谢你对 Csanno 项目的关注！本文档说明如何参与项目开发和发布流程。

## 开发环境

### 前置要求

- .NET SDK 8.0 或更高版本
- Git

### 克隆仓库

```bash
git clone https://github.com/BeanYa/csanno.git
cd csanno
```

### 构建项目

```bash
# 还原依赖
dotnet restore

# 构建项目
dotnet build

# 运行测试
dotnet test
```

## 开发流程

### 分支策略

- `main` - 主分支，保持稳定
- 功能分支 - 从 `main` 创建，用于开发新功能
  - 命名约定：`feature/功能名称` 或 `fix/问题描述`

### 提交规范

提交信息应清晰描述变更内容：

```
<类型>: <简短描述>

<详细说明（可选）>
```

类型示例：
- `feat`: 新功能
- `fix`: 修复问题
- `docs`: 文档更新
- `refactor`: 代码重构
- `test`: 测试相关
- `chore`: 构建/工具相关

### 代码规范

- 遵循 .NET 命名约定
- 保持 XML 文档注释完整
- 确保所有测试通过
- 避免编译警告

## 发布流程

### 版本管理

项目使用语义化版本（Semantic Versioning）：`主版本.次版本.补丁版本`

例如：`1.0.0`、`1.1.0`、`1.1.1`

### 发布步骤

1. **更新版本号**

   编辑 `src/Csanno.csproj`，更新 `<Version>` 标签：

   ```xml
   <Version>1.0.0</Version>
   ```

2. **创建版本标签**

   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

   标签格式：`v` + 版本号（如 `v1.0.0`）

3. **自动发布**

   推送标签后，GitHub Actions 会自动：
   - 运行所有测试
   - 创建 NuGet 包
   - 发布到 [NuGet.org](https://www.nuget.org/packages/Csanno)

4. **验证发布**

   访问 NuGet 包页面确认版本已发布：
   https://www.nuget.org/packages/Csanno

### 手动本地打包（用于测试）

```bash
# 打包项目
dotnet pack src/Csanno.csproj --configuration Release

# 验证包
dotnet nuget verify src/bin/Release/Csanno.*.nupkg

# 本地安装测试
dotnet new install -i src/bin/Release/Csanno.*.nupkg
```

### API 密钥配置

NuGet API 密钥已安全存储在 GitHub Secrets 中：

- 仓库设置 → Secrets and variables → Actions
- 名称：`NUGET_API_KEY`
- 获取密钥：https://www.nuget.org/account/apikeys

### GitHub Actions 工作流

发布工作流文件：`.github/workflows/publish.yml`

**触发条件**：
- 推送版本标签（如 `v1.0.0`）
- 手动触发（workflow_dispatch）

**执行步骤**：
1. 检出代码
2. 设置 .NET SDK
3. 提取版本号
4. 还原依赖
5. 构建项目
6. 运行测试
7. 打包项目
8. 发布到 NuGet.org

## 测试指南

### 运行测试

```bash
# 运行所有测试
dotnet test

# 运行特定测试
dotnet test --filter "FullyQualifiedName~TestClassName"

# 查看详细输出
dotnet test --verbosity normal
```

### 测试结构

```
tests/
├── Fixtures/           # 共享测试辅助设施
├── TestComponents/     # 测试用组件定义
├── Lifetime/           # 生命周期测试
├── Services/           # 服务注册测试
├── Metadata/           # 元数据测试
├── Owned/              # Owned 实例测试
├── Dependencies/       # 依赖注入测试
└── EdgeCases/          # 边界情况测试
```

## 报告问题

请通过 [GitHub Issues](https://github.com/BeanYa/csanno/issues) 报告问题或提出建议。

## 许可证

通过贡献代码，你同意你的贡献将使用 [MIT License](LICENSE) 进行许可。
