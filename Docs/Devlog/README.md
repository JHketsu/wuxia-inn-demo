# 每日留档

每天复制 TEMPLATE.md 为上海日期日志，填写新增、修复、验证、已知问题、下一步；选 3–4 张实际截图放入 Screenshots/YYYY-MM-DD 并相对链接。里程碑才更新根目录 CHANGELOG。

提交前检查差异、凭据、本机路径、第三方资产许可及大文件，再明确选择游戏源码、Assets/.meta、Packages、ProjectSettings 和 Docs 提交；不要 git add 整个工作区。推送后核对远端提交与截图链接。

自动截图全集、Library、UserSettings、构建及生成服务凭据不上传。现阶段自动整理尚未启用。

## 当前仓库与同步
远端：https://github.com/JHketsu/wuxia-inn-demo
本地 Git 仓库：工作区/GitHubArchive；开发工程仍在原位置。
归档副本去除本机 Tripo 可选编辑器依赖，不改变开发工程；未在干净电脑重新导入测试。
每日在工作区更新日志与精选截图后运行 `python3 Tools/Archive/prepare.py`，再进入 GitHubArchive 检查 `git diff`、`git status`，明确暂存并提交、`git push origin main`。同步工具只覆盖新增/更新文件，源文件删除需在归档仓库核对后同步删除。
玩法操作以工程 README 为准，现状与验证细节以对应 Docs 文档为准；CHANGELOG 仅记录里程碑，不复制完整操作说明。
