using UnityEngine;

namespace LOS
{
    //Stores the screen buffer
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Line of Sight/LOS Buffer Storage")]
    public class LOSBufferStorage : MonoBehaviour
    {
        private RenderTexture m_Buffer;

        public RenderTexture BufferTexture
        {
            get { return m_Buffer; }
        }

        private void OnEnable()
        {
            // Check if this component can be enabled.
            enabled &= Util.Verify(SystemInfo.supportsImageEffects, "System does not support image effects");
        }

        private void OnDisable()
        {
            DestroyBuffer();
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // Create the Buffer texture
            if (m_Buffer == null || m_Buffer.width != source.width || m_Buffer.height != source.height || m_Buffer.depth != source.depth)
            {
                DestroyBuffer();

                m_Buffer = new RenderTexture(source.width, source.height, source.depth);
                m_Buffer.hideFlags = HideFlags.HideAndDontSave;
            }

            //Store Buffer
            Graphics.Blit(source, m_Buffer);

            //Copy source to destination
            Graphics.Blit(source, destination);
        }

        private void DestroyBuffer()
        {
            if (m_Buffer != null)
            {
                DestroyImmediate(m_Buffer);
                m_Buffer = null;
            }
        }
    }
}