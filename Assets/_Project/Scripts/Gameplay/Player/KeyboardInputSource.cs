using UnityEngine;

namespace DungeonSurvivors.Gameplay.Player
{
    /// <summary>PC 键鼠输入：WASD / 方向键移动，空格闪避。后续可新增 TouchInputSource 替代。</summary>
    public class KeyboardInputSource : IInputSource
    {
        public Vector2 Move
        {
            get { return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); }
        }

        public bool DashPressed
        {
            get { return Input.GetKeyDown(KeyCode.Space); }
        }
    }
}
