using UnityEngine;

namespace LOS
{
    public class Materials
    {
        #region Constants

        // Paths to material shaders

        private const string MaskShaderPath = "Shaders/LOSMask";
        private const string CombineShaderPath = "Shaders/LOSCombine";
        private const string SkyBoxShaderPath = "Shaders/LOSSkyBox";
        private const string StencilRendererShaderPath = "Shaders/LOSStencilRenderer";
        private const string StencilMaskShaderPath = "Shaders/LOSStencilMask";
        private const string DebugShaderPath = "Shaders/LOSDebug";

        #endregion Constants

        #region Public Properties

        public static Material Mask
        {
            get
            {
                if (m_MaskMaterial == null)
                    m_MaskMaterial = CreateMaterial(MaskShaderPath);

                return m_MaskMaterial;
            }
        }

        public static Material Combine
        {
            get
            {
                if (m_CombineMaterial == null)
                    m_CombineMaterial = CreateMaterial(CombineShaderPath);

                return m_CombineMaterial;
            }
        }

        public static Material SkyBox
        {
            get
            {
                if (m_SkyBoxMaterial == null)
                    m_SkyBoxMaterial = CreateMaterial(SkyBoxShaderPath);

                return m_SkyBoxMaterial;
            }
        }

        public static Material StencilRenderer
        {
            get
            {
                if (m_StencilRenderer == null)
                    m_StencilRenderer = CreateMaterial(StencilRendererShaderPath);

                return m_StencilRenderer;
            }
        }

        public static Material StencilMask
        {
            get
            {
                if (m_StencilMask == null)
                    m_StencilMask = CreateMaterial(StencilMaskShaderPath);

                return m_StencilMask;
            }
        }

        public static Material Debug
        {
            get
            {
                if (m_Debug == null)
                    m_Debug = CreateMaterial(DebugShaderPath);

                return m_Debug;
            }
        }

        #endregion Public Properties

        #region Private Data members

        private static Material m_MaskMaterial;
        private static Material m_CombineMaterial;
        private static Material m_SkyBoxMaterial;
        private static Material m_StencilRenderer;
        private static Material m_StencilMask;
        private static Material m_Debug;

        #endregion Private Data members

        /// <summary>
        /// Destroys all resources
        /// </summary>
        public static void DestroyResources()
        {
            if (m_MaskMaterial != null)
            {
                Object.DestroyImmediate(m_MaskMaterial);
                m_MaskMaterial = null;
            }
            if (m_CombineMaterial != null)
            {
                Object.DestroyImmediate(m_CombineMaterial);
                m_CombineMaterial = null;
            }
            if (m_SkyBoxMaterial != null)
            {
                Object.DestroyImmediate(m_SkyBoxMaterial);
                m_SkyBoxMaterial = null;
            }
            if (m_StencilRenderer != null)
            {
                Object.DestroyImmediate(m_StencilRenderer);
                m_StencilRenderer = null;
            }
            if (m_StencilMask != null)
            {
                Object.DestroyImmediate(m_StencilMask);
                m_StencilMask = null;
            }
            if (m_Debug != null)
            {
                Object.DestroyImmediate(m_Debug);
                m_Debug = null;
            }
        }

        /// <summary>
        /// Creates and returns material from shader.
        /// </summary>
        private static Material CreateMaterial(string shaderResource)
        {
            Material material = null;

            Shader shader = Resources.Load(shaderResource, typeof(Shader)) as Shader;

            if (Util.Verify(Shaders.CheckShader(shader)))
            {
                material = new Material(shader);
                material.hideFlags = HideFlags.HideAndDontSave;
            }

            UnityEngine.Debug.Assert(material != null, "Failed to created material from shader: " + shaderResource);

            return material;
        }
    }

    public class Shaders
    {
        #region Constants

        // Paths to shaders

        private const string DepthShaderPath = "Shaders/LOSDepth";
        private const string DepthRGBAShaderPath = "Shaders/LOSDepthRGBA";

        #endregion Constants

        #region Public Properties

        public static Shader Depth
        {
            get
            {
                if (m_Depth == null)
                    m_Depth = LoadShader(DepthShaderPath);

                return m_Depth;
            }
        }

        #endregion Public Properties

        #region Private Data members

        private static Shader m_Depth;

        #endregion Private Data members

        /// <summary>
        /// Destroys all resources
        /// </summary>
        public static void DestroyResources()
        {
            m_Depth = null;
        }

        /// <summary>
        /// Creates and returns shader.
        /// </summary>
        private static Shader LoadShader(string shaderResource)
        {
            Shader shader = Resources.Load(shaderResource, typeof(Shader)) as Shader;

            Debug.Assert(shader != null, "Failed to load shader: " + shaderResource);
            Debug.Assert(CheckShader(shader), "Shader not supported: " + shaderResource);

            return shader;
        }

        /// <summary>
        /// Checks if shader exists and is supported.
        /// </summary>
        public static bool CheckShader(Shader shader)
        {
            return (shader != null && shader.isSupported);
        }
    }

    public class ShaderID
    {
        #region Shader IDs

        public static int FrustumRays
        {
            get { return Shader.PropertyToID("_FrustumRays"); }
        }

        public static int FrustumOrigins
        {
            get { return Shader.PropertyToID("_FrustumOrigins"); }
        }

        public static int WorldToCameraMatrix
        {
            get { return Shader.PropertyToID("_WorldToCameraMatrix"); }
        }

        public static int SourceDepthTex
        {
            get { return Shader.PropertyToID("_SourceDepthTex"); }
        }

        public static int SourceDepthCube
        {
            get { return Shader.PropertyToID("_SourceDepthCube"); }
        }

        public static int SourceWorldProj
        {
            get { return Shader.PropertyToID("_SourceWorldProj"); }
        }

        public static int SourceInfo
        {
            get { return Shader.PropertyToID("_SourceInfo"); }
        }

        public static int Settings
        {
            get { return Shader.PropertyToID("_Settings"); }
        }

        public static int Flags
        {
            get { return Shader.PropertyToID("_Flags"); }
        }

        public static int ColorMask
        {
            get { return Shader.PropertyToID("_ColorMask"); }
        }

        public static int FarPlane
        {
            get { return Shader.PropertyToID("_FarPlane"); }
        }

        public static int PreEffectTex
        {
            get { return Shader.PropertyToID("_PreEffectTex"); }
        }

        public static int MaskTex
        {
            get { return Shader.PropertyToID("_MaskTex"); }
        }

        public static int StencilMaskTex
        {
            get { return Shader.PropertyToID("_StencilMask"); }
        }

        public static int DebugTex
        {
            get { return Shader.PropertyToID("_DebugTex"); }
        }

        #endregion Shader IDs
    }
}