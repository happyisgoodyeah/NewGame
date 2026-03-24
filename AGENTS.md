# AGENTS.md

## 项目定位

- Unity: `2022.3.62f3c1`
- 语言: `C#`，编码按 `UTF-8`
- 框架: `ET` + `YIUI` + `YooAssets` + `HybridCLR` + `Luban`
- 这是 ET/YIUI 模板工程，主代码区在 `Packages/cn.etetet.*`，不是 `Assets/Scripts`

## 核心工作流

- 入口场景: `Packages/cn.etetet.loader/Scenes/Init.unity`
- 常用操作:
  - `F6` -> `ET/Loader/Compile`
  - `F7` -> `ET/Loader/Reload`
  - `ET/Loader/Build Tool`
  - `ET/Excel/ExcelExporter`
  - `ET/YIUI 自动化工具`
  - `ET/YIUI Luban 配置工具`
- 当前 `GlobalConfig.asset`:
  - `CodeMode = ClientServer`
  - `SceneName = StateSync`
  - `Address = 127.0.0.1:10101`
- F6 后热更 DLL 会刷新到 `Packages/cn.etetet.loader/Bundles/Code`
- 中间输出目录: `Temp/Bin/Debug`
- AOT metadata 目录: `Packages/cn.etetet.loader/Bundles/AotDlls`

## 主要目录

- `Packages/cn.etetet.core`: ET 核心实体、事件、Fiber、网络、序列化
- `Packages/cn.etetet.loader`: Unity 入口、CodeLoader、编译/构建、CodeMode
- `Packages/cn.etetet.statesync`: ET 示例业务
- `Packages/cn.etetet.yiuiframework`: YIUI 运行时与工具
- `Packages/cn.etetet.yiuiyooassets`: YIUI 与 YooAssets 桥接
- `Packages/cn.etetet.yiuiluban`: Luban 工具链
- `Packages/cn.etetet.yiuilubangen`: Luban 生成产物与加载实现
- `Packages/cn.etetet.yiuistatesync`: 当前主要 UI 示例业务
- `Packages/gamebase`: 项目基础层，放可复用、可迁移到其他项目的基础代码
- `Packages/gameplay`: 项目业务层，放当前项目专属玩法、流程、界面与业务逻辑

## ET 设计规则

### 数据与结构

- ET 是树状 Entity 结构，不是扁平 ECS
- 关系只有两类:
  - `Parent -> Child`
  - `Entity -> Component`
- 先决定数据挂在哪棵树上，再决定是否拆组件
- 通用稳定字段放实体本体；可插拔能力拆组件

### Entity 理解

- `Entity` 在 ET 里首先是“运行时数据树节点”，不是只代表纯数据类，也不是只代表 Unity 表现对象
- `Child` 适合“结构上的下级对象”:
  - 有独立 `Id`
  - 可以有多个同类型子节点
  - 生命周期跟随父节点
  - 适合 `Grid -> Slot`、`Puzzle -> Slot` 这种层级归属关系
- `Component` 适合“挂在实体上的能力或表现”:
  - 通过 `GetComponent<T>()` 按类型获取
  - 同一实体下通常同类型只挂一个
  - 适合 `PuzzleView`、`GridView`、`AudioComponent` 这类功能或表现扩展
- 是否设计成 `Child` 还是 `Component`，优先看语义:
  - 它是不是父对象组成结构的一部分
  - 它是不是一种可插拔能力/表现
  - 它是否需要同类型多实例
- 推荐分三层理解:
  - 配置定义: Luban/配置表数据，不一定是 `Entity`
  - 运行时模型: `Entity`
  - 运行时表现: 由运行时模型驱动的 `Component Entity` 或 View 相关实体
- 数据驱动表现的常见方式不是“只有数据和表现两层”，而是:
  - 配置生成运行时实体
  - 运行时实体驱动 View / 表现组件
