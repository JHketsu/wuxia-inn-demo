# 客栈建筑：拆块与生成 Prompt v1

2026-11-03。状态：图纸与提示词准备；不提交Aholo任务。家具仅框线占位，另案生成。用户认可的外观是基准，新增背面、屋顶细节与窗位仍属提案。

## 决策：先拆块，再生成

按现有Unity尺寸先建立模块接口：墙体、柱梁、楼板、楼梯采用尺寸驱动构件；Aholo生成独特的门窗、檐口、招牌、灯罩等视觉件。整栋生成可以做外观缩略预览，但不能承担可进入建筑的正式几何。整栋后拆容易留下封死室内、错误墙厚、不可重复接缝和混合材质；自动部件拆分不是建筑拓扑修复。

窗框与窗扇独立，窗洞是墙体规格；窗格必须有真实空隙，不用不透明贴图假装透光。整体风格图不包含太阳烘焙阴影。屋顶可按坡面、屋脊、直檐、端部拆；不逐片生成瓦。可见内侧与外侧均要有表面，屋顶底面固定8.50m；屋脊、外檐另定。

## 图纸包

纯净外观：前右/后左三分之四视角、南东北西立面。空间研究：两层平面、屋顶俯视、纵横剖面及太阳方向图。AI图仅作视觉参考，平面与剖面必须对照Unity尺寸复核后才可作为建模输入；家具仅框线。

仍需在窗位和结构确认后制作：各模块单独白底主视图、背视图、接口尺寸图；大堂/走廊/客房的灰盒同机位对照。不能直接把整张多资产制图板送入单物件生成。

## Prompt 固定顺序

主体与数量 → 形状与连接接口 → 材质 → 颜色 → 风格 → 单位与尺寸 → 几何/贴图预算 → 参考视角及光照 → 排除项。

英文模板：

Single isolated [SUBJECT], [SHAPE AND JOINERY]. Materials: [MATERIALS]. Colors: [PALETTE]. Refined stylized game asset, fine voxel-inspired block forms, restrained pixel-scale surface detail, readable three-dimensional volume, Chinese wuxia roadside inn. Target dimensions in meters: [W x H x D], Y-up, bottom-center pivot. Geometry budget: [TARGET] triangles in the FINAL optimized asset, clean silhouette, no hidden duplicate geometry. PBR textures, consistent texel density, [TEXTURE SIZE], maximum [N] material slots. [VIEW], pure white background, neutral diffuse studio illumination, complete object in frame, minimal contact shadow. No text, dimensions, watermark, terrain, plants, people, extra objects, baked directional sunlight, glowing windows, palace ornaments, Japanese motifs, exaggerated cartoon proportions.

图像Prompt中写面数不会控制图像，也不保证网格输出。API实际支持的模型版本、faceCount范围与输出拓扑优先于文本；此前查到生成faceCount最低10000，不得直接承诺输出低于该限制。最终三角面数导入后实测，必要时用已核实可用的重拓扑能力；不用Blender。尺寸/轴向/枢轴同样需要导入后纠正，文字不是约束求解器。

## 模块提示词草案

以下拼接模板公共尾段；方括号参数未定稿前不提交生成。

### WIN-A 木窗视觉组件

Single isolated Chinese timber lattice window assembly, rectangular outer frame with restrained square lattice and two separately defined opening leaves, consistent straight edges and visible rear frame, actual open gaps between lattice bars, no wall attached. Materials: weathered matte wood, small dark iron hinges. Colors: muted dark walnut brown, subtle warm grey wear. Target dimensions [W] x [H] x [D] meters; frame outer bounds and wall opening fit must be specified separately. Final triangle target 4000, 1K textures, maximum 2 material slots. Front-right three-quarter view showing depth and rear joint edges.

窗宽高、窗台高、墙厚、框口尺寸待平面/立面确认，不能自动用门洞尺寸替代。

### EAVE-A 直檐模块

Single isolated straight eave segment for a modest two-storey Chinese roadside inn, repeatable straight end interfaces, shallow grey clay tile overhang supported by simple brown timber rafters, no ornate brackets, no curled corner, visible underside. Materials: matte weathered clay tile and old timber. Colors: charcoal grey tiles, dark warm brown timber. Target dimensions [L] x [H] x [D] meters, final triangle target 6000, 1K textures, maximum 2 material slots. Neutral three-quarter view with both end interfaces visible.

