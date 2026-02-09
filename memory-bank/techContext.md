# 技术上下文

## 技术栈
- **引擎**: Unity 6000 (Unity 6)
- **语言**: C#, .NET
- **框架**: ET9 框架
- **帧同步库**: TrueSync（定点数数学库）
- **序列化**: MemoryPack, MongoDB.Bson
- **热更新**: HybridCLR
- **资源管理**: YooAssets

## 编译方式
- **Unity 编辑器编译**: 标准 Unity 编译，有 `UNITY_EDITOR` 和 `ENABLE_VIEW` 宏
- **F6 编译（Player 编译）**: 使用 `PlayerBuildInterface.CompilePlayerScripts`，**没有 `UNITY_EDITOR` 宏**
  - 入口: `Packages/cn.etetet.loader/Editor/Helper/AssemblyTool.cs` → `MenuItemOfCompile()`
  - 输出目录: `Temp/Bin/Debug`
  - 编译的 DLL: `ET.Hotfix`, `ET.HotfixView`, `ET.Model`, `ET.ModelView`
  - 编译后复制到 CodeDir 供热更新加载
- **DotNet 编译**: 服务端编译，使用 `Packages/cn.etetet.core/DotNet~/ET.Core.csproj`，定义 `DOTNET` 宏

## 关键宏定义
- `ENABLE_VIEW`: 在 Unity 编辑器中启用 Entity 的 ViewGO 调试视图（ProjectSettings 中配置）
- `UNITY_EDITOR`: 仅在 Unity 编辑器中可用
- `DOTNET`: 仅在 .NET 服务端编译时可用
- `IS_COMPILING`: F6 编译时额外添加的宏

## 重要注意事项
- `Entity.ViewGO` 属性在 `#if ENABLE_VIEW && UNITY_EDITOR` 条件编译下，**F6 Player 编译时不可用**
- 不要在 Hotfix/HotfixView/Model/ModelView 层的代码中直接使用 `Entity.ViewGO`
- 需要持有 GameObject 引用时，应创建独立的 View Entity（如 `LSUnitView`、`LSMineView`）
