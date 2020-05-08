using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class UIInfo : MonoBehaviour
{
    private Text m_Text;
    private PlayerInfo m_PlayerInfo;

    private void OnEnable()
    {
        m_Text = GetComponent<Text>();

        m_PlayerInfo = GameObject.FindObjectOfType<PlayerInfo>();
    }

    private void LateUpdate()
    {
        if (m_PlayerInfo != null)
        {
            if (m_PlayerInfo.VisibleSources.Count > 0)
            {
                // Clear text
                m_Text.text = "";

                foreach (string sourceName in m_PlayerInfo.VisibleSources)
                {
                    m_Text.text += "Player has been spotted by " + sourceName + "\n";
                }

                m_Text.color = Color.red;
            }
            else
            {
                m_Text.text = "Player has not been spotted";
                m_Text.color = Color.white;
            }
        }
    }
}