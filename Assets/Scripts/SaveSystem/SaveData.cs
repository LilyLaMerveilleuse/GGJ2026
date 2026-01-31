using System;

namespace SaveSystem
{
    /// <summary>
    /// Structure de données pour une sauvegarde.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int slotIndex;
        public string createdAt;        // DateTime ISO
        public string lastPlayedAt;     // DateTime ISO
        public float totalPlayTime;     // Secondes
        public string currentScene;     // Nom de la scène
        public float posX, posY, posZ;  // Position du joueur

        public SaveData()
        {
            slotIndex = -1;
            createdAt = DateTime.Now.ToString("o");
            lastPlayedAt = createdAt;
            totalPlayTime = 0f;
            currentScene = "Village";
            posX = posY = posZ = 0f;
        }

        public SaveData(int slot) : this()
        {
            slotIndex = slot;
        }

        public DateTime GetCreatedDate()
        {
            return DateTime.TryParse(createdAt, out var date) ? date : DateTime.MinValue;
        }

        public DateTime GetLastPlayedDate()
        {
            return DateTime.TryParse(lastPlayedAt, out var date) ? date : DateTime.MinValue;
        }

        public string GetFormattedPlayTime()
        {
            var span = TimeSpan.FromSeconds(totalPlayTime);
            if (span.TotalHours >= 1)
            {
                return $"{(int)span.TotalHours}h {span.Minutes:D2}m";
            }
            return $"{span.Minutes}m {span.Seconds:D2}s";
        }
    }
}
