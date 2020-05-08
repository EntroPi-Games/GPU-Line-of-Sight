using UnityEngine;

namespace LOS
{
    public enum PixelOperation { Clamp, Include, Exclude };

    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Line of Sight/LOS Source")]
    public class LOSSource : MonoBehaviour, ILOSSource
    {
        #region Constants

        private const int MIN_TEXTURE_SIZE = 1;
        private const int MAX_TEXTURE_SIZE = 4096;

        #endregion Constants

        #region Exposed Data Members

        [Header("Mask Settings", order = 0)]
        [Tooltip("Color rendered into mask")]
        [SerializeField]
        private Color m_MaskColor = Color.white;

        [Tooltip("Intensity of color rendered into mask")]
        [Range(0f, 10f)]
        [SerializeField]
        private float m_MaskIntensity = 1.0f;

        [Tooltip("Inverts mask")]
        [SerializeField]
        private bool m_MaskInvert = false;

        [Header("Render Target Settings", order = 1)]
        [Tooltip("Width of target render texture")]
        [SerializeField]
        private int m_RenderTargetWidth = 512;

        [Tooltip("Height of target render texture")]
        [SerializeField]
        private int m_RenderTargetHeight = 512;

        [Header("Fade Settings", order = 2)]
        [Tooltip("Fade over distance amount")]
        [Range(0.0f, 1f)]
        [SerializeField]
        private float m_DistanceFade = 0.1f;

        [Tooltip("Edge fade amount")]
        [Range(0.0f, 1f)]
        [SerializeField]
        private float m_EdgeFade = 0.1f;

        [Header("Backface Settings", order = 4)]
        [Tooltip("Excludes backfacing faces from mask")]
        [SerializeField]
        private bool m_ExcludeBackfaces = true;

        [Tooltip("Fades the edge of the backface transition in the mask")]
        [Range(0.0f, 1f)]
        [SerializeField]
        private float m_BackfacesFade = 0.1f;

        [Header("Advanced Settings", order = 4)]
        [Tooltip("Reduces artifacts in mask, similar to shadow bias setting")]
        [Range(0.0f, 1f)]
        [SerializeField]
        private float m_MinVariance = 0.1f;

        [Tooltip("Sets how pixels above and below the cameras frustum are handled")]
        [SerializeField]
        private PixelOperation m_OutOfBoundArea = PixelOperation.Clamp;

        #endregion Exposed Data Members

        #region Public Properties

        public Color MaskColor
        {
            get { return m_MaskColor; }
            set { m_MaskColor = value; }
        }

        public float MaskIntensity
        {
            get { return m_MaskIntensity; }
            set { m_MaskIntensity = value; }
        }

        public bool MaskInvert
        {
            get { return m_MaskInvert; }
            set { m_MaskInvert = value; }
        }

        public int RenderTargetWidth
        {
            get { return m_RenderTargetWidth; }
            set { m_RenderTargetWidth = Mathf.Clamp(value, MIN_TEXTURE_SIZE, MAX_TEXTURE_SIZE); }
        }

        public int RenderTargetHeight
        {
            get { return m_RenderTargetHeight; }
            set { m_RenderTargetHeight = Mathf.Clamp(value, MIN_TEXTURE_SIZE, MAX_TEXTURE_SIZE); }
        }

        public float DistanceFade
        {
            get { return m_DistanceFade; }
            set { m_DistanceFade = Mathf.Clamp01(value); }
        }

        public float EdgeFade
        {
            get { return m_EdgeFade; }
            set { m_EdgeFade = Mathf.Clamp01(value); }
        }

        public bool ExcludeBackfaces
        {
            get { return m_ExcludeBackfaces; }
            set { m_ExcludeBackfaces = value; }
        }

        public float BackfacesFade
        {
            get { return m_BackfacesFade; }
            set { m_BackfacesFade = value; }
        }

        public float MinVariance
        {
            get { return m_MinVariance; }
            set { m_MinVariance = Mathf.Clamp01(value); }
        }

