using UnityEngine;

namespace LOS
{
    /// <summary>
    /// Enables a gameobjects renderer when the object becomes visibile
    /// </summary>
    [RequireComponent(typeof(LOS.LOSCuller))]
    [AddComponentMenu("Line of Sight/LOS Object Revealer")]
    public class LOSObjectRevealer : MonoBehaviour
    {
        private LOSCuller m_Culler;
        private bool m_Revealed = false;

        private void Awake()
        {
            m_Culler = GetComponent<LOSCuller>();
        }

        private void OnEnable()
        {
            enabled &= Util.Verify(m_Culler != null, "Failed to get LOS culler component!");
            enabled &= Util.Verify(GetComponent<Renderer>() != null, "No renderer attached to this GameObject! LOS Culler component must be added to a GameObject containing a MeshRenderer or Skinned Mesh Renderer!");

            if (enabled)
                GetComponent<Renderer>().enabled = false;
        }

        private void LateUpdate()
        {
            if (!m_Revealed)
            {
                if (m_Culler.Visibile)
                {
                    m_Revealed = true;
                    GetComponent<Renderer>().enabled = true;

                    //Disable LOS Culler, we don't need it to check anymore
                    m_Culler.enabled = false;
                }
            }
        }
    }
}