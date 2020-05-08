using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LOS.LOSVisibilityInfo))]
public class PlayerInfo : MonoBehaviour
{
    public List<string> VisibleSources
    {
        get { return m_VisibleSources; }
    }

    private LOS.LOSVisibilityInfo m_VisibilityInfo;
    private List<string> m_VisibleSources = new List<string>();

    private void OnEnable()
    {
        m_VisibilityInfo = GetComponent<LOS.LOSVisibilityInfo>();
    }

    private void Update()
    {
        m_VisibleSources.Clear();

        foreach (LOS.ILOSSource losSource in m_VisibilityInfo.VisibleSources)
        {
            // Only add LOS ources that aren't attached to the player
            if (losSource.GameObject.tag != "Player")
            {
                m_VisibleSources.Add(losSource.GameObject.name);
            }
        }
    }
}