        public PixelOperation OutOfBoundArea
        {
            get { return m_OutOfBoundArea; }
            set { m_OutOfBoundArea = value; }
        }

        public Camera SourceCamera
        {
            get { return m_Camera; }
        }

        public Bounds CameraBounds
        {
            get { return m_CameraBounds; }
        }

        public GameObject GameObject
        {
            get { return gameObject; }
        }

        public Plane[] FrustumPlanes
        {
            get { return m_FrustumPlanes; }
        }

        public Vector4 SourceInfo
        {
            get
            {
                Vector4 sourceInfo = Vector4.zero;

                if (m_Camera != null)
                {
                    sourceInfo = m_Camera.transform.position;
                    sourceInfo.w = m_Camera.farClipPlane;
                }

                return sourceInfo;
            }
        }

        public bool IsVisible
        {
            get { return m_IsVisibile; }
        }

        #endregion Public Properties

        #region Private Data Members

        private Plane[] m_FrustumPlanes = new Plane[6];
        private Camera m_Camera;
        private Bounds m_CameraBounds;

        private bool m_IsVisibile = true;

        #endregion Private Data Members

        #region MonoBehaviour Functions

        private void Awake()
        {
            // Get camera component.
            m_Camera = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            // Register with LOS manager singleton.
            LOSManager.Instance.AddLOSSource(this);

            // Check if component can be enabled.
            enabled &= Util.Verify(m_Camera != null, "Camera Component missing");

            if (enabled)
            {
                // Alert user if camera is set to orthographic.
                Debug.Assert(!m_Camera.orthographic, "Cameras attached to an LOS source can't be orthographic, changing to perspective!");

                // Initiliaze source camera.
                LOSHelper.InitSourceCamera(m_Camera);

                // Initiliaze bounds.
                m_CameraBounds = LOSHelper.CalculateSourceBounds(m_Camera);

                // Initiliaze frustum planes.
                LOSHelper.ExtractFrustumPlanes(m_FrustumPlanes, m_Camera);
            }
        }

        private void OnDisable()
        {
            // Unregister with LOS manager singleton.
            LOSManager.Instance.RemoveLOSSource(this);
        }

        private void Update()
        {
            // Check if this source's transform has changed.
            if (transform.hasChanged)
            {
                // Recalculate camera bounds.
                m_CameraBounds = LOSHelper.CalculateSourceBounds(m_Camera);

                // Reset transform changed flag.
                transform.hasChanged = false;
            }

            // Check if this source and it's frustum are visible to the camera used by the LOS mask.
            m_IsVisibile = GeometryUtility.TestPlanesAABB(LOSMask.CameraFrustumPlanes, m_CameraBounds);

            // Recalculate frustum planes.
            LOSHelper.ExtractFrustumPlanes(m_FrustumPlanes, m_Camera);
        }

        private void OnDrawGizmos()
        {
            // Draw gizmo in editor.
            DrawGizmo(false);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw gizmo in editor.
            DrawGizmo(true);
        }

        #endregion MonoBehaviour Functions

        #region Private Functions

        /// <summary>
        /// Draws camera gizmo showing the correct frustum in editor scene.
        /// </summary>
        private void DrawGizmo(bool selected)
        {
            if (m_Camera == null) return;

            Color gizmoColor = m_MaskColor;
            gizmoColor.a = selected ? 0.9f : 0.25f;
            Gizmos.color = gizmoColor;

            float aspectRatio = (float)m_RenderTargetWidth / (float)m_RenderTargetHeight;
            Gizmos.matrix = Matrix4x4.TRS(m_Camera.transform.position, m_Camera.transform.rotation, Vector3.one);

#if UNITY_5_6_OR_NEWER
            Gizmos.DrawFrustum(transform.position, m_Camera.fieldOfView, m_Camera.farClipPlane, m_Camera.nearClipPlane, 1);
#else
            Gizmos.DrawFrustum(Vector3.zero, m_Camera.fieldOfView, m_Camera.farClipPlane, m_Camera.nearClipPlane, aspectRatio);
#endif
        }

        #endregion Private Functions
    }
}