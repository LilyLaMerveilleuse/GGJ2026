using System.Collections.Generic;
using Masks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystem
{
    /// <summary>
    /// Singleton persistant contenant l'état du jeu en mémoire.
    /// </summary>
    public class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }

        /// <summary>
        /// Données de la partie en cours (null si pas de partie chargée).
        /// </summary>
        public SaveData CurrentSave { get; private set; }

        /// <summary>
        /// Indique si une position doit être chargée depuis la sauvegarde.
        /// </summary>
        public bool ShouldLoadPositionFromSave { get; set; }

        private float sessionStartTime;
        private List<GameObject> persistentGameplayObjects = new List<GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            bool isMainMenu = scene.name == "Main Menu";

            // Gérer les objets persistants selon la scène
            SetPersistentObjectsActive(!isMainMenu);

            // Met à jour la scène courante dans la sauvegarde
            if (CurrentSave != null && !isMainMenu)
            {
                CurrentSave.currentScene = scene.name;
            }
        }

        /// <summary>
        /// Enregistre un objet persistant de gameplay à gérer.
        /// </summary>
        public void RegisterPersistentObject(GameObject obj)
        {
            if (obj != null && !persistentGameplayObjects.Contains(obj))
            {
                persistentGameplayObjects.Add(obj);
            }
        }

        /// <summary>
        /// Active ou désactive les objets de gameplay persistants.
        /// </summary>
        private void SetPersistentObjectsActive(bool active)
        {
            // Nettoyer les références nulles (objets détruits)
            persistentGameplayObjects.RemoveAll(obj => obj == null);

            // Collecter les objets persistants
            CollectPersistentObjects();

            // Activer/désactiver tous les objets enregistrés
            foreach (var obj in persistentGameplayObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(active);
                }
            }
        }

        private void CollectPersistentObjects()
        {
            // Chercher le joueur
            AddObjectByTag("Player");

            // Chercher la caméra principale
            AddObjectByTag("MainCamera");

            // Chercher les objets "Persist"
            var persistObjects = GameObject.FindGameObjectsWithTag("Persist");
            foreach (var obj in persistObjects)
            {
                if (obj.GetComponent<GameState>() == null &&
                    obj.GetComponent<SaveManager>() == null &&
                    !persistentGameplayObjects.Contains(obj))
                {
                    persistentGameplayObjects.Add(obj);
                }
            }
        }

        private void AddObjectByTag(string tag)
        {
            var obj = GameObject.FindWithTag(tag);
            if (obj != null && !persistentGameplayObjects.Contains(obj))
            {
                persistentGameplayObjects.Add(obj);
            }
        }

        /// <summary>
        /// Démarre une nouvelle partie.
        /// </summary>
        public void StartNewGame(int slotIndex)
        {
            // Réactiver les objets persistants avant de charger la scène
            SetPersistentObjectsActive(true);

            CurrentSave = SaveManager.Instance.CreateNewSave(slotIndex, "Village");
            ShouldLoadPositionFromSave = false;
            sessionStartTime = Time.realtimeSinceStartup;

            // Réinitialiser l'inventaire de masques
            EnsureMaskInventoryExists();
            MaskInventory.Instance.Clear();

            SceneManager.LoadScene("Village");
        }

        /// <summary>
        /// Charge une partie existante.
        /// </summary>
        public void LoadGame(int slotIndex)
        {
            var save = SaveManager.Instance.GetSave(slotIndex);
            if (save == null)
            {
                Debug.LogError($"Aucune sauvegarde trouvée au slot {slotIndex}");
                return;
            }

            // Réactiver les objets persistants avant de charger la scène
            SetPersistentObjectsActive(true);

            CurrentSave = save;
            ShouldLoadPositionFromSave = true;
            sessionStartTime = Time.realtimeSinceStartup;

            // Charger les masques depuis la sauvegarde
            EnsureMaskInventoryExists();
            MaskInventory.Instance.LoadFromSaveData(save);

            SceneManager.LoadScene(save.currentScene);
        }

        /// <summary>
        /// S'assure que MaskInventory existe.
        /// </summary>
        private void EnsureMaskInventoryExists()
        {
            if (MaskInventory.Instance == null)
            {
                var maskInvObj = new GameObject("MaskInventory");
                maskInvObj.AddComponent<MaskInventory>();
            }
        }

        /// <summary>
        /// Sauvegarde l'état actuel.
        /// </summary>
        public void SaveCurrentGame()
        {
            if (CurrentSave == null)
            {
                Debug.LogWarning("Aucune partie en cours à sauvegarder");
                return;
            }

            // Met à jour le temps de jeu
            var sessionTime = Time.realtimeSinceStartup - sessionStartTime;
            CurrentSave.totalPlayTime += sessionTime;
            sessionStartTime = Time.realtimeSinceStartup;

            // Synchroniser les masques
            if (MaskInventory.Instance != null)
            {
                MaskInventory.Instance.SyncToSaveData();
            }

            // Sauvegarde sur disque
            SaveManager.Instance.Save(CurrentSave);
        }

        /// <summary>
        /// Met à jour la position du joueur dans la sauvegarde.
        /// </summary>
        public void UpdatePlayerPosition(Vector3 position)
        {
            if (CurrentSave == null) return;

            CurrentSave.posX = position.x;
            CurrentSave.posY = position.y;
            CurrentSave.posZ = 0f; // Toujours 0 pour un jeu 2D
        }

        /// <summary>
        /// Retourne la position sauvegardée.
        /// </summary>
        public Vector3 GetSavedPosition()
        {
            if (CurrentSave == null)
            {
                return Vector3.zero;
            }
            // Z toujours à 0 pour un jeu 2D
            return new Vector3(CurrentSave.posX, CurrentSave.posY, 0f);
        }

        /// <summary>
        /// Retourne au menu principal.
        /// </summary>
        public void ReturnToMainMenu()
        {
            // Sauvegarde avant de quitter
            SaveCurrentGame();
            CurrentSave = null;
            ShouldLoadPositionFromSave = false;

            SceneManager.LoadScene("Main Menu");
        }
    }
}
