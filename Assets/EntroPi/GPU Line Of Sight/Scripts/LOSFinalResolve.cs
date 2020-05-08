using UnityEngine;

namespace LOS
{
    // Fixes an issue with multiple cameras, image effects and deferred rendering
    // NOTE: The script must be the last in the image effect chain, so order it in the inspector!
    // Check http://forum.unity3d.com/threads/deferred-renderer-multiple-cameras-posts-ssao-bugged-ufps-onrenderimage.198632/ for more info

    [ExecuteInEditMode]
    [AddComponentMenu("Line of Sight/Deprecated/LOS Final Resolve")]
    public class LOSFinalResolve : MonoBehaviour
    {
        private RenderTexture m_ActiveRT;

        private void OnEnable()
        {
            Debug.LogWarning("The LOS Final Resolve script component is obsolete, you can safely remove this script.");
        }

        private void OnPreRender()
        {
            if (GetComponent<Camera>().actualRenderingPath == RenderingPath.DeferredShading)
            {
                m_ActiveRT = RenderTexture.active;
            }
            else
            {
                m_ActiveRT = null;
            }
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (GetComponent<Camera>().actualRenderingPath == RenderingPath.DeferredShading && m_ActiveRT && src != m_ActiveRT)
            {
                if (src.format == m_ActiveRT.format)
                {
                    // Copy source to active Render texture.
                    Graphics.Blit(src, m_ActiveRT);
                    // In case we're not the last image effect.
                    Graphics.Blit(m_ActiveRT, dest);
                }
                else
                {
                    Debug.LogWarning("Cant resolve texture, because of different formats!");
                }
            }
            else
            {
                // Script must be last anyway, so we don't need a final copy?
                // Just in case we are not last!
                Graphics.Blit(src, dest);
            }
        }
    }
}