- View 命名保持简洁:
  - 表现层组件优先命名为 `GridView`、`PuzzleView`、`SlotView`
  - 不额外追加 `Component` 后缀；其本质仍然是 ET `Component`
- 数据层驱动表现层:
  - `Grid`、`Puzzle`、`Slot` 等数据实体创建后，优先通过事件通知表现层生成对应 `View`
  - 避免由场景总控或某个父级 `View` 直接代替子数据批量创建全部表现
  - 推荐模式是 `AfterCreateXxx -> CreateXxxView`

### 代码组织

- 实体类优先只放数据
- 逻辑优先放静态 `System` / `Handler` / `Helper`
- 如果一个组件设计为通用复用能力，命名也应保持通用，避免带入当前项目或当前玩法的专属语义
- 新功能优先组织为:
  - `Entity/Component`
  - `static partial *System`
  - 事件 / handler
- 常见属性:
  - `[ComponentOf]`
  - `[ChildOf]`
  - `[FriendOf]`
  - `[EntitySystemOf]`
  - `[EntitySystem]`
- ET 分析器规范:
  - 静态字段需要声明 `StaticField`
  - `Hotfix` 程序集中不允许声明非 `const` 静态字段
  - `Hotfix` 中需要静态只读数据时，优先改为局部变量、配置数据，或迁移到非 `Hotfix` 程序集
  - 遇到 `ET0013` 静态类函数引用环依赖时，静态 `System/Helper` 之间必须改成单向依赖；不要在两个静态类里互相调用扩展函数，优先直接读取实体字段、父子关系或类型信息来完成判断
  - 遇到 `ET0032` 时，`Model/ModelView` 程序集里声明的非 ET Object/Entity 类需要显式加 `[EnableClass]`，例如 `MonoBehaviour`、普通辅助类或桥接类，否则分析器会禁止声明

### 四层分工

- `Model`: 纯数据、配置、协议相关
- `ModelView`: 客户端桥接、轻量 Unity 相关、部分生成代码
- `Hotfix`: 业务逻辑、消息、服务端逻辑
- `HotfixView`: UI、表现、资源加载、Unity 强相关逻辑

## 事件与异步

- ET 逻辑默认是事件驱动 + 单线程异步
- 模块联动顺序:
  - 改数据
  - 抛事件 / Publish
  - 订阅方各自处理
- `Publish/PublishAsync` 是广播
- `Invoke` 是强约束调用
- `await` 不等于多线程
- 优先使用:
  - `ETTask`
  - `TimerComponent`
  - `EventSystem`
  - Mailbox 队列
- 不要把默认业务写成多线程共享状态模型
- 对象跨 `await` 后可能已释放、复用或迁移；长链异步要注意生命周期

## Scene / 启动

- `ET.Entry.Start()` -> 创建主 Fiber
- `FiberInit_Main` -> 发布 `EntryEvent1/2/3`
- 当前根场景由 `Options.Instance.SceneName` 决定
- 当前默认是 `StateSync`
- `EntryEvent3_InitClient` 会挂:
  - `GlobalComponent`
  - `ResourcesLoaderComponent`
  - `PlayerComponent`
  - `CurrentScenesComponent`
  - `YIUIMgrComponent`
- ET 的 `Scene` 是 ET 数据树中的逻辑场景实体，不是 Unity 场景对象
- Unity 的 `Scene` 属于 `UnityEngine.SceneManagement`
- 需要获取 `Transform`、`GameObject`、场景根节点时，必须显式通过 Unity 场景 API 或场景桥接组件处理
- 不要把 ET `Scene` 当作 Unity `Scene` 使用；代码中涉及两者时应明确区分命名，例如 `etScene` / `unityScene`
- `GameObjectEntityRef` 是现成的 Unity 到 ET 桥接组件：挂在 `GameObject` 上后，可通过其 `Entity` 属性取回关联的 ET `Entity`；内部基于 `EntityRef<Entity>`，当实体已释放或 `InstanceId` 失效时会自动返回空，适合用于输入命中后的 `GameObject -> Entity` 反查

