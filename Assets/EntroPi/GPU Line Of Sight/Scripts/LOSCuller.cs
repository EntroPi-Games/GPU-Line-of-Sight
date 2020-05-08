using System.Collections.Generic;
using UnityEngine;

namespace LOS
{
    [AddComponentMenu("Line of Sight/LOS Culler")]
    public class LOSCuller : MonoBehaviour
    {
        #region Exposed Data Members

        [Tooltip("Selects which layers block raycasts used for visibility calculations")]
        [SerializeField]
        private LayerMask m_RaycastLayerMask = -1;

        #endregion Exposed Data Members

        #region Private Data Members

        private bool m_IsVisible = true;

        #endregion Private Data Members

        #region Public Properties

        public LayerMask RaycastLayerMask
        {
            get { return m_RaycastLayerMask; }
            set { m_RaycastLayerMask = value; }
        }

        public bool Visibile
        {
            get { return m_IsVisible; }
        }

        #endregion Public Properties

        #region MonoBehaviour Functions

        private void OnEnable()
        {
            enabled &= Util.Verify(GetComponent<Renderer>() != null, "No renderer attached to this GameObject! LOS Culler component must be added to a GameObject containing a MeshRenderer or Skinned Mesh Renderer!");
        }

        private void Update()
        {
            m_IsVisible = CustomCull(gameObject.GetComponent<Renderer>().bounds, m_RaycastLayerMask.value);
        }

        #endregion MonoBehaviour Functions

        #region Private Functions

        /// <summary>
        /// Checks to see if object is inside the view frustum of any of the LOS cameras.
        /// Ideally should be called in OnWillRenderObject, but it's to late to disable renderer..
        /// Early outs when visible to one camera.
        /// </summary>
        private static bool CustomCull(Bounds meshBounds, int layerMask)
        {
            // Get list of sources.
            List<LOSSource> losSources = LOSManager.Instance.LOSSources;

            for (int i = 0; i < losSources.Count; ++i)
            {
                LOSSource losSource = losSources[i];
                if (LOSHelper.CheckBoundsVisibility(losSource, meshBounds, layerMask))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion Private Functions
    }
}