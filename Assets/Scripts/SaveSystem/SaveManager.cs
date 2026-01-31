using System;
using System.IO;
using Constants;
using UnityEngine;

namespace SaveSystem
{
    /// <summary>
    /// Singleton gérant la lecture/écriture des fichiers de sauvegarde.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        public const int MaxSlots = 3;
        private const string SaveFolder = "saves";
        private const string SaveFilePrefix = "save_slot_";
        private const string SaveFileExtension = ".json";

        private SaveData[] cachedSaves = new SaveData[MaxSlots];

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureSaveDirectoryExists();
            LoadAllSaves();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private string GetSaveFolderPath()
        {
            return Path.Combine(Application.persistentDataPath, SaveFolder);
        }

        private string GetSaveFilePath(int slotIndex)
        {
            return Path.Combine(GetSaveFolderPath(), $"{SaveFilePrefix}{slotIndex}{SaveFileExtension}");
        }

        private void EnsureSaveDirectoryExists()
        {
            var path = GetSaveFolderPath();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Charge toutes les sauvegardes depuis le disque.
        /// </summary>
        public void LoadAllSaves()
        {
            for (int i = 0; i < MaxSlots; i++)
            {
                cachedSaves[i] = LoadSaveFromDisk(i);
            }
        }

        private SaveData LoadSaveFromDisk(int slotIndex)
        {
            var path = GetSaveFilePath(slotIndex);
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                var json = File.ReadAllText(path);
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Erreur lors du chargement de la sauvegarde {slotIndex}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Retourne les données de sauvegarde d'un slot (null si vide).
        /// </summary>
        public SaveData GetSave(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSlots)
            {
                return null;
            }
            return cachedSaves[slotIndex];
        }

        /// <summary>
        /// Vérifie si un slot contient une sauvegarde.
        /// </summary>
        public bool HasSave(int slotIndex)
        {
            return GetSave(slotIndex) != null;
        }

        /// <summary>
        /// Sauvegarde les données sur le disque.
        /// </summary>
        public bool Save(SaveData data)
        {
            if (data == null || data.slotIndex < 0 || data.slotIndex >= MaxSlots)
            {
                return false;
            }

            try
            {
                data.lastPlayedAt = DateTime.Now.ToString("o");
                var json = JsonUtility.ToJson(data, true);
                File.WriteAllText(GetSaveFilePath(data.slotIndex), json);
                cachedSaves[data.slotIndex] = data;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Erreur lors de la sauvegarde du slot {data.slotIndex}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Crée une nouvelle sauvegarde dans un slot.
        /// </summary>
        public SaveData CreateNewSave(int slotIndex, string initialScene = null)
        {
            var save = new SaveData(slotIndex)
            {
                currentScene = initialScene ?? GameConstants.Scenes.Village
            };

            if (Save(save))
            {
                return save;
            }
            return null;
        }

        /// <summary>
        /// Supprime une sauvegarde.
        /// </summary>
        public bool DeleteSave(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSlots)
            {
                return false;
            }

            var path = GetSaveFilePath(slotIndex);
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                cachedSaves[slotIndex] = null;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Erreur lors de la suppression du slot {slotIndex}: {e.Message}");
                return false;
            }
        }
    }
}
