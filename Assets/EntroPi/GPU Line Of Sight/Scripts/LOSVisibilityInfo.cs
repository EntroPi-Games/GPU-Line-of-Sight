using System.Collections.Generic;
using UnityEngine;

namespace LOS
{
    [AddComponentMenu("Line of Sight/LOS Visibility Info")]
    public class LOSVisibilityInfo : MonoBehaviour
    {
        #region Exposed Data Members

        [Tooltip("Selects which layers block raycasts used for visibility calculations")]
        [SerializeField]
        private LayerMask m_RaycastLayerMask = -1;

        #endregion Exposed Data Members

        #region Private Data Members

        private List<ILOSSource> m_VisibleSources = new List<ILOSSource>();

        #endregion Private Data Members

        #region Public Properties

        public List<ILOSSource> VisibleSources
        {
            get { return m_VisibleSources; }
        }

        public LayerMask RaycastLayerMask
        {
            get { return m_RaycastLayerMask; }
            set { m_RaycastLayerMask = value; }
        }

        public bool Visibile
        {
            get { return m_VisibleSources.Count > 0; }
        }

        #endregion Public Properties

        #region Delegates

        public delegate void OnLineOfSightEnterHandler(GameObject sender, ILOSSource losSource);

        public delegate void OnLineOfSightStayHandler(GameObject sender, ILOSSource losSource);

        public delegate void OnLineOfSightExitHandler(GameObject sender, ILOSSource losSource);

        #endregion Delegates

        #region Events

        public event OnLineOfSightEnterHandler OnLineOfSightEnter;

        public event OnLineOfSightStayHandler OnLineOfSightStay;

        public event OnLineOfSightExitHandler OnLineOfSightExit;

        #endregion Events

        #region MonoBehaviour Functions

        private void OnEnable()
        {
            enabled &= Util.Verify(GetComponent<Renderer>() != null, "No renderer attached to this GameObject! LOS Culler component must be added to a GameObject containing a MeshRenderer or Skinned Mesh Renderer!");
        }

        private void Update()
        {
            UpdateVisibleSources();
        }

        #endregion MonoBehaviour Functions

        #region Private Functions

        /// <summary>
        /// Checks to see if object is inside the view frustum of any of the LOS cameras.
        /// Ideally should be called in OnWillRenderObject, but it's to late to disable renderer..
        /// Early outs when visible to one camera.
        /// </summary>
        private void UpdateVisibleSources()
        {
            Bounds meshBounds = gameObject.GetComponent<Renderer>().bounds;

            // Get list of sources.
            List<LOSSource> losSources = LOSManager.Instance.LOSSources;

            for (int i = 0; i < losSources.Count; ++i)
            {
                LOSSource losSource = losSources[i];

                bool isVisible = LOSHelper.CheckBoundsVisibility(losSource, meshBounds, m_RaycastLayerMask.value);

                UpdateList(losSource, isVisible);
            }
        }

        /// <summary>
        /// Updates the list with visible sources and trigger events if needed
        /// </summary>
        private void UpdateList(ILOSSource losSource, bool isVisible)
        {
            if (isVisible)
            {
                // LOS Source is already in list.
                if (m_VisibleSources.Contains(losSource))
                {
                    InvokeOnLineOfSightStay(losSource);
                }
                // LOS Source is added to list.
                else
                {
                    m_VisibleSources.Add(losSource);
                    InvokeOnLineOfSightEnter(losSource);
                }
            }
            else
            {
                // Source is removed from list.
                if (m_VisibleSources.Contains(losSource))
                {
                    m_VisibleSources.Remove(losSource);
                    InvokeOnLineOfSightEXit(losSource);
                }
            }
        }

        #endregion Private Functions

        #region Event Invole Function

        private void InvokeOnLineOfSightEnter(ILOSSource losSource)
        {
            if (OnLineOfSightEnter != null)
            {
                OnLineOfSightEnter(gameObject, losSource);
            }
        }

        private void InvokeOnLineOfSightStay(ILOSSource losSource)
        {
            if (OnLineOfSightStay != null)
            {
                OnLineOfSightStay(gameObject, losSource);
            }
        }

        private void InvokeOnLineOfSightEXit(ILOSSource losSource)
        {
            if (OnLineOfSightExit != null)
            {
                OnLineOfSightExit(gameObject, losSource);
            }
        }

        #endregion Event Invole Function
    }
}