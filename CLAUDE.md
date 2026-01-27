# 项目 上下文

## 目的
在 Autofac 上实现类似 Java Spring 的注解式组件注册功能

## 技术栈
- C#

## 已有功能
- 注解式组件注册
- 编译期组件扫描
- 运行时组件扫描
- 组件生命周期管理（与Autofac保持一致）
- 自定义Aop切面织入
- Aop支持编译期生成（生成代理类）

## 项目约定

### 代码风格
- 驼峰命名
- 遵循一般c#的命名规范
- 尽量使用新的语言特性

### 架构模式
本项目为类库，无特殊架构指定

### 测试策略

测试框架：NUnit
组件注入部分均需单元测试，测试边界条件
Test-Driven Development (TDD)： 在修改核心逻辑前，请先为我生成测试用例。

### Git工作流
- 分支策略：另建分支开发，合并前请进行代码审查
    - 分支命名策略：
        - feature/xxx
        - fix/xxx
        - release/xxx
        - hotfix/xxx
    其中xxx为功能名称或修复内容
- 提交信息：参考已有的提交信息，commit message中建议描述本次改动，若改动涉及issue，请在提交信息中引用issue，格式为`#xxx`。例如：关于`issue #123`则填写message `fix @issue#123: 修复...`或者`feat @issue#123: 添加...`。
- 自动化检查：每次提交前运行dotnet test测试，保证无错误
- 文档同步：提交修改前，修改README.md和CONTRIBUTING.md（如有需要），确保目录树和模块等内容与项目同步
- 单纯的文档修改，直接提交到main分支即可。

完成需求审查通过后，非main分支的改动，直接提交到远程仓库

## CI/CD
项目已在Github上创建仓库，使用Github Action进行CI/CD，在Environment:Pub中配置了Nuget Api Key。

如有部署等改动，修改@/.github/workflows/publish.yml
