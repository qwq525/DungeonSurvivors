namespace DungeonSurvivors.Gameplay.Player
{
    /// <summary>
    /// 输入抽象：PC 键盘 / 手机虚拟摇杆 / 小游戏触屏都用同一接口接进玩家控制。
    /// 面试可讲点：为什么要抽象输入（同一套玩法逻辑适配 PC/移动端），以及接口与实现分离。
    /// </summary>
    public interface IInputSource
    {
        UnityEngine.Vector2 Move { get; }
        bool DashPressed { get; }
    }
}
