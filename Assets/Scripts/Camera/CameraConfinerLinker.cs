using System.Collections;
using SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

namespace Bundles.SimplePlatformer2D.Scripts.CameraSystem
{
    /// <summary>
    /// Automatically links the CinemachineConfiner2D to the CameraBounds in each scene.
    /// Attach this to your persistent Cinemachine camera.
    /// </summary>
    [RequireComponent(typeof(CinemachineConfiner2D))]
    public class CameraConfinerLinker : MonoBehaviour
    {
        private CinemachineConfiner2D confiner;
        private CinemachineCamera cinemachineCamera;

        private void Awake()
        {
            confiner = GetComponent<CinemachineConfiner2D>();
            cinemachineCamera = GetComponent<CinemachineCamera>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            // Also try to link on enable in case scene is already loaded
            StartCoroutine(LinkConfinerDelayed());
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(LinkConfinerDelayed());
        }

        private IEnumerator LinkConfinerDelayed()
        {
            // Wait a frame for entry points and CameraBounds to register
            yield return null;

            // Téléporter le joueur au bon entry point
            if (GameState.Instance != null)
            {
                GameState.Instance.TeleportPlayerToEntryPoint();
            }

            // Téléporter la caméra sur le joueur
            TeleportCameraToPlayer();

            if (CameraBounds.Current != null)
            {
                confiner.BoundingShape2D = CameraBounds.Current.BoundingCollider;
                confiner.InvalidateBoundingShapeCache();
            }
            else
            {
                confiner.BoundingShape2D = null;
            }
        }

        private void TeleportCameraToPlayer()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) return;

            Vector3 playerPos = player.transform.position;

            // Forcer la position de Cinemachine instantanément
            if (cinemachineCamera != null)
            {
                // Calculer la position cible de la caméra (avec le bon Z)
                float cameraZ = Camera.main != null ? Camera.main.transform.position.z : -10f;
                Vector3 targetPos = new Vector3(playerPos.x, playerPos.y, cameraZ);

                // Forcer la position sans transition
                cinemachineCamera.ForceCameraPosition(targetPos, Quaternion.identity);

                // Notifier que la cible s'est téléportée (pour reset le damping interne)
                if (cinemachineCamera.Follow != null)
                {
                    cinemachineCamera.OnTargetObjectWarped(cinemachineCamera.Follow, Vector3.zero);
                }
            }

            // Téléporter aussi la caméra principale directement
            if (Camera.main != null)
            {
                Vector3 camPos = Camera.main.transform.position;
                Camera.main.transform.position = new Vector3(playerPos.x, playerPos.y, camPos.z);
            }
        }
    }
}
