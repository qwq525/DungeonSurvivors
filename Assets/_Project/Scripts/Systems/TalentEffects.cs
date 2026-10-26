using DungeonSurvivors.Core;
using DungeonSurvivors.Gameplay.Player;
using DungeonSurvivors.Gameplay.Weapons;
using UnityEngine;

namespace DungeonSurvivors.Systems
{
    /// <summary>
    /// 开局应用局外天赋：读取存档里的天赋等级，加到玩家/武器基础属性上。
    /// 面试可讲点：局外成长与局内数值的衔接点（Start 时统一应用，避免散落在各处）。
    /// </summary>
    public class TalentEffects : MonoBehaviour
    {
        private void Start()
        {
            MissileWeapon weapon = GetComponent<MissileWeapon>();
            PlayerHealth health = GetComponent<PlayerHealth>();
            PlayerController controller = GetComponent<PlayerController>();

            foreach (TalentDefinition talent in TalentCatalog.Talents)
            {
                int level = MetaProgression.GetTalentLevel(talent.Id);
                if (level <= 0) continue;

                switch (talent.Id)
                {
                    case "claws":
                        if (weapon != null) weapon.AddDamage(level);
                        break;
                    case "hide":
                        if (health != null) health.AddMaxHealth(level * 10, true);
                        break;
                    case "legs":
                        if (controller != null) controller.AddMoveSpeed(level * 0.03f);
                        break;
                    case "fangs":
                        if (weapon != null) weapon.AddAttackSpeed(level * 0.03f);
                        break;
                }
            }
        }
    }
}
