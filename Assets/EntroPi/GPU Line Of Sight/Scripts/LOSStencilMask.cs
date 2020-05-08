using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace LOS
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Line of Sight/LOS Stencil Mask")]
    public class LOSStencilMask : MonoBehaviour
    {
        #region Constants

        private const CameraEvent MASK_CAMERA_EVENT = CameraEvent.BeforeImageEffectsOpaque;
        private const CameraEvent FORWARD_EXCLUDE_CAMERA_EVENT = CameraEvent.AfterForwardOpaque;
        private const CameraEvent DEFERRED_EXCLUDE_CAMERA_EVENT = CameraEvent.AfterFinalPass;

        #endregion Constants

        #region Public Properties

        public RenderTexture MaskTexture { get { return m_MaskTexture; } }

        #endregion Public Properties

        #region Private Properties

        private CameraEvent ExcludeCameraEvent
        {
            get
            {
                CameraEvent cameraEvent = FORWARD_EXCLUDE_CAMERA_EVENT;

                if (m_Camera != null && m_Camera.actualRenderingPath == RenderingPath.DeferredShading)
                {
                    cameraEvent = DEFERRED_EXCLUDE_CAMERA_EVENT;
                }

                return cameraEvent;
            }
        }

        private Mesh Quad
        {
            get
            {
                if (m_QuadMesh == null)
                {
                    Vector3[] vertices = new Vector3[] { new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, 0.5f, 0f), new Vector3(0.5f, -0.5f, 0f), new Vector3(-0.5f, 0.5f, 0f) };
                    Vector2[] uvs = new Vector2[] { new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0f), new Vector2(0f, 1f) };
                    int[] indices = new int[] { 0, 1, 2, 1, 0, 3 };

                    m_QuadMesh = new Mesh { vertices = vertices, uv = uvs, triangles = indices };
                    m_QuadMesh.RecalculateNormals();
                    m_QuadMesh.RecalculateBounds();
                    m_QuadMesh.hideFlags = HideFlags.HideAndDontSave;
                }

                return m_QuadMesh;
            }
        }

        #endregion Private Properties

        #region Private Data Members

        private Camera m_Camera;
        private CommandBuffer m_MaskCommandBuffer;
        private RenderTexture m_MaskTexture;
        private RenderTargetIdentifier m_MaskTextureID;
        private Mesh m_QuadMesh;

        #endregion Private Data Members

        #region MonoBehaviour Functions

        private void OnEnable()
        {
            m_Camera = GetComponent<Camera>();
            enabled &= Util.Verify(m_Camera != null, "Camera component not found.");

            if (enabled)
            {
                // Check for incompatible settings.
                CheckSettings(m_Camera);

                // Delay creating the required resources until the screen resolution is valid.
                StartCoroutine(SetupRoutine());

                // Add Culling Group to track visibility changes in all LOSStencilRenderer components.
                CullingGroup cullingGroup = LOSManager.Instance.AddCullingGroup(m_Camera);
                cullingGroup.onStateChanged = OnCullingGroupStateChanged;

                // Subscribe to event which triggers when a Stencil Renderer is removed from the LOS Manager.
                LOSManager.Instance.StencilRendererRemoved += OnStencilRendererRemovedFromLOSManager;
            }
        }

        private void OnDisable()
        {
            if (m_Camera != null)
            {
                LOSManager.Instance.StencilRendererRemoved -= OnStencilRendererRemovedFromLOSManager;

                // Remove Culling Group.
                LOSManager.Instance.RemoveCullingGroup(m_Camera);

                // Remove Mask Command Buffer.
                RemoveMaskCommandBuffer();

                // Remove remaining Command Buffers.
                m_Camera.RemoveCommandBuffers(ExcludeCameraEvent);
            }

            // Clean up Render Texture resource.
            if (m_MaskTexture != null)
            {
                ReleaseMaskRenderTexture();
            }
        }

        private void LateUpdate()
        {
            // Update the Culling Group data.
            LOSManager.Instance.UpdateBoundingSpheres();
        }

        #endregion MonoBehaviour Functions

        #region Private Functions

        /// <summary>
        /// This Coroutine delays the creation of the required resources until the screen resolution is valid
        /// </summary>
        private IEnumerator SetupRoutine()
        {
            // Always skip a single frame.
            // Screen width and height are incorrect in the first frame.
            yield return null;

            while (Screen.width == 0 || Screen.height == 0)
            {
                yield return null;
            }

            if (m_MaskTexture == null)
            {
                CreateMaskRenderTexture();
            }

            if (m_MaskCommandBuffer == null)
            {
                AddMaskCommandBuffer();
            }
        }

        /// <summary>
        /// Creates Render Texture to copy Stencil Buffer into
        /// </summary>
        private void CreateMaskRenderTexture()
        {
            Debug.Assert(Screen.width > 0 && Screen.height > 0, "Invalid screen size");

            m_MaskTexture = new RenderTexture(Screen.width, Screen.height, 24);
            m_MaskTexture.name = "LOS Stencil Mask";
            m_MaskTexture.hideFlags = HideFlags.HideAndDontSave;

            m_MaskTextureID = new RenderTargetIdentifier(m_MaskTexture);
        }

        /// <summary>
        /// Correctly release Render Texture Resource
        /// </summary>
        private void ReleaseMaskRenderTexture()
        {
            m_MaskTexture.Release();
            m_MaskTexture = null;
        }

        /// <summary>
        /// Adds Command Buffer to Camera that copies the Stencil Buffer into a black and white mask
        /// </summary>
        private void AddMaskCommandBuffer()
        {
            m_MaskCommandBuffer = new CommandBuffer();
            m_MaskCommandBuffer.name = "LOS Stencil Mask";

            // Clear mask render texture.
            m_MaskCommandBuffer.SetRenderTarget(m_MaskTextureID, BuiltinRenderTextureType.CameraTarget);
            m_MaskCommandBuffer.ClearRenderTarget(false, true, Color.clear);

            // Blit using a custom quad mesh.
            m_MaskCommandBuffer.DrawMesh(Quad, Matrix4x4.identity, Materials.StencilMask, 0, 0);

            m_Camera.AddCommandBuffer(MASK_CAMERA_EVENT, m_MaskCommandBuffer);
        }

        /// <summary>
        /// Removes mask Command Buffer from Camera
        /// </summary>
        private void RemoveMaskCommandBuffer()
        {
            if (m_MaskCommandBuffer != null)
            {
                m_Camera.RemoveCommandBuffer(MASK_CAMERA_EVENT, m_MaskCommandBuffer);
                m_MaskCommandBuffer = null;
            }
        }

        /// <summary>
        /// Culling Group Callback.
        /// Adds or removes Command Buffer to / from Camera based on LOSStencilRenderer visibility.
        /// </summary>
        private void OnCullingGroupStateChanged(CullingGroupEvent evt)
        {
            LOSStencilRenderer stencilRenderer = LOSManager.Instance[evt.index];

            if (stencilRenderer != null)
            {
                if (evt.hasBecomeVisible)
                {
                    m_Camera.AddCommandBuffer(ExcludeCameraEvent, stencilRenderer.RendererCommandBuffer);
                }
                else if (evt.hasBecomeInvisible)
                {
                    m_Camera.RemoveCommandBuffer(ExcludeCameraEvent, stencilRenderer.RendererCommandBuffer);
                }
            }
        }

        /// <summary>
        /// Subscribed to LOSManager event
        /// </summary>
        private void OnStencilRendererRemovedFromLOSManager(LOSStencilRenderer stencilRenderer)
        {
            if (m_Camera == null) return;

            // Remove the Command Buffer if the Stencil Renderer was visible before being removed
            if (LOSManager.Instance.IsLOSStencilRendererVisible(stencilRenderer, m_Camera))
            {
                m_Camera.RemoveCommandBuffer(ExcludeCameraEvent, stencilRenderer.RendererCommandBuffer);
            }
        }

        #endregion Private Functions

        #region Editor Functions

        /// <summary>
        /// Ensures there are no incompatible settings
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private static void CheckSettings(Camera currentCamera)
        {
            // Update AA settings if required.
            if (QualitySettings.antiAliasing > 0)
            {
                Debug.LogWarning("The LOS Stencil Mask component does currently not support MSAA.\nDisabling MSAA in Quality Settings!");
                QualitySettings.antiAliasing = 0;
            }

            // Update Camera Clear Flags if required.
            if (currentCamera.clearFlags == CameraClearFlags.Depth || currentCamera.clearFlags == CameraClearFlags.Nothing)
            {
                Debug.LogError("The Camera Clear Flags should be set to Solid Color or SkyBox.");
            }
        }

#if UNITY_EDITOR

        private void Update()
        {
            CheckSettings(m_Camera);
        }

        /// <summary>
        /// Resizes the Render Texture if the game window resolution changes in the editor
        /// </summary>
        private void OnPreRender()
        {
            if (m_MaskTexture != null && m_MaskTexture.IsCreated())
            {
                if (m_MaskTexture.width != Screen.width || m_MaskTexture.height != Screen.height)
                {
                    RemoveMaskCommandBuffer();

                    // Release current render texture.
                    m_MaskTexture.Release();

                    // Recreate render texture with correct resolution.
                    CreateMaskRenderTexture();

                    AddMaskCommandBuffer();
                }
            }
        }

#endif

        #endregion Editor Functions
    }
}