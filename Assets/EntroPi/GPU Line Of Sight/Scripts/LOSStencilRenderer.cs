using UnityEngine;
using UnityEngine.Rendering;

namespace LOS
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Renderer))]
    [AddComponentMenu("Line of Sight/LOS Stencil Renderer")]
    public class LOSStencilRenderer : MonoBehaviour
    {
        #region Serialized Data Members

        [SerializeField]
        private bool m_IsDynamicBatchingDisabled = false;

        [SerializeField]
        private bool m_IsStatic = false;

        #endregion Serialized Data Members

        #region Public Properties

        /// <summary>
        /// Returns Bounding Sphere used by Culling Group
        /// </summary>
        public BoundingSphere RendererBoundingSphere
        {
            get
            {
                BoundingSphere boundingSphere = new BoundingSphere();

                Debug.Assert(m_Renderer != null, "Renderer not yet initialized.");

                if (m_Renderer != null)
                {
                    Vector3 center = m_Renderer.bounds.center;

                    Vector3 boundExtents = m_Renderer.bounds.extents;

                    float radius = Mathf.Max(boundExtents.x, boundExtents.y);
                    radius = Mathf.Max(radius, boundExtents.z);

                    boundingSphere = new BoundingSphere(center, radius);
                }

                return boundingSphere;
            }
        }

        public bool IsStatic
        {
            get { return m_IsStatic; }
        }

        public CommandBuffer RendererCommandBuffer { get { return m_CommandBuffer; } }

        #endregion Public Properties

        #region Private Data Members

        private CommandBuffer m_CommandBuffer;
        private Renderer m_Renderer;

        #endregion Private Data Members

        #region MonoBehaviour Functions

        private void OnEnable()
        {
            m_Renderer = GetComponent<Renderer>();
            enabled &= Util.Verify(m_Renderer != null, "Failed to get Renderer component.");

            if (enabled)
            {
                if (Application.isPlaying && m_IsDynamicBatchingDisabled && !IsStatic)
                {
                    // Assign unique material to prevent dynamic batching
                    m_Renderer.material = m_Renderer.material;
                }

                if (m_CommandBuffer == null)
                {
                    m_CommandBuffer = CreateCommandBuffer(m_Renderer);
                }

                // Register with LOSManager
                LOSManager.Instance.AddLOSStencilRenderer(this);
            }
        }

        private void OnDisable()
        {
            if (m_Renderer != null)
            {
                // Unregister with LOSManager
                LOSManager.Instance.RemoveLOSStencilRenderer(this);
            }
        }

#if UNITY_EDITOR

        private void Update()
        {
            m_IsStatic = gameObject.isStatic;
        }

#endif

        #endregion MonoBehaviour Functions

        #region Static Functions

        /// <summary>
        /// Creates Command Buffer that will draw Renderer into the Stencil Buffer
        /// </summary>
        private static CommandBuffer CreateCommandBuffer(Renderer renderer)
        {
            CommandBuffer commandBuffer = new CommandBuffer();
            commandBuffer.name = "LOS Stencil Renderer: " + renderer.name;

            commandBuffer.DrawRenderer(renderer, Materials.StencilRenderer);

            return commandBuffer;
        }

        #endregion Static Functions
    }
}