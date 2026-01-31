using System;
using System.Collections.Generic;
using SaveSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Masks
{
    /// <summary>
    /// Singleton gérant la collection de masques du joueur.
    /// </summary>
    public class MaskInventory : MonoBehaviour
    {
        public static MaskInventory Instance { get; private set; }

        [Header("Events")]
        public UnityEvent<MaskType> onMaskObtained;

        private HashSet<MaskType> ownedMasks = new HashSet<MaskType>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Vérifie si le joueur possède un masque.
        /// </summary>
        public bool HasMask(MaskType mask)
        {
            return ownedMasks.Contains(mask);
        }

        /// <summary>
        /// Donne un masque au joueur.
        /// </summary>
        public void ObtainMask(MaskType mask)
        {
            if (mask == MaskType.None) return;

            if (ownedMasks.Add(mask))
            {
                Debug.Log($"[MaskInventory] Masque obtenu: {mask}");
                onMaskObtained?.Invoke(mask);

                // Mettre à jour la sauvegarde
                SyncToSaveData();
            }
        }

        /// <summary>
        /// Retire un masque au joueur (pour debug/tests).
        /// </summary>
        public void RemoveMask(MaskType mask)
        {
            ownedMasks.Remove(mask);
            SyncToSaveData();
        }

        /// <summary>
        /// Retourne la liste des masques possédés.
        /// </summary>
        public List<MaskType> GetOwnedMasks()
        {
            return new List<MaskType>(ownedMasks);
        }

        /// <summary>
        /// Charge les masques depuis les données de sauvegarde.
        /// </summary>
        public void LoadFromSaveData(SaveData save)
        {
            ownedMasks.Clear();

            if (save?.ownedMasks != null)
            {
                foreach (var maskId in save.ownedMasks)
                {
                    if (Enum.IsDefined(typeof(MaskType), maskId))
                    {
                        ownedMasks.Add((MaskType)maskId);
                    }
                }
            }

            Debug.Log($"[MaskInventory] Chargé {ownedMasks.Count} masques");
        }

        /// <summary>
        /// Synchronise les masques vers les données de sauvegarde.
        /// </summary>
        public void SyncToSaveData()
        {
            if (GameState.Instance?.CurrentSave == null) return;

            var save = GameState.Instance.CurrentSave;
            save.ownedMasks = new List<int>();

            foreach (var mask in ownedMasks)
            {
                save.ownedMasks.Add((int)mask);
            }
        }

        /// <summary>
        /// Réinitialise l'inventaire (nouvelle partie).
        /// </summary>
        public void Clear()
        {
            ownedMasks.Clear();
        }
    }
}
