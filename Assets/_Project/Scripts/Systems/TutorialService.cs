using DungeonSurvivors.Core;

namespace DungeonSurvivors.Systems
{
    /// <summary>新手引导状态（存档里只记一个 bool）。</summary>
    public static class TutorialService
    {
        public static bool ShouldShow
        {
            get { return !SaveSystem.Current.meta.tutorialSeen; }
        }

        public static void MarkSeen()
        {
            if (SaveSystem.Current.meta.tutorialSeen) return;
            SaveSystem.Current.meta.tutorialSeen = true;
            SaveSystem.Save();
        }
    }
}
