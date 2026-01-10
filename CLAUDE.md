# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

ET是一个为AI开发设计的Unity游戏框架,支持客户端服务端双端C#开发。使用Entity-Component-System架构、Fiber多线程系统、Actor消息机制,实现了完整的分布式游戏服务端和Unity客户端热更新。

**核心特点**:
- 客户端服务端代码共享,双端C#开发
- HybridCLR热更新,支持运行时热重载
- Fiber系统实现类似Erlang的轻量级进程
- 严格的代码分层和编译时分析器约束
- 全新的机器人测试框架,AI友好设计

## 开发环境

### 必需工具
- **Unity**: 6000.0.25 (必须此版本)
- **IDE**: Rider 2024.3或更高版本
- **.NET**: .NET 8 SDK
- **开发要求**: 必须全局翻墙(下载Unity包和NuGet包)

### 启动步骤

#### 客户端启动(Unity编辑器)
```bash
# 1. 打开Unity工程,选择ET目录
# 2. 安装demo包(二选一):
#    - cn.etetet.statesync (状态同步,推荐新手)
#    - cn.etetet.lockstep (帧同步)

# 3. 在Packages/manifest.json添加包依赖
"dependencies": {
  "cn.etetet.statesync": "0.0.47"
}

# 4. Unity菜单执行初始化
ET -> StateSync -> Init

# 5. 配置GlobalConfig
# Packages/ET.Loader/Resources/GlobalConfig:
#   - CodeMode: ClientServer (客户端服务端一体运行)
#   - SceneName: StateSync 或 LockStep

# 6. 配置YooAssets
# Packages/ET.YooAssets/Resources/YooConfig:
#   - EPlayMode: EditorSimulateMode

# 7. 打开C#工程并编译
Unity菜单 -> Assets -> Open C# Project
# 在Rider中编译整个ET.sln

# 8. 运行
双击 Packages/ET.Loader/Scenes/Init场景
点击Play按钮
```

#### 服务端独立启动
```bash
# 必须以管理员权限运行(需要开启HTTP服务)
# Unity菜单 -> ET -> Server Tools -> Start Server(Single Process)

# 或使用命令行:
cd ET
dotnet Bin/ET.App.dll --Console=1

# 注意: 工作目录必须在ET根目录(不是Bin目录)
```

### 编译命令

#### Unity中编译
```bash
# 按F6键 - 编译所有热更新DLL
# 或使用菜单: ET -> Build -> Compile
```

#### Rider/命令行编译
```bash
# 编译整个解决方案
dotnet build ET.sln

# 编译特定项目
dotnet build ET.Model.csproj
dotnet build ET.Hotfix.csproj

# 发布Linux服务端
powershell ./Scripts/Publish-linux-x64.ps1
```

### 热重载
```bash
# Unity运行时热重载(F7键):
1. 修改Hotfix/HotfixView层代码
2. 在Rider中编译(Ctrl+Shift+B)
3. Unity中按F7键或菜单: ET -> Reload

# 注意事项:
- Unity设置: Edit -> Preferences -> General -> ScriptChangesWhilePlaying
  选择 "RecompileAfterFinishedPlaying"
- 只能热重载Hotfix/HotfixView层,不能重载Model/ModelView层
```

## 代码架构

### 四层分离设计

ET框架将代码严格分为4层,通过Assembly Definition实现隔离:

#### 1. Core层 (ET.Core)
- **位置**: `Packages/cn.etetet.core/Scripts/Core/`
- **特点**: 不依赖Unity,纯C#实现
- **内容**: Entity、Fiber、网络(KCP/TCP)、序列化(MemoryPack)、对象池

#### 2. Loader层 (ET.Loader)
- **位置**: `Packages/cn.etetet.loader/Scripts/Loader/`
- **职责**: 启动引导、DLL加载、资源初始化
- **入口**:
  - 客户端: `Client/Init.cs` (Unity MonoBehaviour)
  - 服务端: `Server/Init.cs` (DotNet Main方法)

