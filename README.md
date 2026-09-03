# 框架内容概述

Unity 客户端框架。用 HybridCLR 做代码热更，用 YooAsset 做资源热更，启动流程、UI、配置、网络和 SDK 都收在同一套程序集分层里。

| 项 | 值 |
| --- | --- |
| 引擎 | Unity 2022.3.51f1c1 |
| 启动场景 | `Assets/Scenes/Start.unity` |
| 主目标 | Android（最低 API 22），竖屏 |

## 能力概览

- **代码热更**：HybridCLR 编译 `HotUpdate` 程序集，产物以 `.bin` 放进 `GameAssets/DLL`，随资源包下发。
- **资源热更**：YooAsset 2.3.18，资源包名 `MyPackage`。编辑器走 `EditorSimulateMode`，真机走 `HostPlayMode`，远端地址由 `RemoteServices` 提供。
- **启动状态机**：`Launcher` 依次执行初始化资源系统、检查清单、下载资源、热更结束，再进入游戏。
- **UI**：分层 Canvas（`UI_Root/Canvas_n/Ts_Panel`），`UIManager` 管理面板生命周期，热更侧用 `HotUpdateUtils.OpenUIPrefabPanel` 打开界面。
- **配置**：Excel 导出为 `.bin`，运行时按 `Config_{表名}` 地址加载；多语言读 `Language` 表。
- **网络**：`MessageNetManager` 负责与服务器通信。
- **SDK**：接入第三方登录。
- **实体裁剪**：`CullingGroupManager` 按包围球控制场景对象显隐。

## 程序集分层

| 程序集 | 位置 | 职责 |
| --- | --- | --- |
| `Invariable` | `Assets/Scripts/Invariable` | AOT 常驻：启动、资源、管理器、通用组件。包体更新才能改。 |
| `HotUpdate` | `Assets/Scripts/HotUpdate` | 热更逻辑：登录等业务 UI、开局流程。只引用 `Invariable`。 |
| `MyTools` | `Assets/Editor/MyTools` | 编辑器工具：配置导出、图集、打包等。 |

`Invariable` 依赖 YooAsset、UniTask、DOTween、TextMesh Pro、ExcelDataReader 等。`HotUpdate` 被 HybridCLR 标为热更程序集。

## 目录

```
CometRider/
├── Assets/
│   ├── Scenes/Start.unity              # 唯一进包场景
│   ├── Scripts/
│   │   ├── Invariable/
│   │   │   ├── Workflow/               # Launcher、热更状态节点、RemoteServices
│   │   │   ├── Manager/                # UI / 音频 / 语言 / 网络 / SDK / 资源 / 裁剪
│   │   │   ├── Component/              # UIButton、LoopScrollList、Rocker、圆形图等
│   │   │   ├── Utils/                  # Singleton、状态机、配置读写、日志
│   │   │   └── ScriptableObject/       # BinAsset
│   │   └── HotUpdate/
│   │       ├── UI/                     # 业务面板（如 LoginPanel）
│   │       ├── Workflow/               # 热更完成后的开局逻辑
│   │       └── Utils/                  # 热更侧打开 UI、动态加组件
│   ├── GameAssets/                     # YooAsset 收集根目录
│   │   ├── Atlas/                      # Atlas00–03
│   │   ├── Prefabs/UI/                 # CommonPanel、Workflow
│   │   ├── Config/                     # Language / Player / RoleRune 等 .bin
│   │   ├── Audios/
│   │   ├── Materials/
│   │   ├── Animation/
│   │   ├── Png/
│   │   ├── Scenes/
│   │   ├── DLL/Android/                # HotUpdate 与 AOT 补充 DLL
│   │   └── LocalAssets/
│   ├── Resources/LocalAssets/          # 启动必需：UI_Root、SceneGameObject、HotUpdatePanel
│   ├── StreamingAssets/yoo/            # 内置资源清单
│   ├── Editor/MyTools/                 # 配置导出、图集、Bin 导入、打包脚本
│   ├── ToolPackage/                    # UniTask 2.5.10、DOTween、TextMesh Pro
│   └── Plugins/                        # ExcelDataReader、Android Gradle 模板
├── Packages/manifest.json
└── ProjectSettings/
```

## 启动流程

`Start` 场景挂 `Launcher`。`Awake` 里按平台选播放模式，并创建 `GameManager`、`AudioManager`。

```
InitializeYooAsset
        ↓
CheckCatalogUpdate
        ↓
CheckResourceUpdates
        ↓
HotUpdateOver
        ↓
Launcher_StartGame → 销毁热更界面与 Launcher
```

编辑器默认 `EditorSimulateMode`，Android 默认 `HostPlayMode`。进度和文案通过事件 `Launcher_ShowProgress` / `Launcher_ShowTips` 刷新 `HotUpdatePanel`。

启动时若不存在，会从 `Resources/LocalAssets` 实例化并 `DontDestroyOnLoad`：

- `UI_Root`：UI 相机与分层 Canvas
- `SceneGameObject`：场景主相机

## 资源与寻址

YooAsset 包 `MyPackage` 开启 Addressable。收集规则见 `Assets/AssetBundleCollectorSetting.asset`。

