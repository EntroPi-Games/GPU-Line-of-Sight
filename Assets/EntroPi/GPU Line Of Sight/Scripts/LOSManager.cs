using System;
using System.Collections.Generic;
using UnityEngine;

namespace LOS
{
    public class LOSManager
    {
        #region Singleton

        private static LOSManager m_Instance;

        private LOSManager()
        {
        }

        public static LOSManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new LOSManager();
                }
                return m_Instance;
            }
        }

        #endregion Singleton

        #region Private Data Members

        private List<LOSSource> m_LOSSources = new List<LOSSource>();
        private List<LOSStencilRenderer> m_LOSStencilRenderers = new List<LOSStencilRenderer>();
        private BoundingSphere[] m_BoundingSpheres = new BoundingSphere[512];
        private Dictionary<Camera, CullingGroup> m_CullingGroups = new Dictionary<Camera, CullingGroup>();

        #endregion Private Data Members

        #region Public Properties

        /// <summary>
        /// Event which is triggered when a Stencil Renderer is removed
        /// </summary>
        public event Action<LOSStencilRenderer> StencilRendererRemoved;

        /// <summary>
        /// Returns list containing all LOSSources in the current scene.
        /// </summary>
        public List<LOSSource> LOSSources
        {
            get { return m_LOSSources; }
        }

        /// <summary>
        /// Returns LOS Stencil Renderer at given index
        /// </summary>
        public LOSStencilRenderer this[int index]
        {
            get
            {
                LOSStencilRenderer stencilRenderer = null;

                if (index >= 0 && index < m_LOSStencilRenderers.Count)
                {
                    stencilRenderer = m_LOSStencilRenderers[index];
                }

                Debug.Assert(stencilRenderer != null, "Failed to return LOSStencilRenderer");

                return stencilRenderer;
            }
        }

        /// <summary>
        /// Return numbers of cameras that are currently visible and rendering.
        /// </summary>
        public int ActiveCameraCount
        {
            get
            {
                int visibleSourceCount = 0;

                foreach (ILOSSource source in m_LOSSources)
                {
                    if (source.IsVisible)
                        ++visibleSourceCount;
                }

                return visibleSourceCount;
            }
        }

        /// <summary>
        /// Returns total number of cameras in the scene.
        /// </summary>
        public int CameraCount
        {
            get { return m_LOSSources.Count; }
        }

        #endregion Public Properties

        #region Public Functions

        public void AddLOSSource(LOSSource source)
        {
            Debug.Assert(!m_LOSSources.Contains(source), "LOSSource already in list, can't add");

            m_LOSSources.Add(source);
        }

        public void RemoveLOSSource(LOSSource source)
        {
            Debug.Assert(m_LOSSources.Contains(source), "LOSSource not found in list, can't remove");

            m_LOSSources.Remove(source);
        }

        // Adds LOS Stencil Renderer and updates Culling Groups
        public void AddLOSStencilRenderer(LOSStencilRenderer stencilRenderer)
        {
            Debug.Assert(!m_LOSStencilRenderers.Contains(stencilRenderer), "LOSStencilRenderer already in list, can't add");

            int index = m_LOSStencilRenderers.Count;

            m_LOSStencilRenderers.Add(stencilRenderer);

            // Increase the capacity of the Bounding Spheres Array if needed
            if (m_BoundingSpheres.Length <= index)
            {
                IncreaseBoundingSphereArrayCapacity();
            }

            m_BoundingSpheres[index] = stencilRenderer.RendererBoundingSphere;

            Debug.Assert(m_LOSStencilRenderers.IndexOf(stencilRenderer) == index, "Index Mismatch!");

            // Update Culling Groups.
            foreach (CullingGroup cullingGroup in m_CullingGroups.Values)
            {
                cullingGroup.SetBoundingSphereCount(m_LOSStencilRenderers.Count);
            }
        }

        // Removes LOS Stencil Renderer and updates Culling Groups
        public void RemoveLOSStencilRenderer(LOSStencilRenderer stencilRenderer)
        {
            Debug.Assert(m_LOSStencilRenderers.Contains(stencilRenderer), "LOSStencilRenderer not found in list, can't remove");

            // Invoke Stencil Renderer removed event, before actually removing the Renderer.
            if (StencilRendererRemoved != null)
            {
                StencilRendererRemoved(stencilRenderer);
            }

            int index = GetIndexOf(stencilRenderer);
            int lastIndex = m_LOSStencilRenderers.Count - 1;

            // Move the reference at the end of the list to the removed objects index.
            m_LOSStencilRenderers[index] = m_LOSStencilRenderers[lastIndex];
            m_BoundingSpheres[index] = m_BoundingSpheres[lastIndex];

            // Remove the reference at the end of the list.
            m_LOSStencilRenderers.RemoveAt(lastIndex);

            Debug.Assert(!m_LOSStencilRenderers.Contains(stencilRenderer), "Remove failed");

            // Update Culling Groups.
            foreach (CullingGroup cullingGroup in m_CullingGroups.Values)
            {
                cullingGroup.EraseSwapBack(index);
                cullingGroup.SetBoundingSphereCount(m_LOSStencilRenderers.Count);
            }
        }

        /// <summary>
        /// Adds Culling Group for specified Camera
        /// </summary>
        public CullingGroup AddCullingGroup(Camera targetCamera)
        {
            if (!m_CullingGroups.ContainsKey(targetCamera))
            {
                CullingGroup cullingGroup = new CullingGroup();
                cullingGroup.targetCamera = targetCamera;

                cullingGroup.SetBoundingSpheres(m_BoundingSpheres);
                cullingGroup.SetBoundingSphereCount(m_LOSStencilRenderers.Count);

                m_CullingGroups.Add(targetCamera, cullingGroup);
            }

            return m_CullingGroups[targetCamera];
        }

        /// <summary>
        /// Removes Culling Group for specified Camera
        /// </summary>
        public void RemoveCullingGroup(Camera targetCamera)
        {
            if (m_CullingGroups.ContainsKey(targetCamera))
            {
                CullingGroup cullingGroup = m_CullingGroups[targetCamera];

                cullingGroup.onStateChanged = null;
                cullingGroup.Dispose();

                m_CullingGroups.Remove(targetCamera);
            }
        }

        /// <summary>
        /// Updates the Bounding Spheres of all Stencil Renderers whose Transform has changed
        /// </summary>
        public void UpdateBoundingSpheres()
        {
            for (int i = 0; i < m_LOSStencilRenderers.Count; ++i)
            {
                LOSStencilRenderer stencilRenderer = m_LOSStencilRenderers[i];

                if (!stencilRenderer.IsStatic && stencilRenderer.transform.hasChanged)
                {
                    m_BoundingSpheres[i] = stencilRenderer.RendererBoundingSphere;
                    stencilRenderer.transform.hasChanged = false;
                }
            }
        }

        /// <summary>
        /// Returns if Stencil Renderer is visible to Camera
        /// </summary>
        public bool IsLOSStencilRendererVisible(LOSStencilRenderer stencilRenderer, Camera targetCamera)
        {
            bool isVisible = false;

            CullingGroup cullingGroup = GetCullingGroup(targetCamera);
            int rendererIndex = GetIndexOf(stencilRenderer);

            if (cullingGroup != null && rendererIndex >= 0)
            {
                isVisible = cullingGroup.IsVisible(rendererIndex);
            }

            return isVisible;
        }

        #endregion Public Functions

        #region Private Functions

        /// <summary>
        /// Doubles the capacity of the Bounding Sphere Array
        /// </summary>
        private void IncreaseBoundingSphereArrayCapacity()
        {
            // Create Array with larger capacity
            int currentArrayLength = m_BoundingSpheres.Length;
            BoundingSphere[] newBoundingSphereArray = new BoundingSphere[currentArrayLength * 2];

            // Copy content to new Array.
            for (int i = 0; i < currentArrayLength; ++i)
            {
                newBoundingSphereArray[i] = m_BoundingSpheres[i];
            }

            // Swap Array references.
            m_BoundingSpheres = newBoundingSphereArray;

            // Update Culling Groups.
            foreach (CullingGroup cullingGroup in m_CullingGroups.Values)
            {
                cullingGroup.SetBoundingSpheres(m_BoundingSpheres);
                cullingGroup.SetBoundingSphereCount(m_LOSStencilRenderers.Count);
            }
        }

        private CullingGroup GetCullingGroup(Camera targetCamera)
        {
            CullingGroup cullingGroup;

            m_CullingGroups.TryGetValue(targetCamera, out cullingGroup);

            Debug.Assert(cullingGroup != null, "Failed to get Culling Group");

            return cullingGroup;
        }

        private int GetIndexOf(LOSStencilRenderer stencilRenderer)
        {
            int index = m_LOSStencilRenderers.IndexOf(stencilRenderer);

            Debug.Assert(index >= 0, "Failed to get a valid index for LOS Stencil Renderer.");

            return index;
        }

        #endregion Private Functions
    }
}