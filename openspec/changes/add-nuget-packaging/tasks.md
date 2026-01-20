## 1. 配置项目文件

- [ ] 1.1 更新 `src/Csanno.csproj` 添加完整的 NuGet 包元数据
- [ ] 1.2 添加 README.md 作为包描述
- [ ] 1.3 配置符号包生成选项
- [ ] 1.4 配置包发布选项（IncludeSymbols、SymbolPackageFormat）

## 2. 添加许可证文件

- [ ] 2.1 创建 `LICENSE` 文件（MIT 许可证）
- [ ] 2.2 在 `.csproj` 中配置许可证引用

## 3. 创建 GitHub Actions 工作流

- [ ] 3.1 创建 `.github/workflows/publish.yml` 文件
- [ ] 3.2 配置打包和发布步骤
- [ ] 3.3 添加版本标签解析逻辑
- [ ] 3.4 配置 NuGet API 密钥使用 GitHub Secrets

## 4. 本地验证

- [ ] 4.1 执行 `dotnet pack` 生成包
- [ ] 4.2 验证生成的 `.nupkg` 文件内容
- [ ] 4.3 验证生成的 `.snupkg` 符号包
- [ ] 4.4 使用 `dotnet nuget verify` 验证包

## 5. 文档更新

- [ ] 5.1 更新 README.md 添加安装说明（NuGet 命令）
- [ ] 5.2 添加 CONTRIBUTING.md 说明发布流程（可选）
