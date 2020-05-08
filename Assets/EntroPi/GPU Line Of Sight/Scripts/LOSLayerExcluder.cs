using UnityEngine;

namespace LOS
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Line of Sight/LOS Layer Excluder")]
    public class LOSLayerExcluder : MonoBehaviour
    {
        [Tooltip("Selects which layers to exclude from the line of sight")]
        [SerializeField]
        private LayerMask m_ExcludeLayers;

        private Camera m_ExcludeCamera;

        #region MonoBehaviour Functions

        private void OnEnable()
        {
            // Disable script when Deferred rendering is used
            if (GetComponent<Camera>().actualRenderingPath == RenderingPath.DeferredShading)
            {
                Debug.LogError("The LOS Layer Excluder script component does not support Deferred Rendering!\nPlease use the LOS Stencil Mask script component instead.");
                enabled = false;
            }
        }

        private void Start()
        {
            // Creates child GameObject with camera to render exclude objects to mask.
            m_ExcludeCamera = CreateExcludeCamera();
        }

        private void LateUpdate()
        {
            if (m_ExcludeCamera)
                SyncCamera(m_ExcludeCamera);
        }

        #endregion MonoBehaviour Functions

        #region Private Functions

        /// <summary>
        /// Creates extra camera for rendering excluded objects.
        /// </summary>
        private Camera CreateExcludeCamera()
        {
            // Create new empty child gameObject.
            GameObject cameraObject = null;

            Transform childTransform = transform.FindChild("Exclude Camera");
            if (null != childTransform)
                cameraObject = childTransform.gameObject;

            if (null == cameraObject)
            {
                cameraObject = new GameObject("Exclude Camera");
                cameraObject.transform.parent = this.gameObject.transform;

                // Reset transform.
                cameraObject.transform.localPosition = Vector3.zero;
                cameraObject.transform.localRotation = Quaternion.identity;
                cameraObject.transform.localScale = Vector3.one;
            }

            Camera excludeCamera = cameraObject.GetComponent<Camera>();

            if (null == excludeCamera)
                excludeCamera = cameraObject.AddComponent<Camera>();

            return excludeCamera;
        }

        /// <summary>
        /// Renders the excluded objects to targetTexture
        /// </summary>
        private void SyncCamera(Camera excludeCamera)
        {
            // Copy settings from primary camera.
            excludeCamera.CopyFrom(GetComponent<Camera>());

            // Change Camera Culling Mask, Clear Flags and Depth.
            excludeCamera.cullingMask = m_ExcludeLayers.value;
            excludeCamera.clearFlags = CameraClearFlags.Nothing;
            excludeCamera.depth = excludeCamera.depth + 1;

            // Set parent camera culling mask.
            GetComponent<Camera>().cullingMask = GetComponent<Camera>().cullingMask & ~m_ExcludeLayers.value;
        }

        #endregion Private Functions
    }
}