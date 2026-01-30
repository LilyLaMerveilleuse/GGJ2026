using System.Collections;
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

        private void Awake()
        {
            confiner = GetComponent<CinemachineConfiner2D>();
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
            // Wait a frame for CameraBounds to register
            yield return null;

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
    }
}
