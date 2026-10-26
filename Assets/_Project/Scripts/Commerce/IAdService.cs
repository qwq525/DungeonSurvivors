using System;

namespace DungeonSurvivors.Commerce
{
    /// <summary>
    /// 广告服务接口（小游戏商业化接口抽象）：真接穿山甲/优量汇时替换实现即可。
    /// 面试可讲点：接口抽象的意义（Mock 与真 SDK 解耦）、激励视频的成功/失败/关闭回调、频控。
    /// </summary>
    public interface IAdService
    {
        void ShowRewardedAd(string placement, Action<bool> onCompleted);
    }
}
