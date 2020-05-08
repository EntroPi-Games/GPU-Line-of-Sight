using System;
using System.Collections.Generic;
using UnityEngine;

namespace LOS
{
    public enum LOSQualityLevel { Low, Medium, High }

    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Line of Sight/LOS Mask")]
    public class LOSMask : MonoBehaviour
    {
        #region Exposed Data Members

        [Tooltip("Reference to the LOS Buffer Storage component used by this Mask")]
        [SerializeField]
        private LOSBufferStorage m_BufferStorage;

        [Tooltip("Level of quality")]
        [SerializeField]
        private LOSQualityLevel m_QualityLevel = LOSQualityLevel.Medium;

        [Tooltip("Enable HDR rendering of this mask")]
        [SerializeField]
        private bool m_HDRMask = true;

        #endregion Exposed Data Members

        #region Public Properties

        public LOSQualityLevel QualityLevel
        {
            get { return m_QualityLevel; }
            set { m_QualityLevel = value; }
        }

        public static Plane[] CameraFrustumPlanes
        {
            get { return m_CameraFrustumPlanes; }
        }

        #endregion Public Properties

        #region Private Data Members

        private Dictionary<int, Cubemap> m_CubeMaps = new Dictionary<int, Cubemap>();

        private static Plane[] m_CameraFrustumPlanes = new Plane[6];

        private Camera m_Camera;
        private LOSStencilMask m_StencilMask;

        #endregion Private Data Members

        #region MonoBehaviour Functions

        private void Awake()
        {
            // Get and verify camera component
            m_Camera = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            // Check if this component can be enabled.
            // Disable if post processing not supported.
            enabled &= Util.Verify(SystemInfo.supportsImageEffects, "Image effects not supported.");

            // Disable if buffer storage is not assigned.
            enabled &= Util.Verify(m_BufferStorage != null, "LOS Buffer Storage property not assigned.");

            // Disable if camera component is missing.
            enabled &= Util.Verify(m_Camera != null, "Camera component missing.");

            if (enabled)
            {
                // Make sure Frustum planes are initiliazed.
                LOSHelper.ExtractFrustumPlanes(m_CameraFrustumPlanes, m_Camera);

                m_StencilMask = GetComponent<LOSStencilMask>();
            }
        }

        private void OnDisable()
        {
            // Remove all cubemaps from dictionary.
            m_CubeMaps.Clear();

            // Destroy Resources.
            Materials.DestroyResources();
            Shaders.DestroyResources();
        }