#### 3. Model层 (ET.Model + ET.ModelView)
- **位置**: 各Package的`Scripts/Model/`和`Scripts/ModelView/`
- **规则**: 只有数据字段,禁止编写方法
- **Model**: 纯数据组件(不依赖Unity)
- **ModelView**: Unity视图数据(引用GameObject、Transform等)

#### 4. Hotfix层 (ET.Hotfix + ET.HotfixView)
- **位置**: 各Package的`Scripts/Hotfix/`和`Scripts/HotfixView/`
- **规则**: 所有逻辑代码,可热更新
- **Hotfix**: 纯逻辑层
- **HotfixView**: Unity视图逻辑层

### 代码组织结构

```
Packages/cn.etetet.{包名}/
├── Scripts/                    # 热更新代码
│   ├── Model/
│   │   ├── Share/             # 双端共享数据
│   │   ├── Client/            # 客户端专用数据
│   │   └── Server/            # 服务端专用数据
│   ├── ModelView/             # Unity视图数据
│   ├── Hotfix/
│   │   ├── Share/             # 双端共享逻辑
│   │   ├── Client/            # 客户端逻辑
│   │   └── Server/            # 服务端逻辑
│   └── HotfixView/            # Unity视图逻辑
├── Runtime/                    # AOT代码(不可热更)
│   └── ET.{包名}.asmdef
├── Editor/                     # 编辑器工具
├── Excel/                      # 配置表
├── Proto/                      # 协议定义
└── DotNet~/                    # 服务端专用工程
    ├── Model/ET.Model.csproj
    └── Hotfix/ET.Hotfix.csproj
```

### 目录说明
- **Share**: 客户端服务端共享代码
- **Client**: 客户端专用(条件编译 `#if !DOTNET`)
- **Server**: 服务端专用(条件编译 `#if DOTNET`)
- **DotNet~**: 服务端独立工程(~号使Unity忽略)

## 核心系统

### Entity-Component-System

#### Entity设计原则
```csharp
// Model层 - 只有数据字段
[ComponentOf(typeof(Scene))]
public class PlayerComponent : Entity, IAwake, IDestroy
{
    public long PlayerId;
    public string Name;
    // 禁止编写方法!
}

// Hotfix层 - 所有逻辑都是扩展方法
[EntitySystemOf(typeof(PlayerComponent))]
public static partial class PlayerComponentSystem
{
    [EntitySystem]
    private static void Awake(this PlayerComponent self)
    {
        // 初始化逻辑
    }

    [EntitySystem]
    private static void Destroy(this PlayerComponent self)
    {
        // 清理逻辑
    }

    public static void SetName(this PlayerComponent self, string name)
    {
        self.Name = name;
    }
}
```

#### 重要规则
1. **Entity只能在Model/ModelView层定义,Hotfix层禁止定义Entity类**
2. **Model层的Entity禁止编写方法(分析器会检查)**
3. **必须使用ComponentOf特性指定父类型**
4. **生命周期方法使用接口标记**: IAwake、IDestroy、IUpdate、ILateUpdate
5. **所有逻辑使用扩展方法**: static class + static methods

### Fiber系统

Fiber是类似Erlang进程的轻量级执行单元:

```csharp
// 创建Fiber
int fiberId = await FiberManager.Instance.Create(
    SchedulerType.ThreadPool,  // Main/Thread/ThreadPool
    0,                          // Zone区服
    SceneType.NetClient,       // Scene类型
    "ClientNetworkFiber"       // 名称
);

// 获取Fiber的Root Scene
Scene root = FiberManager.Instance.Get(fiberId);

// 在Fiber中执行操作
root.GetComponent<ClientSenderComponent>().Send(message);
```

**调度类型**:
- **Main**: 主线程(Unity主线程)
- **Thread**: 独立线程(每个Fiber一个线程)
- **ThreadPool**: 线程池(多Fiber共享)

