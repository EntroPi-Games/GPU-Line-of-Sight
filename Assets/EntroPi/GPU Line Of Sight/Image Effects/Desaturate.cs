using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Rendering/Desaturate")]
public class Desaturate : MonoBehaviour
{
    [SerializeField]
    private Shader m_Shader;

    [Range(0, 1f)]
    [SerializeField]
    private float m_Brightness = 1f;

    private Material m_Material;

    private void OnEnable()
    {
        // Disable if image effects aren't supported
        enabled &= SystemInfo.supportsImageEffects;

        // Disable the image effect if the m_Shader can't
        // run on the users graphics card
        if (!m_Shader || !m_Shader.isSupported)
            enabled = false;
    }

    private Material material
    {
        get
        {
            if (null == m_Material)
            {
                m_Material = new Material(m_Shader);
                m_Material.hideFlags = HideFlags.HideAndDontSave;
            }

            return m_Material;
        }
    }

    private void OnDisable()
    {
        if (m_Material)
            DestroyImmediate(m_Material);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        material.SetFloat("_Brightness", m_Brightness);
        Graphics.Blit(source, destination, material);
    }
}