        private void OnPreRender()
        {
            if (m_Camera == null) return;

            // Make sure we can acces the cameras depth buffer in our shader.
            m_Camera.depthTextureMode = DepthTextureMode.DepthNormals;

            // Update Mask Camera frutsum planes if needed.
            if (transform.hasChanged)
            {
                LOSHelper.ExtractFrustumPlanes(m_CameraFrustumPlanes, m_Camera);
                transform.hasChanged = false;
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (m_Camera == null) return;

            // Calculate Frustum origins and rays for mask camera.
            Matrix4x4 frustumOrigins;
            Matrix4x4 frustumRays;
            LOSHelper.CalculateViewVectors(m_Camera, out frustumRays, out frustumOrigins);

            // Push parameters which are identical for all LOS sources.
            Materials.Mask.SetMatrix(ShaderID.FrustumRays, frustumRays);
            Materials.Mask.SetMatrix(ShaderID.FrustumOrigins, frustumOrigins);
            Materials.Mask.SetMatrix(ShaderID.WorldToCameraMatrix, m_Camera.worldToCameraMatrix);

            // Store original skybox.
            Material originalSkybox = RenderSettings.skybox;
            // Set-up skybox material which clears render texture to farplane depth.
            RenderSettings.skybox = Materials.SkyBox;

            // Create Mask Render Texture.
            RenderTexture maskRenderTexture = CreateMaskRenderTexture();

            // Get list with all LOS sources.
            List<LOSSource> losSources = LOSManager.Instance.LOSSources;
            // Iterate over all LOS sources.
            for (int i = 0; i < losSources.Count; i++)
            {
                RenderSourceToMask(losSources[i], ref maskRenderTexture);
            }

            // Revert original skybox.
            RenderSettings.skybox = originalSkybox;

            // Get unmodified screen buffer.
            Materials.Combine.SetTexture(ShaderID.PreEffectTex, m_BufferStorage.BufferTexture);

            // Set-up material.
            Materials.Combine.SetTexture(ShaderID.MaskTex, maskRenderTexture);

            // Check if Stencil Mask component is used.
            bool isStencilMaskEnabled = (m_StencilMask != null) && (m_StencilMask.enabled == true);

            if (isStencilMaskEnabled)
            {
                // Set Stencil Mask texture.
                Materials.Combine.SetTexture(ShaderID.StencilMaskTex, m_StencilMask.MaskTexture);
            }

            // Render final effect.
            Graphics.Blit(source, destination, Materials.Combine, isStencilMaskEnabled ? 1 : 0);

            RenderTexture.ReleaseTemporary(maskRenderTexture);
        }

        #endregion MonoBehaviour Functions

        #region Private Functions

        /// <summary>
        /// Updates mask for specific LOS source.
        /// </summary>
        private void RenderSourceToMask(LOSSource losSource, ref RenderTexture maskRenderTexture)
        {
            Camera sourceCamera = losSource.SourceCamera;

            if (sourceCamera == null) return;

            // Set "skybox" material farplane.
            Materials.SkyBox.SetVector(ShaderID.FarPlane, new Vector4(sourceCamera.farClipPlane, sourceCamera.farClipPlane, sourceCamera.farClipPlane, sourceCamera.farClipPlane));

            // Source depth texture resolution.
            int sourceBufferWidth = CalculateRTSize(losSource.RenderTargetWidth, m_QualityLevel);
            int sourceBufferHeight = CalculateRTSize(losSource.RenderTargetHeight, m_QualityLevel);

            // Create temporary rendertexture for rendering linear depth.
            RenderTexture sourceBuffer = RenderTexture.GetTemporary(sourceBufferWidth, sourceBufferHeight, 16, RenderTextureFormat.RGFloat);
            sourceBuffer.filterMode = FilterMode.Trilinear;
            sourceBuffer.wrapMode = TextureWrapMode.Clamp;

            // Set camera render target.
            sourceCamera.targetTexture = sourceBuffer;

            // Render depth from source Camera.
            sourceCamera.RenderWithShader(Shaders.Depth, null);

            //Push LOS source specific parameters.
            Materials.Mask.SetTexture(ShaderID.SourceDepthTex, sourceBuffer);
            Materials.Mask.SetMatrix(ShaderID.SourceWorldProj, sourceCamera.projectionMatrix * sourceCamera.worldToCameraMatrix);
            Materials.Mask.SetVector(ShaderID.SourceInfo, losSource.SourceInfo);
            Materials.Mask.SetVector(ShaderID.Settings, new Vector4(losSource.DistanceFade, losSource.EdgeFade, losSource.MinVariance, losSource.BackfacesFade));
            Materials.Mask.SetVector(ShaderID.Flags, new Vector4(PixelOperation.Clamp == losSource.OutOfBoundArea ? 0.0f : 1.0f, PixelOperation.Exclude == losSource.OutOfBoundArea ? -1.0f : 1.0f, losSource.MaskInvert ? 1.0f : 0.0f, losSource.ExcludeBackfaces ? 1.0f : 0.0f));
            Materials.Mask.SetColor(ShaderID.ColorMask, losSource.MaskColor * losSource.MaskIntensity);

            // Set Correct material pass.
            Materials.Mask.SetPass(0);

            // Render Mask.
            IndexedGraphicsBlit(maskRenderTexture);

            // Release linear depth render texture.
            RenderTexture.ReleaseTemporary(sourceBuffer);
        }

        /// <summary>
        /// Creates a temporary render texture used for rendering the mask
        /// </summary>
        private RenderTexture CreateMaskRenderTexture()
        {
            // Mask RenderTexture settings.
            int maskDownSampler = LOSQualityLevel.Medium > m_QualityLevel ? 1 : 0;
            int maskBufferWidth = Screen.width >> maskDownSampler;
            int maskBufferHeight = Screen.height >> maskDownSampler;

            RenderTextureFormat maskTextureFormat = m_HDRMask ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
            FilterMode maskFilterMode = FilterMode.Bilinear;

            // Get mask temporary render texture.
            RenderTexture maskRenderTexture = RenderTexture.GetTemporary(maskBufferWidth, maskBufferHeight, 0, maskTextureFormat);
            maskRenderTexture.filterMode = maskFilterMode;

            // Clear mask rendertexture to black.
            RenderTexture.active = maskRenderTexture;
            GL.Clear(true, true, new Color(0, 0, 0, 0));

            return maskRenderTexture;
        }

        /// <summary>
        /// Renders quad to full screen with index for interpolating frustum corners
        /// </summary>
        public static void IndexedGraphicsBlit(RenderTexture destination)
        {
            RenderTexture.active = destination;

            GL.PushMatrix();
            GL.LoadOrtho();

            GL.Begin(GL.QUADS);

            // Bottom left corner.
            GL.MultiTexCoord2(0, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, 3.0f);

            // Bottom right corner.
            GL.MultiTexCoord2(0, 1.0f, 0.0f);
            GL.Vertex3(1.0f, 0.0f, 2.0f);

            // Top right corner.
            GL.MultiTexCoord2(0, 1.0f, 1.0f);
            GL.Vertex3(1.0f, 1.0f, 1.0f);

            // Top left corner.
            GL.MultiTexCoord2(0, 0.0f, 1.0f);
            GL.Vertex3(0.0f, 1.0f, 0.0f);

            GL.End();
            GL.PopMatrix();

            RenderTexture.active = null;
        }

        #endregion Private Functions

        #region Static Functions

        /// <summary>
        /// Calculates the size of the Render texture according to the quality setting
        /// </summary>
        private static int CalculateRTSize(int size, LOSQualityLevel level)
        {
            const int maxTextureSize = 4096;
            int finalSize = size;

            if (level > LOSQualityLevel.Medium)
            {
                finalSize *= 2;

                Debug.Assert(finalSize <= maxTextureSize, "Render texture size to big, can't be larger than " + maxTextureSize);
                //Make sure size is not bigger than max texture size
                Math.Min(finalSize, maxTextureSize);
            }
            else if (level < LOSQualityLevel.Medium)
            {
                finalSize = finalSize >> 1;
            }

            return finalSize;
        }

        #endregion Static Functions
    }
}