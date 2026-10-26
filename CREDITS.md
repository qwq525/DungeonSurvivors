# Credits / 素材与工具说明

> 正式发布前请把占位的素材来源替换为实际使用项，并保留对应 License 文件。

## 代码

- 游戏代码：本人编写（AI 辅助开发，核心模块逐行评审）。
- 第三方库/包：Unity 2022.3、TextMeshPro、Unity UI、Unity Test Framework（Unity 官方，遵循 Unity 许可）。

## 美术（当前本地美术版使用）

- 占位精灵：由项目内编辑器脚本程序化生成（`Day0SceneSetup.CreateSprite`），公开仓库使用此版本。
- 本地美术版（不随公开仓库分发，授权文件保留在 `Assets/ThirdParty/`）：
  - **Craftpix** – Free Slime Mobs Pixel Art Top Down Sprite Pack（主角史莱姆，4 方向动画）
  - **Craftpix** – Free Base 4 Direction Male Character Pixel Art（勇者敌人，4 方向动画）
  - **Craftpix** – Top Down Dungeon Pixel Tileset（地牢素材，待接入）
  - **Craftpix** – Free Basic Pixel Art UI for RPG、Magic Sprite Effects、Explosion Sprites（待接入）
  - 遵循 Craftpix File Licenses：仅用于游戏成品，不 redistribute 源文件
- 计划接入（已下载）：
  - Kenney（CC0）：tiny dungeon、impact sounds、rpg audio
  - Craftpix Warrior 4 Direction（Boss 候选）
- **当前已实际接入（本地美术版，不随公开仓库分发）**：
  - Craftpix Slime / Base Male 4-direction（角色 + 四方向动画）
  - Kenney tiny-dungeon（地板 `tile_0049`、砖墙 `tile_0059`，CC0）
  - Kenney impact-sounds / rpg-audio（打击、死亡、受伤、升级、拾取、射击音效，CC0）
- 字体：得意黑 Smiley Sans（SIL OFL 1.1）、MiSans（小米免费商用）、Noto Sans CJK（SIL OFL 1.1，当前 UI 使用）

## 音频（计划）

- Kenney RPG Audio / Impact Sounds（CC0）
- 其他免费音效/BGM：逐个登记作者与 License

## AI 工具

- 使用 AI Agent 辅助：脚手架、编辑器工具、压测脚本、文档初稿；核心玩法、架构、验收与调优由本人完成。
- 详细工作流见项目文档与技术文章。

## 免责声明

本仓库为个人求职作品集项目，不含任何商业游戏素材；第三方素材授权以各平台 License 原文为准。