| 分组 | 收集路径 | 地址规则 |
| --- | --- | --- |
| Animation | `GameAssets/Animation` | 组名 + 文件名 |
| Atlas | `GameAssets/Atlas/Atlas00`–`03` | 组名 + 文件名，按 Collector 打包 |
| Audios | `GameAssets/Audios` | 组名 + 文件名 |
| Config | `GameAssets/Config` | 组名 + 文件名 |
| Materials | `GameAssets/Materials` | 组名 + 文件名 |
| Png | `GameAssets/Png` | 组名 + 文件名 |
| Prefabs | `GameAssets/Prefabs/UI` | 组名 + 文件名，按 Collector 打包 |
| Scenes | `GameAssets/Scenes` | 组名 + 文件名 |
| DLL | `GameAssets/DLL` | 文件夹 + 文件名 |
| LocalAssets | `GameAssets/LocalAssets` | 组名 + 文件名（带 LocalAssets 标签） |

运行时常用地址：

- UI：`Prefabs_{面板名}`
- 音频：`Audios_{文件名}`
- 配置：`Config_{表名}`
- 图集 / 材质：由 `Utils.SetImage` / `Utils.SetGray` 按路径拼 key

真机走 Host 模式时，`RemoteServices` 按本地环境配置拼下载地址。环境相关文件不进库。

## 配置表

菜单：**Config**（编辑器自定义菜单下）

| 菜单 | 作用 |
| --- | --- |
| 导出 Web 配置 | 把本地环境配置导出为运行时可用的二进制 |
| 导出 Excel 配置 | 读取根目录 `Excel/`，按 Sheet 导出到 `Assets/GameAssets/Config/{Sheet名}.bin` |

Excel 约定（从第 0 行起）：

1. 第 0 行忽略
2. 第 1 行：`1` 客户端、`2` 服务端、`3` 两端
3. 第 2 行：字段名；列名为 `Index` 的列作为主键
4. 第 3 行：类型
5. 数据行；首列为 `NO` 跳过，`END` 结束当前表

`.bin` 由编辑器导出，`BinImporter` 把它导入成 `BinAsset`，运行时用 `ConfigUtils.GetConfigData` 取值。

`Utils.GetetTextByKey` 按 `LanguageManager.LanguageKey`（`Chinese` / `English`）读 `Language` 表。未设置时：系统 `zh-CN` 用中文，其余默认中文。

## 运行时模块

| 类型 | 说明 |
| --- | --- |
| `GameManager` | 启动事件总线（`Launcher_*`） |
| `YooAssetManager` | 异步加载资源 |
| `UIManager` | 面板注册与关闭 |
| `AudioManager` | 按名加载并播放 AudioClip |
| `LanguageManager` | 语言 key，切换后可重启场景 |
| `SdkManager` | 第三方登录 |
| `MessageNetManager` | 网络收发、`BindReceiveMessage` / `Send` |
| `CullingGroupManager` | 实体进出视野时显隐 |
| `UIPanel` / `UIPopup` | 面板基类；弹窗用 DOTween 缩放 |
| `UIButton` | 单击 / 双击 / 长按 / 按下抬起，可转发给 ScrollRect |
| `LoopScrollList` | 横/纵向循环列表 |
| `Rocker` | 虚拟摇杆 |
| `CircleImage` / `CircleRawImage` | 圆形裁剪 |
| `DebugLogTool` | 真机错误日志落盘 |

热更侧打开界面：

```csharp
HotUpdateUtils.OpenUIPrefabPanel("Prefabs/UI/Workflow/LoginPanel", 0);
```

## 本地开发

1. 安装 **Unity 2022.3.51f1c1**（或同系列 2022.3 LTS），用 Hub 打开本仓库根目录。
2. 等 Package Manager 拉齐依赖（YooAsset、HybridCLR、Spine 等走 Git / OpenUPM）。
3. 打开 `Assets/Scenes/Start.unity`，进入 Play。编辑器走模拟模式，不访问远端资源。
4. 若要导出配置：把 Excel 放到仓库根目录 `Excel/`，执行 **Config → 导出 Excel 配置**。
5. 真机 Host 模式所需的环境配置放在本地，不要提交。

真机包大致步骤：

1. HybridCLR 生成并编译 `HotUpdate`，把 DLL（及需要的 AOT 补充 DLL）放到 `Assets/GameAssets/DLL/Android/`。
2. YooAsset 构建 `MyPackage`，内置部分进 `StreamingAssets/yoo`，更新部分上传到资源服务器。
3. 打 Android 包。签名文件只放本机，不要提交。

`HybridCLRData/`、`HybridCLRGenerate/`、`Build/`、`CDN/`、`Bundles/` 均为本地产物，不要提交。

## 第三方依赖

**Package Manager**

| 包 | 用途 |
| --- | --- |
| `com.code-philosophy.hybridclr` | 代码热更 |
| `com.tuyoogame.yooasset` 2.3.18 | 资源热更 |
| `com.esotericsoftware.spine.*` 4.2 | Spine 动画 |
| `com.coffee.ui-particle` | UGUI 粒子 |
| `com.unity.nuget.newtonsoft-json` | JSON |
| `com.google.external-dependency-manager` | Android 原生依赖 |

**内置在 `Assets/ToolPackage`**

- UniTask 2.5.10
- DOTween
- TextMesh Pro

**Plugins**

- `ExcelDataReader.dll`：编辑器导表
- `Plugins/Android/*Template.gradle`：Android 原生依赖模板