**应用场景**:
- 客户端网络独立Fiber(避免阻塞主线程)
- 服务端各类Scene(Realm、Gate、Map等)
- 帧同步房间Fiber
- 机器人测试Fiber(沙箱隔离)

### 网络系统

#### 网络协议
- **KCP**: 可靠UDP,低延迟,适合竞技游戏
- **TCP**: 可靠连接,稳定性好
- **WebSocket**: Web平台支持

#### Actor消息机制
```csharp
// 发送消息(Send)
ActorId targetId = new ActorId(process, fiberId, instanceId);
A2NetClient_Message msg = A2NetClient_Message.Create();
msg.MessageObject = yourMessage;
self.Root().GetComponent<ProcessInnerSender>().Send(targetId, msg);

// RPC调用(Call)
IResponse response = await self.Root().GetComponent<ProcessInnerSender>()
    .Call(targetId, request);

// 定义消息处理器
[MessageHandler(SceneType.NetClient)]
public class YourMessageHandler : MessageHandler<NetClientComponent, YourMessage>
{
    protected override async ETTask Run(NetClientComponent self, YourMessage message)
    {
        // 处理消息
    }
}
```

### 资源管理(YooAssets)

#### 配置模式
- **EditorSimulateMode**: 编辑器模拟(开发时使用)
- **OfflinePlayMode**: 离线模式(内置资源)
- **HostPlayMode**: 联机模式(CDN下载)
- **WebPlayMode**: WebGL模式

#### 资源加载
```csharp
// 通过ResourcesComponent加载
var dlls = await ResourcesComponent.Instance
    .LoadAllAssetsAsync<TextAsset>("路径");

// 配置文件
Packages/ET.YooAssets/Resources/YooConfig
```

### 配置表系统

```bash
# Excel表格位置
Packages/cn.etetet.*/Excel/*.xlsx

# 导出配置
Unity菜单 -> ET -> Excel -> Export

# 生成路径
Config/Excel/      # 服务端二进制
Config/Json/       # 客户端JSON
```

### 协议定义(Proto)

```bash
# 协议文件位置
Packages/cn.etetet.*/Proto/*.proto

# 生成代码
Unity菜单 -> ET -> Proto -> Export

# 生成位置
Packages/cn.etetet.proto/Scripts/Model/Share/
```

## 编程规范和分析器

ET框架包含20+个编译时分析器,强制约束代码规范:

### 重要规范

#### 1. Entity相关
- **ET0001**: Entity类只能在Model/ModelView层定义
- **ET0002**: Entity类禁止定义方法(只能有字段)
- **ET0003**: Entity字段禁止直接访问(必须通过扩展方法)
- **ET0004**: Entity必须使用`[ComponentOf(typeof(Parent))]`特性
- **ET0005**: 禁止对Entity使用GetHashCode()
- **ET0006**: 禁止直接new Entity(必须用Entity.Create或AddComponent)

#### 2. 异步相关
- **ET0007**: async方法必须返回ETTask/ETTask<T>(不能是Task)
- **ET0008**: 禁止使用CancellationToken(使用ETCancellationToken)
- **ET0009**: ETTask必须await或Coroutine()

#### 3. 层级隔离
- **ET0010**: Hotfix层禁止定义class(除了System类)
- **ET0011**: 客户端代码不能出现在服务端专用程序集
- **ET0012**: Model层禁止定义普通class(只能Entity、IMessage等)
- **ET0013**: Hotfix层禁止定义字段(除了static readonly)

#### 4. 消息相关
- **ET0014**: 网络消息必须使用[MemoryPackable]和partial
- **ET0015**: 网络消息的ResponseType必须继承IResponse

#### 5. 其他
- **ET0016**: 静态类循环依赖检测
- **ET0017**: UniqueId特性检查

### 违反规范的示例(禁止)

