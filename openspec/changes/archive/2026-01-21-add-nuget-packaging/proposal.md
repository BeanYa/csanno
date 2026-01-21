# 变更：添加 NuGet 包打包和发布支持

## 为什么

当前项目已具备基本 NuGet 包配置，但缺少完整的发布配置和自动化流程。为了将 Csanno 发布到 NuGet.org，需要：

1. 完善包元数据（许可证 URL、项目 URL、图标、标签等）
2. 添加符号包支持（便于调试）
3. 配置 GitHub Actions 自动化发布流程
4. 确保 NuGet 包符合最佳实践

## 变更内容

- 完善 `.csproj` 文件中的 NuGet 包元数据
- 添加 `README.md` 作为包描述页面
- 添加许可证文件
- 配置符号包生成
- 创建 GitHub Actions 工作流用于自动化打包和发布
- 添加本地打包验证命令

## 影响

- 受影响规范：`packaging`（NuGet 打包规范）
- 受影响代码：
  - `src/Csanno.csproj` - 添加完整包元数据
  - `LICENSE` - 添加 MIT 许可证文件
  - `.github/workflows/publish.yml` - 新建发布工作流
