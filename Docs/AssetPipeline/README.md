# ARPG 美术资产 Pipeline

版本：0.1 · 2026-10-07 · **草案，待项目负责人审阅批准**

目标：把不一致的 AI 生成资产转化为风格连贯、形态可辨识、性能可控的游戏资产，同时保留有意设计的美术与文化细节。

工作流：Tripo → Blender → Unity。

## 文档入口

- [Asset Style Bible](01_Asset_Style_Bible.md)：视觉原则与人工判断边界。
- [Game-Ready Asset Standard](02_Game_Ready_Asset_Standard.md)：技术交付与通过规则。
- [Asset Validation Checklist](03_Asset_Validation_Checklist.md)：检查分类、证据与处理方式。
- [Folder Structure & Automation Roadmap](04_Folder_Structure_and_Automation.md)：目录、命名、追踪与后续自动化范围。

## 当前审阅事项

1. 确定 Unity 版本、渲染管线、目标平台与性能目标。
2. 确定各资产类别的尺寸范围、三角面/材质/贴图/LOD 预算，以及镜头距离。
3. 确定角色骨架、动画、武器挂点、碰撞与导出配置。
4. 用首批 Golden 候选验证规则；建筑风格继续保留 TBD。

上述事项未确定前，相应检查报告为 REVIEW，不臆造统一硬阈值。本文档批准不等于授权批量修改资产；修复策略与具体资产变更仍应满足清单中的人工审查要求。

当前交付仅包含规范。未创建完整 Blender addon，未执行模型清理、减面、合并、材质替换或导出。
