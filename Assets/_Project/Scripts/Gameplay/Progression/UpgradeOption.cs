namespace DungeonSurvivors.Gameplay.Progression
{
    public enum UpgradeType
    {
        Damage,
        AttackSpeed,
        MoveSpeed,
        MaxHealth,
        OrbitBlades
    }

    /// <summary>一个升级/变异选项（MVP 用运行时数据；后续接 ScriptableObject 配置表）。</summary>
    public readonly struct UpgradeOption
    {
        public readonly string Title;
        public readonly string Description;
        public readonly UpgradeType Type;
        public readonly float Value;

        public UpgradeOption(string title, string description, UpgradeType type, float value)
        {
            Title = title;
            Description = description;
            Type = type;
            Value = value;
        }
    }
}