```csharp
// ❌ 错误: Entity定义在Hotfix层
// Hotfix/MyComponent.cs
public class MyComponent : Entity { } // 违反ET0001

// ❌ 错误: Entity包含方法
// Model/PlayerComponent.cs
public class PlayerComponent : Entity
{
    public void DoSomething() { } // 违反ET0002
}

// ❌ 错误: 直接访问Entity字段
// Hotfix/SomeSystem.cs
player.Name = "test"; // 违反ET0003
// 应该: player.SetName("test");

// ❌ 错误: 直接new Entity
var entity = new MyComponent(); // 违反ET0006
// 应该: var entity = self.AddComponent<MyComponent>();

// ❌ 错误: async返回Task
public async Task DoSomethingAsync() { } // 违反ET0007
// 应该: public async ETTask DoSomethingAsync() { }

// ❌ 错误: Hotfix层定义字段
// Hotfix/SomeSystem.cs
public static class SomeSystem
{
    private static int count; // 违反ET0013
}
```

### 正确的代码示例

```csharp
// ✅ Model层: 定义数据
[ComponentOf(typeof(Scene))]
public class PlayerComponent : Entity, IAwake<string>
{
    public string Name;
    public int Level;
}

// ✅ Hotfix层: 实现逻辑
[EntitySystemOf(typeof(PlayerComponent))]
public static partial class PlayerComponentSystem
{
    [EntitySystem]
    private static void Awake(this PlayerComponent self, string name)
    {
        self.Name = name;
    }

    public static void LevelUp(this PlayerComponent self)
    {
        self.Level++;
    }
}

// ✅ 使用Entity
Scene scene = self.Root();
PlayerComponent player = scene.AddComponent<PlayerComponent, string>("张三");
player.LevelUp();
```

## 常见任务

### 创建新的Entity组件
```bash
# 1. 在Model层定义数据
Packages/你的包/Scripts/Model/Share/YourComponent.cs

# 2. 在Hotfix层实现逻辑
Packages/你的包/Scripts/Hotfix/Share/YourComponentSystem.cs

# 3. 编译后自动生成System类模板
```

### 添加网络消息
```bash
# 1. 定义Proto文件
Packages/你的包/Proto/YourMessage.proto

# 2. 生成代码
Unity菜单 -> ET -> Proto -> Export

# 3. 实现消息处理器
[MessageHandler(SceneType.YourScene)]
public class YourMessageHandler : MessageHandler<Component, YourMessage>
{
    protected override async ETTask Run(Component self, YourMessage message)
    {
        // 处理逻辑
    }
}
```

### 添加配置表
```bash
# 1. 创建Excel文件
Packages/你的包/Excel/YourConfig.xlsx

# 2. 按照ET格式填写(第一行字段名,第二行类型,第三行注释)

# 3. 导出
Unity菜单 -> ET -> Excel -> Export

# 4. 使用配置
var config = ConfigComponent.Instance.GetById<YourConfig>(id);
```

### 创建机器人测试
```csharp
// 服务端控制台命令
CreateRobot --Num=10  // 创建10个机器人

// 机器人代码位置
Packages/cn.etetet.robot/Scripts/Hotfix/Server/
```

### 打包发布

#### 客户端打包
```bash
# 1. HybridCLR安装
HybridCLR -> Installer -> Install

# 2. 编译DLL
右键ET.sln -> 编译

# 3. 生成AOT元数据
HybridCLR -> Generate -> All

# 4. 复制AOT DLLs
ET -> HybridCLR -> CopyAotDlls

# 5. 构建AssetBundle
YooAsset -> AssetBundle Builder
  - BuildPipeline: ScriptableBuildPipeline
  - BuildMode: IncrementalBuild
  - 点击 Click Build

# 6. 打包可执行文件
ET -> BuildTool -> BuildPackage
生成位置: Release/
```

#### 服务端发布
```bash
# Linux服务端
powershell ./Scripts/Publish-linux-x64.ps1

# 输出目录
Bin/
```

## 重要配置文件