瓦纹重复节距、端部接口、屋面坡度及挑出长度必须统一。转角与端头另立资产，不混在直檐任务。

### SIGN-A 布幌与支架

Single isolated modest Chinese roadside inn hanging sign, simple horizontal timber bracket and one vertical rectangular faded cloth banner, gentle static cloth folds, no lettering or symbols, no building attached. Materials: coarse woven cloth, old timber, dark iron fasteners. Colors: faded ochre beige, dark brown. Target dimensions [W] x [H] x [D] meters, final triangle target 3000, 1K textures, maximum 3 material slots. Front-right three-quarter view.

文字后期单独制作，尺寸及悬挂位置待确认。布与支架需分件时先检查源输出，不默认付费拆分。

### LAMP-A 灯罩

Single isolated hanging lantern for a modest Chinese timber inn, simple rectangular timber frame with warm off-white translucent paper panels and thin hanging rod, understated practical construction, no tassels or ceremonial ornaments, lantern unlit. Materials: matte dark wood, paper, dark iron. Colors: muted walnut and ivory. Target dimensions [W] x [H] x [D] meters, final triangle target 2500, 1K textures, maximum 3 material slots. Front-right three-quarter view, neutral studio lighting.

透光材质与Unity灯源分开设置，不将亮灯效果烘焙进底色。

## 结构构件（不必消耗生成积分）

墙体：实际墙厚、洞口、表面厚度与碰撞代理对齐。柱梁：截面、长度、相邻插接面按尺寸表。楼板：顶面4.35m，厚0.30m。楼梯：24级、宽2.40m，保留原通行几何。材质统一复用，规则构件可在Unity侧建立可编辑模型，不要求Blender。

## 太阳与参考图分离

模型生成使用中性棚光。实景晨光：+X东/+Z北/+Y上，正门朝南；太阳方位110°（北起顺时针）、高度20°，光传播向量约(-0.883,-0.342,0.321)。阴影向西北偏西。太阳是方向要求，不规定离模型多少米。先固定曝光，检验窗洞真实透光、楼板遮挡与深处补光。避免将建筑图中错误的“8.5m檐下高”沿用：该数值是工程屋顶底面。

## 每件接入记录格式

ID / 版本 / 参考图 / Prompt / 服务模型与参数 / 报价及实际扣分 / 任务ID / 源文件 / XYZ实测尺寸 / 三角面 / 材质槽与贴图 / 枢轴与缩放 / Prefab路径 / 接口检查 / 近景与背面截图 / 当前问题。

当前阶段未确定生成批次，不以图纸任务替代用户对资产范围和接口的讨论。

## 工具分工与图纸QA补充

建议混合流程，并不强制两种工具都使用：Unity尺寸驱动模块负责规则几何与装配，Aholo负责独特造型；当生成网格的接口、拓扑、UV或内外表面不能直接修正时再接Blender MCP。Blender是精修与可控建模工具，不是Aholo生成的必经步骤。复杂屋顶若无法在Unity侧稳定制作，也可以优先在Blender按尺寸建模，避免反复生图消耗。

已生成纯净外观参考v1与平面/剖面/晨光研究v1。后者QA未通过：屋顶俯视画成四坡而外观为双坡，剖面将直梯画成折返梯，纵横屋面方向未保持一致，8.50m与4.35m标注引线存在误指。只保留为概念研究，不用于正式建模。纯净外观各立面窗数、比例与背面也需对齐；图中白底视角名称不应代替真实相机方向校验。准确施工参考仍需从Unity尺寸构建后导出，不能宣称图纸包已通过工程验收。

晨光直接在Unity实现，与模型生成分离。当前InnDemo.Start会禁用既有灯光并创建Warm daylight，FinishInterior还会覆盖环境光，所以仅在场景Inspector添加太阳会在运行时失效。实施需调整现有运行时光源创建与环境光配置，而非叠加第二盏太阳。当前墙面没有实际窗洞，先加太阳仅能验收方向与外部阴影，不能实现最终室内窗格采光；需待窗洞和光照遮挡几何接入后验证。2026-11-03本轮只核查，未修改灯光脚本。
