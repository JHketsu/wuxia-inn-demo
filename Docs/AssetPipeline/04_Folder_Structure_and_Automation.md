# Folder Structure & Automation Roadmap

版本：0.1 · 草案，待批准

## 建议目录（尚未批量创建）

```text
项目根目录/
  Docs/AssetPipeline/                 # 本规范
  Art/
    GoldenReferences/<asset_id>/v001/ # 受保护：快照、截图、批准记录
    RawTripo/<asset_id>/v001/         # 原始下载、原贴图、生成信息；不覆盖
    BlenderWorking/<asset_id>/        # .blend 工作版本；引用统一 Textures
    Textures/<asset_id>/v001/          # 可编辑派生贴图；保留原始文件
    ApprovedAssets/<asset_id>/v001/   # 已批准 .blend、依赖副本、manifest 与签核
    UnityExports/<asset_id>/v001/     # Unity-ready 模型、贴图、导出配置
    Validation/<asset_id>/v001/       # JSON/Markdown 报告、截图、录像、日志
  Pipeline/
    Profiles/                         # 类别预算、单位、导出/导入规则，后续建立
    Blender/                          # 未来只读检查脚本
    Unity/                            # 未来导入与验证工具
  UnityProject/Assets/GameArt/
    Models/<category>/
    Textures/<category>/
    Materials/<category>/
    Prefabs/<category>/
  UnityProject/Assets/Validation/      # Validation Scene 和测试参考
```

Unity 项目名称和位置待确认。UnityExports 是交付暂存，正式导入后由 Unity 管理 `.meta`；不得用同步脚本覆盖或重建已有 `.meta`。ApprovedAssets 与 Golden 保留当时依赖快照，避免共享贴图后续变化污染批准版本。

## 命名与追踪

建议 ASCII、snake_case、稳定 ID；中文展示名保存在 manifest。示例：

- ID：`prop_incense_burner_001`；类别前缀建议 `chr / enemy / npc / wpn / prop / veg / terrain / arch`。
- Blender：`prop_incense_burner_001_v003.blend`。
- 对象：`prop_incense_burner_001_body`；Mesh：`mesh_prop_incense_burner_001_body`。
- 材质：`mat_prop_incense_burner_001_bronze`。
- 贴图：`tex_prop_incense_burner_001_bronze_basecolor_v003.png`；用途后缀 `normal / roughness / metallic / ao`。
- LOD：`prop_incense_burner_001_body_lod0`；碰撞代理：`col_prop_incense_burner_001_body`。这些是项目命名，不假定 Unity 自动识别。
- 报告：`validation.json`、`validation.md`；元数据：`manifest.json`。

manifest 记录来源/生成信息（可获得时）、类别、显示名、版本、父版本、文件及哈希、尺寸/轴心用途、预算配置、依赖、导出配置、检查与签核。文件改动后重新验证，禁止覆盖原始下载和批准快照。

## 后续 Blender Python 自动化

| 阶段 | 范围 | 边界/完成条件 |
|---|---|---|
| 0：规则校准 | 对 Golden 候选人工走完清单，确认预算与例外 | 先批准规范、检查口径与类别 profile |
| 1：只读报告 | A01–A14、A19：几何/UV/材质/图像/变换/命名/签核检查 | 单脚本起步，输出 JSON + Markdown；不改场景，不保存 .blend |
| 2：交付预检 | A15–A17：导出集合、Collider/LOD 声明、骨架/权重/动作检查 | 使用明确 profile；记录原网格与 modifier 后网格的统计差异 |
| 3：低风险辅助 | F01–F03 | 单独启用、工作副本、备份、改动日志、重检；Golden 硬保护 |
| 4：Unity 验证 | A18、场景证据与报告整合 | Blender 无法证明 Unity 导入正确；Unity 侧验证 `.meta`、材质、碰撞、LOD、动画 |
| 5：按需工具化 | 规则稳定后再考虑 ARPG Asset Validator addon | 根据实际重复任务决定 UI 和批处理，不提前建设完整框架 |

Blender 可安全自动化的是**检测与测量**，不是艺术判断。重复顶点/重叠几何检测具有容差和候选性质；法线、UV、轴心、拓扑与材质检查必须声明局限。Unity 性能、导入和运行时行为需要独立证据。

暂不实施自动减面、重拓扑、合并/焊接、删除几何、重算法线、应用角色变换、材质合并、贴图降采样、文化装饰删减或 Golden 更新。未来这些操作只能作为人工审查后的具体变更，在副本上执行并重新签核。