## 序列化

- ET 核心依赖 MongoBson 思路，一套结构尽量同时适配配置、日志、数据库、消息
- 设计可序列化对象时优先使用:
  - `[BsonIgnore]`
  - `[BsonElement]`
  - `[BsonIgnoreExtraElements]`
- 反序列化后的派生数据恢复优先放:
  - `BeginInit`
  - `EndInit`
- 当前仓库里 `MongoHelper`、`MemoryPackHelper` 已处理 `ISupportInitialize`

## Luban

- 项目已接入 Luban
- 关键位置:
  - `Packages/cn.etetet.yiuiluban`
  - `Packages/cn.etetet.yiuilubangen`
  - `Packages/cn.etetet.yiuilubangen/Assets/LubanGen`
- 运行时加载入口:
  - `Packages/cn.etetet.yiuiluban/Scripts/Model/Share/Config/LubanConfigLoader.cs`
- 生成代码位置:
  - `Packages/cn.etetet.yiuilubangen/CodeMode/Model/*/LubanGen`
- 二进制配置位置:
  - `Packages/cn.etetet.yiuilubangen/Assets/LubanGen/Config/Binary/...`
  - `Packages/cn.etetet.yiuilubangen/Assets/LubanGen/StartConfig/...`
- Luban 配置类型一般实现 `ILubanConfig`
- 不要手改 `LubanGen` 生成代码或导出的 `.bytes`
- 配表不生效时优先:
  - 重新执行 `ET/Excel/ExcelExporter`
  - 检查 `LubanGen` 和 `Binary` 产物
  - 检查当前 `CodeMode`

## Actor / Actor Location

- Actor 是“挂了 `MailboxComponent` 的 Entity”
- 普通 Actor:
  - 已知 `InstanceId`
  - 直接发到目标 Entity
- Actor Location:
  - 已知稳定 `Entity.Id`
  - 通过 Location 服务查当前 `InstanceId`
  - 适合会迁移、换线、换场景、换进程的对象
- Mailbox 是串行队列，注意 `A -> B -> C -> A` 这类死锁链
- 不要绕过现有 sender 组件自己实现定位/重试

## Numeric

- 复杂属性统一走 `NumericComponent + NumericType`
- 不要退回“大量字段 Numeric 类”
- Buff、装备、被动、配置修改都应尽量通过 `NumericType` 驱动
- 监听属性变化优先用数值变化事件或 `NumericWatcher`

## AI

- ET 更偏“行为机”而不是复杂状态机网
- 规则:
  - 节点描述一种行为
  - 节点只关心当前条件
  - 满足条件时打断旧行为，进入新行为
  - 共享的是函数，不是节点
  - 协程必须支持取消
- 当前仓库对应结构:
  - `AIComponent`
  - `AIDispatcherComponent`
  - `AAIHandler.Check`
  - `AAIHandler.Execute`

## YIUI

- YIUI 典型文件分三类:
  - `*ComponentGen.cs`
  - `*ComponentSystemGen.cs`
  - 手写 `*ComponentSystem.cs`
- `YIUIGen` 目录视为生成代码区
- 正常业务逻辑写在 `YIUISystem`
- 不要手改生成文件，除非你在修生成器本身
- 改 prefab 结构、绑定名、资源后通常需要重新生成

## 修改建议

- 后续新增代码优先落在 `Packages/gamebase` 或 `Packages/gameplay`
- 可复用的基础能力、通用组件、基础工具、基础框架扩展放 `Packages/gamebase`
- 当前项目专属玩法、业务流程、业务 UI、项目定制配置放 `Packages/gameplay`
- 纯 ET 业务: 放到业务包的 `Model / ModelView / Hotfix / HotfixView`
- UI 业务: 用 YIUI 工具生成组件，手写逻辑放 `YIUISystem`
- 扩展示例 UI: 优先在 `Packages/cn.etetet.yiuistatesync`
- 正式项目业务: 更推荐新增独立业务包

