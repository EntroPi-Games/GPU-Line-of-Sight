using UnityEngine;
using System.Collections;

namespace LOS
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(LOSMask))]
    public class LOSDebugView : MonoBehaviour
    {
        private enum Mode { None, CameraNormals, CameraDepth, WorldPosition }

        private Camera m_Camera;
        private Mode m_Mode = Mode.None;

        private void OnEnable()
        {
            m_Camera = GetComponent<Camera>();
            enabled &= Util.Verify(m_Camera != null, "Failed to get Camera component");
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (m_Camera == null) return;

            switch (m_Mode)
            {
                case Mode.CameraNormals:
                    RenderCameraNormals(source, destination);
                    break;

                case Mode.CameraDepth:
                    RenderCameraDepth(source, destination);
                    break;

                case Mode.WorldPosition:
                    RenderWorldPosition(source, destination);
                    break;

                default:
                    Graphics.Blit(source, destination);
                    break;
            }
        }

        private void RenderCameraNormals(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, Materials.Debug, 0);
        }

        private void RenderCameraDepth(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, Materials.Debug, 1);
        }

        private void RenderWorldPosition(RenderTexture source, RenderTexture destination)
        {
            // Calculate Frustum origins and rays for mask camera.
            Matrix4x4 frustumOrigins;
            Matrix4x4 frustumRays;
            LOSHelper.CalculateViewVectors(m_Camera, out frustumRays, out frustumOrigins);

            // Push parameters which are identical for all LOS sources.
            Materials.Debug.SetMatrix(ShaderID.FrustumRays, frustumRays);
            Materials.Debug.SetMatrix(ShaderID.FrustumOrigins, frustumOrigins);

            Materials.Debug.SetPass(2);

            LOSMask.IndexedGraphicsBlit(destination);
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Normal"))
            {
                m_Mode = Mode.None;
            }
            if (GUILayout.Button("Camera Normals"))
            {
                m_Mode = Mode.CameraNormals;
            }
            if (GUILayout.Button("Camera Depth"))
            {
                m_Mode = Mode.CameraDepth;
            }
            if (GUILayout.Button("World Position"))
            {
                m_Mode = Mode.WorldPosition;
            }
        }
    }
}