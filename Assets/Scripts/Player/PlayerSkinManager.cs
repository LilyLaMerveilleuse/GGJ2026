using System.Linq;
using Masks;
using UnityEngine;

namespace Bundles.SimplePlatformer2D.Scripts.Player
{
    /// <summary>
    /// Gère le changement de skin du joueur selon le masque équipé.
    /// Utilise des AnimatorOverrideControllers pour swapper les animations.
    /// </summary>
    public class PlayerSkinManager : MonoBehaviour
    {
        [System.Serializable]
        public class MaskSkin
        {
            public MaskType maskType;
            public AnimatorOverrideController animatorOverride;
        }

        [Header("References")]
        [SerializeField] private Animator animator;

        [Header("Skins")]
        [SerializeField] private RuntimeAnimatorController baseAnimator;
        [SerializeField] private MaskSkin[] maskSkins;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (baseAnimator == null && animator != null)
            {
                baseAnimator = animator.runtimeAnimatorController;
            }
        }

        private void OnEnable()
        {
            if (MaskInventory.Instance != null)
            {
                MaskInventory.Instance.onMaskObtained.AddListener(OnMaskObtained);
            }
        }

        private void OnDisable()
        {
            if (MaskInventory.Instance != null)
            {
                MaskInventory.Instance.onMaskObtained.RemoveListener(OnMaskObtained);
            }
        }

        private void Start()
        {
            // Appliquer le skin au démarrage si un masque est déjà équipé
            RefreshSkin();
        }

        private void OnMaskObtained(MaskType mask)
        {
            ApplySkin(mask);
        }

        /// <summary>
        /// Applique le skin correspondant au masque.
        /// </summary>
        public void ApplySkin(MaskType mask)
        {
            if (animator == null) return;

            // Chercher le skin correspondant
            foreach (var skin in maskSkins)
            {
                if (skin.maskType == mask && skin.animatorOverride != null)
                {
                    animator.runtimeAnimatorController = skin.animatorOverride;
                    Debug.Log($"[PlayerSkinManager] Skin appliqué: {mask}");
                    return;
                }
            }

            Debug.LogWarning($"[PlayerSkinManager] Aucun skin trouvé pour le masque: {mask}");
        }

        /// <summary>
        /// Remet le skin de base (sans masque).
        /// </summary>
        public void ResetToBaseSkin()
        {
            if (animator != null && baseAnimator != null)
            {
                animator.runtimeAnimatorController = baseAnimator;
                Debug.Log("[PlayerSkinManager] Skin de base restauré");
            }
        }

        /// <summary>
        /// Rafraîchit le skin selon les masques possédés.
        /// Applique le dernier masque obtenu.
        /// </summary>
        public void RefreshSkin()
        {
            if (MaskInventory.Instance == null) return;

            var ownedMasks = MaskInventory.Instance.GetOwnedMasks();

            // Chercher le premier masque possédé qui a un skin
            foreach (var skin in maskSkins)
            {
                if (ownedMasks.Contains(skin.maskType))
                {
                    ApplySkin(skin.maskType);
                    return;
                }
            }

            // Aucun masque avec skin → skin de base
            ResetToBaseSkin();
        }
    }
}