## 禁区

- 不要把主要业务代码放进 `Assets/Scripts`
- 不要手改 `AssemblyReference.asmref` 作为长期方案
- 不要直接修改 `YIUIGen/*Gen.cs`
- 不要在不了解 `SceneType` 时乱挂事件
- 不要绕开 `NumericComponent`
- 不要把 AI 写成大量碎节点去模仿状态机/行为树
- 不要长期信任可迁移对象的旧地址，涉及迁移优先走 Actor Location
- 不要手改 Luban 生成代码或 `.bytes`

## 快速排查

### 启动失败

1. 确认场景是否为 `Packages/cn.etetet.loader/Scenes/Init.unity`
2. 确认是否执行过 `F6`
3. 确认 `Packages/cn.etetet.loader/Bundles/Code` 下 DLL bytes 是否更新
4. 确认 `GlobalConfig.asset` 的 `CodeMode`
5. 确认 `SceneName`

### UI 不显示

1. 看 `EntryEvent3_InitClient` 是否执行
2. 看 `YIUIMgrComponent.Initialize()` 是否成功
3. 看 `UIRoot` 是否存在或加载成功
4. 看事件是否命中正确 `SceneType`
5. 看 YIUI 绑定和 prefab

### 改了代码但还是旧逻辑

1. `F6`
2. 检查 `Packages/cn.etetet.loader/Bundles/Code`
3. 仅 hotfix 改动可尝试 `F7`
4. 涉及 CodeMode / 配表 / 生成代码时，通常还要刷新或重新生成

## 关键文件

- `Packages/cn.etetet.loader/Scenes/Init.unity`
- `Packages/cn.etetet.loader/Resources/GlobalConfig.asset`
- `Packages/cn.etetet.core/Scripts/Model/Share/Entry.cs`
- `Packages/cn.etetet.core/Scripts/Model/Share/FiberInit_Main.cs`
- `Packages/cn.etetet.core/Scripts/Core/Share/Entity/Entity.cs`
- `Packages/cn.etetet.core/Scripts/Core/Share/World/EventSystem/EventSystem.cs`
- `Packages/cn.etetet.loader/Scripts/Loader/Client/CodeLoader.cs`
- `Packages/cn.etetet.loader/Editor/Helper/AssemblyTool.cs`
- `Packages/cn.etetet.yiuiluban/Scripts/Model/Share/Config/LubanConfigLoader.cs`
- `Packages/cn.etetet.yiuilubangen/Scripts/Hotfix/Server/LubanServerLoaderInvoker.cs`
- `Packages/cn.etetet.yiuilubangen/Scripts/HotfixView/Client/LubanClientLoaderInvoker.cs`
- `Packages/cn.etetet.yiuiframework/Scripts/HotfixView/Client/System/UIMgr/YIUIMgrComponentSystem_Initialize.cs`
- `Packages/cn.etetet.yiuistatesync/Scripts/HotfixView/Client/EntryEvent3_InitClient.cs`

## 一句话总结

- 这是一个以 `Packages/cn.etetet.*` 为主体的 ET 模板工程；默认按树状 Entity、静态 System、事件驱动、单线程异步、Luban/YIUI 生成代码只读、Numeric/Actor/AI 走框架既定模式来开发。

### 注释规范

- C# 新增或改动的函数，按项目规范补充 XML 注释，优先使用 `/// <summary>`。
- 该要求同时适用于 `private`/局部职责 helper、重构时新拆出的承接函数、以及容易被误判为“实现细节”的桥接入口；不要只给 `public` 方法补注释。
- 如果一个函数是在本次改动中新增，或本次改动显著改变了它的职责/行为边界，则视为“需要补注释”，不能因为文件里已有旧代码风格而跳过。
- 需要说明入参、返回值或泛型约束时，补充 `param`、`returns`、`typeparam`。
- 注释应描述职责和行为边界，不写无信息量的逐行翻译式注释。