```bash
# 全局配置
Packages/ET.Loader/Resources/GlobalConfig
  - CodeMode: Client/Server/ClientServer
  - SceneName: demo名称

# YooAssets配置
Packages/ET.YooAssets/Resources/YooConfig
  - EPlayMode: 资源加载模式

# 服务器配置
Packages/cn.etetet.startconfig/Excel/StartConfig/
  - StartMachineConfig.xlsx: 物理机配置
  - StartProcessConfig.xlsx: 进程配置
  - StartSceneConfig.xlsx: 场景配置
```

## 关键入口文件

```bash
# 客户端启动
Packages/cn.etetet.loader/Scripts/Loader/Client/Init.cs

# 服务端启动
Packages/cn.etetet.loader/Scripts/Loader/Server/Init.cs

# 框架初始化
Packages/cn.etetet.core/Scripts/Model/Share/Entry.cs

# 代码加载器
Packages/cn.etetet.loader/Scripts/Loader/Client/CodeLoader.cs
Packages/cn.etetet.loader/Scripts/Loader/Server/CodeLoader.cs
```

## 关键概念

### CodeMode
- **Client**: 纯客户端模式(连接独立服务器)
- **Server**: 纯服务端模式
- **ClientServer**: 一体化模式(Unity中同时运行客户端和服务端,开发推荐)

### SceneType
定义不同类型的Scene:
- **Main**: 主场景(客户端主Fiber)
- **NetClient**: 网络客户端Fiber
- **Realm**: 登录服务器
- **Gate**: 网关服务器
- **Map**: 地图服务器
- **Location**: 位置服务器
- **Router**: 路由服务器(软路由防攻击)

### Domain和IScene
- **IScene**: 标记接口,实现它的Entity就是Domain
- **Domain**: Fiber的根节点,所有Entity通过`self.Root()`获取Domain
- **Scene**: 最常用的Domain实现

## 调试技巧

### Unity中调试
```csharp
// 开启ENABLE_VIEW宏查看Entity树
// Project Settings -> Player -> Scripting Define Symbols
// 添加: ENABLE_VIEW

// Unity Hierarchy面板可见:
Init/
  Global/
  Scene(Process)/
    Fiber_xxx/
      Entity树结构
```

### 服务端调试
```bash
# 启动时带调试参数
dotnet Bin/ET.App.dll --Console=1

# REPL模式(动态执行代码)
# 在控制台输入: repl
```

### 日志查看
```bash
# 日志位置
Logs/
  Debug-xx.log    # 调试日志
  Error-xx.log    # 错误日志
  Warning-xx.log  # 警告日志
```

## 常见问题

### 编译错误
1. 确保.NET 8已安装
2. 确保Rider更新到2024.3+
3. 确保全局翻墙(NuGet包下载)
4. 清理后重新编译: `dotnet clean && dotnet build`

### 启动错误
1. Unity版本必须是6000.0.25
2. 必须运行过`ET -> Init`初始化
3. 检查GlobalConfig配置是否正确
4. 查看Logs/Error日志定位问题

### 热重载失败
1. 确保只修改了Hotfix/HotfixView层
2. 确保代码已编译成功
3. Unity设置正确(RecompileAfterFinishedPlaying)

### 服务端启动失败
1. 必须以管理员权限运行(HTTP服务需要)
2. 工作目录必须是ET根目录(不是Bin/)
3. 检查端口是否被占用

## AI开发注意事项

ET框架专门为AI开发设计,在使用Claude Code等AI工具时:

1. **严格遵守四层分离**: Model层只定义数据,Hotfix层实现逻辑
2. **使用分析器提示**: 编译错误会精确指出违反的规范
3. **使用EntitySystemOf**: 自动生成System类模板,减少模板代码
4. **利用机器人框架**: 编写测试用例验证功能
5. **参考Share目录**: 双端共享代码的最佳实践
6. **使用Fiber隔离**: 每个测试或功能模块可以独立Fiber运行

## 学习资源

- **官方文档**: README.md中的视频链接
- **运行指南**: Book/1.1运行指南.md
- **Package制作**: Book/8.1ET Package制作指南.md
- **ET论坛**: https://et-framework.cn
- **分析器说明**: https://www.yuque.com/u28961999/yms0nt/
