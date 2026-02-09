# 系统架构与设计模式

## ET 框架程序集分层架构

ET 框架将代码分为 4 个程序集层，每层有明确职责：

```
┌─────────────────────────────────────────────────────┐
│  HotfixView (ET.HotfixView)                         │
│  - 客户端 View 层逻辑（可热更新）                      │
│  - 包含 System 类（如 LSUnitViewSystem）              │
│  - 可引用 UnityEngine                                │
│  - 路径: Scripts/HotfixView/Client/                  │
│  - AssemblyReference: ET.HotfixView                  │
├─────────────────────────────────────────────────────┤
│  Hotfix (ET.Hotfix)                                  │
│  - 共享逻辑层（可热更新）                              │
│  - 包含 System 类（如 LSMineComponentSystem）         │
│  - 路径: Scripts/Hotfix/Share/ 或 Client/ 或 Server/ │
│  - AssemblyReference: ET.Hotfix                      │
├─────────────────────────────────────────────────────┤
│  ModelView (ET.ModelView)                            │
│  - 客户端 View 层数据定义                             │
│  - 包含 Entity/Component 类（如 LSUnitView）          │
│  - 可引用 UnityEngine（GameObject, Transform 等）     │
│  - 路径: Scripts/ModelView/Client/                   │
│  - AssemblyReference: ET.ModelView                   │
│  - 命名空间: 通常用 ET（不是 ET.Client）              │
├─────────────────────────────────────────────────────┤
│  Model (ET.Model)                                    │
│  - 共享数据层（客户端+服务端共用）                      │
│  - 包含 Entity/Component 类（如 LSUnit, LSMine）      │
│  - 路径: Scripts/Model/Share/ 或 Client/ 或 Server/  │
│  - AssemblyReference: ET.Model                       │
│  - 命名空间: ET                                      │
└─────────────────────────────────────────────────────┘
```

## 帧同步（LockStep）核心架构

### 核心类关系
```
Room (IScene)
├── LSWorld (帧同步世界)
│   ├── LSUnitComponent (管理所有玩家单位)
│   │   └── LSUnit (玩家单位, LSEntity)
│   │       └── LSInputComponent
│   └── LSMineComponent (管理所有地雷)
│       └── LSMine (地雷, LSEntity)
├── LSUnitViewComponent (管理所有单位视图, Entity)
│   └── LSUnitView (单位视图, Entity)
│       └── LSAnimatorComponent
├── LSMineViewComponent (管理所有地雷视图, Entity)
│   └── LSMineView (地雷视图, Entity)
├── LSCameraComponent
├── LSOperaComponent
└── FrameBuffer, Replay 等
```

### 逻辑层 vs 视图层分离模式
- **逻辑层** (`LSEntity`): 在 `LSWorld` 下，参与帧同步序列化/反序列化，使用定点数 `TSVector`
- **视图层** (`Entity`): 在 `Room` 下，不参与帧同步，使用 Unity `Vector3`/`GameObject`
- 视图层通过 `EntityRef<T>` 引用逻辑层实体，通过 ID 匹配（`AddChildWithId` 使用相同 ID）

### View Entity 创建模式（重要参考）
以 `LSUnitView` 为例：
1. **ModelView 层定义数据类**: `LSUnitView : Entity, IAwake<GameObject>, IUpdate, ILSRollback`
   - 持有 `GameObject`, `Transform`, `EntityRef<LSUnit>`
2. **ModelView 层定义管理组件**: `LSUnitViewComponent : Entity, IAwake, IDestroy`
   - `[ComponentOf(typeof(Room))]`
3. **HotfixView 层实现初始化**: `LSUnitViewComponentSystem.InitAsync()`
   - 遍历逻辑层实体，创建 GameObject，`AddChildWithId<LSUnitView, GameObject>(lsUnit.Id, unitGo)`
4. **HotfixView 层实现更新**: `LSUnitViewSystem.Update()`
   - 通过 `GetUnit()` 获取逻辑层实体，同步位置/旋转到 Transform
5. **场景初始化注册**: `LSSceneInitFinish_Finish` 中 `room.AddComponent<LSUnitViewComponent>().InitAsync()`

## Entity 系统关键点

### Entity 基类
- `Entity` → `LSEntity`（帧同步实体）
- `Entity.ViewGO`: **仅在 `#if ENABLE_VIEW && UNITY_EDITOR` 下可用**，用于编辑器调试，不可在业务代码中使用

### 属性标记
- `[EntitySystemOf(typeof(T))]`: 标记 System 类对应的 Entity 类型
- `[LSEntitySystemOf(typeof(T))]`: 标记 LSEntity 的 System（帧同步相关）
- `[FriendOf(typeof(T))]`: 允许访问 Entity 的私有/内部成员
- `[ComponentOf(typeof(T))]`: 标记组件挂载的父实体类型
- `[ChildOf(typeof(T))]`: 标记子实体的父实体类型

### System 方法标记
- `[EntitySystem]`: 标记 Awake/Update/Destroy 等生命周期方法
- `[LSEntitySystem]`: 标记 LSUpdate/LSRollback 等帧同步方法

## 场景初始化流程
1. `RoomSystem.Init()` → 创建 `LSWorld`，添加 `LSUnitComponent`、`LSMineComponent`
2. `LSSceneInitFinish_Finish.Run()` → 创建 View 层组件
   - `room.AddComponent<LSUnitViewComponent>().InitAsync()`
   - `room.AddComponent<LSMineViewComponent>().Init()`
   - `room.AddComponent<LSCameraComponent>()`
   - `room.AddComponent<LSOperaComponent>()`

## 包结构
主要包位于 `Packages/cn.etetet.*`：
- `cn.etetet.core`: 核心框架（Entity, Fiber, EventSystem 等）
- `cn.etetet.lockstep`: 帧同步模块（本项目主要开发包）
- `cn.etetet.lsentity`: LSEntity 基类和 LSEntitySystem
- `cn.etetet.truesync`: TrueSync 定点数数学库
- `cn.etetet.loader`: 代码加载器、F6 编译工具
- `cn.etetet.proto`: 协议定义
- `cn.etetet.login`: 登录模块
- `cn.etetet.ui`: UI 框架
