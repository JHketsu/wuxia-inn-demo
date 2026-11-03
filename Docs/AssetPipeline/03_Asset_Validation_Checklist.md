# Asset Validation Checklist

版本：0.1 · 草案，待批准

## 使用方式

流程：Tripo Generation → Silhouette Check → Style Check → Geometry Check → Material Check → Transform / Scale / Pivot Check → Game Readiness Check → Unity Validation Scene → 人工 Approved / Needs Review / Reject。

**AUTO CHECK** 仅检测并报告；**SAFE AUTO FIX** 是未来可批准的低风险修复候选，限定在工作副本上、保存前备份、记录改动、可撤销且修复后重检；**MANUAL REVIEW** 由人工判断。每行属于一种执行类别；相关检测和修复分别列出。当前不执行任何修复。

## AUTO CHECK

| ID | 检查 | 输出/处理规则 |
|---|---|---|
| A01 | Golden 路径/标记、文件版本与签核匹配 | 受保护资产仅只读；过期签核失效 |
| A02 | 三角面、顶点、对象数、材质槽数 | 原始及导出结果分别统计，与类别预算比较；未配置为 REVIEW |
| A03 | 非流形边、边界边、松散点/边/独立岛 | 定位并报告；不能推断是垃圾或错误 |
| A04 | 重合顶点候选、重叠几何候选 | 报告容差与位置；接缝、硬边、叠层可能有意 |
| A05 | 退化面、零面积面、无效数值、空网格 | 报告位置与影响；影响导出/渲染者阻断 |
| A06 | 法线数据异常、负尺度、镜像变换 | 报告；朝向是否合理仍需人工检查 |
| A07 | UV 存在、数量、名称、所需通道 | 对照材质要求；重叠/越界只报候选 |
| A08 | 材质槽为空、材质缺失、数量超预算 | 必需材质缺失阻断；未配置预算为 REVIEW |
| A09 | 图像依赖、文件路径、读取状态、打包状态 | 必需图像无法解析阻断；打包图像与外部文件分开记录 |
| A10 | 贴图分辨率、格式、颜色空间、alpha | 4K+ 警告；颜色空间按已配置用途比较 |
| A11 | 相同材质/图像候选 | 比较数据指纹并报告；同名不等于相同，不自动合并 |
| A12 | Scale、Rotation、包围盒尺寸 | 对照资产类型；非单位变换并非所有角色都必须修复 |
| A13 | Origin 与包围盒/声明锚点的关系 | 仅测距离与坐标，不能判断艺术轴心正确性 |
| A14 | 对象/Mesh/材质/贴图命名、重复 asset_id | 对照命名规则；外部依赖未确认时不改名 |
| A15 | 导出集合、依赖完整性、目标路径与配置 | 排除意外对象候选；正式导出前必须人工确认集合 |
| A16 | Collider/LOD 声明与代理/层级存在性 | 检查适用性与配置；Unity 生效状态由 Unity 检查 |
| A17 | 角色骨架、权重异常、动作/挂点存在性 | 按角色配置检查；不推断表演质量 |
| A18 | Unity 导入日志、依赖、材质/Collider/LOD 配置 | 后续 Unity 工具负责；必需配置失败阻断 |
| A19 | 必需人工审查与 Unity 证据是否齐全 | 任何缺项均至少 REVIEW |

## SAFE AUTO FIX（待批准后启用）

| ID | 可考虑的操作 | 安全前提与回退 |
|---|---|---|
| F01 | 创建缺失的工作/报告/导出目录 | 不覆盖同名文件，不触碰 Golden；记录新目录 |
| F02 | 为尚未发布且无外部引用的资产补齐 manifest 默认字段 | 不覆盖已有值；备份旧 manifest；未知项写 TBD |
| F03 | 将可确认的项目内部贴图路径转为相对路径 | 文件唯一解析，内容一致；仅改路径，不移动/重编码贴图；改后重开验证 |

无法证明前提时转为 MANUAL REVIEW。应用变换、焊接顶点、重算法线、清空材质槽、改名、打包/转换贴图均不列为默认安全修复。

## MANUAL REVIEW

| ID | 必需人工判断 | 建议证据 |
|---|---|---|
| M01 | 灰模轮廓、类别与物体识别 | 灰模多视图及游戏镜头截图 |
| M02 | 风格、比例、表面噪声、材质分区 | 中性照明图、Unity 场景图、Golden 对照 |
| M03 | 文化结构、纹样与重要装饰是否保留 | 保留/简化理由；建筑屋顶/斗拱专项确认 |
| M04 | 减面、重拓扑、删除碎片/内部结构、焊接/合并 | 变更前后轮廓与拓扑对比 |
| M05 | 法线朝向、重算法线/硬边、UV 重叠意图 | 着色与 UV 检视 |
| M06 | 面部质量、关节变形、发袖动态、动画感受 | 表情与代表动作视频，极限姿势 |
| M07 | 武器类别、握持、挂点、攻击与武器互动 | 静态握持和攻击动作 |
| M08 | 尺寸、方向、轴心及应用变换 | Unity 尺寸参照、落地/旋转/握持测试；角色绑定影响确认 |
| M09 | 材质合并、槽删除、贴图降采样/烘焙/替换 | 目标镜头前后对比及依赖确认 |
| M10 | 重命名、导出集合与正式导出配置 | 引用影响、导出预览、Unity 导入结果 |
| M11 | Collider 形状、LOD 必要性与切换质量 | 导航/碰撞测试、切换录像 |
| M12 | 环境构图、导航、交互物可读性和视觉层级 | 山林雪地实机场景与游戏镜头 |
| M13 | Golden 提名/替换、最终 Accept/Reject | 批准人、日期、版本、适用范围 |

所有待签核行明确输出 **MANUAL REVIEW REQUIRED**。不适用项需人工确认理由，不能以空白代替通过。

## 单资产记录模板

```text
asset_id / category / version:
source / source_file_hash / working_file_hash / export_file_hash:
validator_version / profile_version / checked_at:
Unity_version / render_pipeline / target_platform:
technical_pass: true | false | pending
status: READY | REVIEW | NOT_READY
decision: Approved | Needs Review | Reject
check_id / class / applicability / severity / measured / expected:
evidence_path / issue / proposed_action:
fix_log / backup_path / recheck_result:
manual_check_id / reviewer / reviewed_at / reviewed_file_hash / result / reason:
```

严重度为 BLOCKER / WARNING / INFO；没有配置或证据不能记录 PASS。报告保留测量口径、容差与检查局限，便于复现。
