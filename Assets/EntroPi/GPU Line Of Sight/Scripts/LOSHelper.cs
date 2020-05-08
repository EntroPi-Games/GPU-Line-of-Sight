using UnityEngine;

namespace LOS
{
    //Collection of static helper functions
    public static class LOSHelper
    {
        // Used for calculating orthographic frustum corners.
        private static readonly Vector4[] m_NearHomogenousCorners = new Vector4[4]
        {
            new Vector4(-1.0f, 1.0f, -1.0f, 1.0f),
            new Vector4(1.0f, 1.0f, -1.0f, 1.0f),
            new Vector4(1.0f, -1.0f, -1.0f, 1.0f),
            new Vector4(-1.0f, -1.0f, -1.0f, 1.0f)
        };

        private static readonly Vector4[] m_FarHomogenousCorners = new Vector4[4]
        {
            new Vector4(-1.0f, 1.0f, 1.0f, 1.0f),
            new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
            new Vector4(1.0f, -1.0f, 1.0f, 1.0f),
            new Vector4(-1.0f, -1.0f, 1.0f, 1.0f)
        };

        /// <summary>
        /// Same functionality as GeometryUtility.CalculateFrustumPlanes, but doesn't allocate memory.
        /// </summary>
        public static void ExtractFrustumPlanes(Plane[] planes, Camera camera)
        {
            Matrix4x4 viewProjMatrix = camera.projectionMatrix * camera.worldToCameraMatrix;

            Vector3 normal = new Vector3(viewProjMatrix[3, 0] + viewProjMatrix[0, 0], viewProjMatrix[3, 1] + viewProjMatrix[0, 1], viewProjMatrix[3, 2] + viewProjMatrix[0, 2]);
            float length = normal.magnitude;
            planes[0].normal = normal.normalized;
            planes[0].distance = (viewProjMatrix[3, 3] + viewProjMatrix[0, 3]) / length;

            normal = new Vector3(viewProjMatrix[3, 0] - viewProjMatrix[0, 0], viewProjMatrix[3, 1] - viewProjMatrix[0, 1], viewProjMatrix[3, 2] - viewProjMatrix[0, 2]);
            length = normal.magnitude;
            planes[1].normal = normal.normalized;
            planes[1].distance = (viewProjMatrix[3, 3] - viewProjMatrix[0, 3]) / length;

            normal = new Vector3(viewProjMatrix[3, 0] + viewProjMatrix[1, 0], viewProjMatrix[3, 1] + viewProjMatrix[1, 1], viewProjMatrix[3, 2] + viewProjMatrix[1, 2]);
            length = normal.magnitude;
            planes[2].normal = normal.normalized;
            planes[2].distance = (viewProjMatrix[3, 3] + viewProjMatrix[1, 3]) / length;

            normal = new Vector3(viewProjMatrix[3, 0] - viewProjMatrix[1, 0], viewProjMatrix[3, 1] - viewProjMatrix[1, 1], viewProjMatrix[3, 2] - viewProjMatrix[1, 2]);
            length = normal.magnitude;
            planes[3].normal = normal.normalized;
            planes[3].distance = (viewProjMatrix[3, 3] - viewProjMatrix[1, 3]) / length;

            normal = new Vector3(viewProjMatrix[3, 0] + viewProjMatrix[2, 0], viewProjMatrix[3, 1] + viewProjMatrix[2, 1], viewProjMatrix[3, 2] + viewProjMatrix[2, 2]);
            length = normal.magnitude;
            planes[4].normal = normal.normalized;
            planes[4].distance = (viewProjMatrix[3, 3] + viewProjMatrix[2, 3]) / length;

            normal = new Vector3(viewProjMatrix[3, 0] - viewProjMatrix[2, 0], viewProjMatrix[3, 1] - viewProjMatrix[2, 1], viewProjMatrix[3, 2] - viewProjMatrix[2, 2]);
            length = normal.magnitude;
            planes[5].normal = normal.normalized;
            planes[5].distance = (viewProjMatrix[3, 3] - viewProjMatrix[2, 3]) / length;
        }

        /// <summary>
        /// Calculates frustum corners for perspective projection in world space.
        /// </summary>
        public static Matrix4x4 CalculatePerspectiveCorners(Camera currentCamera)
        {
            float cameraFar = currentCamera.farClipPlane;
            float cameraFOV = currentCamera.fieldOfView;
            float cameraRatio = currentCamera.aspect;

            Matrix4x4 frustumCorners = Matrix4x4.zero;

            float fovWHalf = cameraFOV * 0.5f * Mathf.Deg2Rad;
            float Hfar = Mathf.Tan(fovWHalf) * cameraFar;
            float Wfar = Mathf.Tan(fovWHalf) * cameraRatio * cameraFar;

            Vector3 frustumFarUp = currentCamera.transform.up * Hfar;
            Vector3 frustumFarRight = currentCamera.transform.right * Wfar;
            Vector3 frustumCenter = currentCamera.transform.forward * cameraFar;

            Vector3 topLeft = frustumCenter + (frustumFarUp) - (frustumFarRight);
            Vector3 topRight = frustumCenter + (frustumFarUp) + (frustumFarRight);
            Vector3 bottomRight = frustumCenter - (frustumFarUp) + (frustumFarRight);
            Vector3 bottomLeft = frustumCenter - (frustumFarUp) - (frustumFarRight);

            frustumCorners.SetRow(0, topLeft);
            frustumCorners.SetRow(1, topRight);
            frustumCorners.SetRow(2, bottomRight);
            frustumCorners.SetRow(3, bottomLeft);

            return frustumCorners;
        }

        /// <summary>
        /// Calculates camera origins and rays for world space reconstruction using depth in the LOS mask shader.
        /// </summary>
        public static void CalculateViewVectors(Camera currentCamera, out Matrix4x4 rays, out Matrix4x4 origins)
        {
            Matrix4x4 inverseProjection = currentCamera.projectionMatrix.inverse;
            rays = Matrix4x4.zero;
            origins = Matrix4x4.zero;

            for (int i = 0; i < 4; ++i)
            {
                // Unproject the far and near frustum corners from NDC to view space.
                Vector4 nearPos = inverseProjection * m_NearHomogenousCorners[i];
                Vector4 farPos = inverseProjection * m_FarHomogenousCorners[i];
                nearPos /= nearPos.w;
                farPos /= farPos.w;

                Vector4 ray = farPos - nearPos;
                ray /= ray.z;

                Vector4 origin = nearPos - ray * nearPos.z;
                origin.w = 1;
                origin = currentCamera.cameraToWorldMatrix * origin;

                // Invert ray.
                ray *= -1;
                ray.w = 0;
                ray = currentCamera.cameraToWorldMatrix * ray;

                rays.SetRow(i, ray);
                origins.SetRow(i, origin);
            }
        }

        /// <summary>
        /// Calculates AABB for source camera.
        /// </summary>
        public static Bounds CalculateSourceBounds(Camera sourceCamera)
        {
            Matrix4x4 frustumCorners = LOSHelper.CalculatePerspectiveCorners(sourceCamera);
            Vector3 cameraPosition = sourceCamera.transform.position;

            Vector3 frustumMin = GetFrustumMin(frustumCorners);
            frustumMin = Vector3.Min(frustumMin + cameraPosition, cameraPosition);

            Vector3 frustumMax = GetFrustumMax(frustumCorners);
            frustumMax = Vector3.Max(frustumMax + cameraPosition, cameraPosition);

            Bounds cameraBound = new Bounds();
            cameraBound.SetMinMax(frustumMin, frustumMax);

            return cameraBound;
        }

        /// <summary>
        /// Calculates the min point of the frustum bound.
        /// </summary>
        private static Vector3 GetFrustumMin(Matrix4x4 frustumCorners)
        {
            Vector3 frustumMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

            for (int i = 0; i < 4; ++i)
            {
                frustumMin = Vector3.Min(frustumMin, frustumCorners.GetRow(i));
            }

            return frustumMin;
        }

        /// <summary>
        /// Calculates the max point of the frustum bound.
        /// </summary>
        private static Vector3 GetFrustumMax(Matrix4x4 frustumCorners)
        {
            Vector3 frustumMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int i = 0; i < 4; ++i)
            {
                frustumMax = Vector3.Max(frustumMax, frustumCorners.GetRow(i));
            }

            return frustumMax;
        }

        /// <summary>
        /// Returns nearest power of two.
        /// </summary>
        public static int NearestPowerOfTwo(int value)
        {
            return (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(value) / Mathf.Log(2)));
        }

        /// <summary>
        /// Initiliaze camera for LOS source and LOS source cube components.
        /// </summary>
        public static void InitSourceCamera(Camera sourceCamera)
        {
            // Disable camera so it will only render on request.
            sourceCamera.enabled = false;

            // Set-up camera.
            sourceCamera.orthographic = false;
            sourceCamera.renderingPath = RenderingPath.VertexLit;
            sourceCamera.clearFlags = CameraClearFlags.Skybox;
            sourceCamera.hdr = false;

            // Disable depth texture just to make sure.
            sourceCamera.depthTextureMode = DepthTextureMode.None;
        }

        /// <summary>
        /// Checks if the mesh bounds are visible to the LOS Source
        /// </summary>
        public static bool CheckBoundsVisibility(LOSSource losSource, Bounds meshBounds, int layerMask)
        {
            Camera currentCamera = losSource.SourceCamera;

            if (losSource.IsVisible && currentCamera != null)
            {
                Plane[] cameraPlanes = losSource.FrustumPlanes;

                if (GeometryUtility.TestPlanesAABB(cameraPlanes, meshBounds))
                {
                    if (LOSHelper.CheckRayCast(currentCamera, meshBounds, losSource.SourceInfo.w, layerMask))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks to see if the view from the camera to the object is not obstructed by other objects with colliders.
        /// Early outs when one ray connects.
        /// </summary>
        private static bool CheckRayCast(Camera currentCamera, Bounds meshBounds, float maxDistance, int layerMask)
        {
            bool rayConnect = false;
            Vector3 cameraPosition = currentCamera.transform.position;

            //Written like this to avoid memory allocation
            if (CheckRayConnect(meshBounds.center, cameraPosition, maxDistance, layerMask) || CheckRayConnect(meshBounds.max, cameraPosition, maxDistance, layerMask) || CheckRayConnect(meshBounds.min, cameraPosition, maxDistance, layerMask))
                rayConnect = true;

            return rayConnect;
        }

        /// <summary>
        /// Checks if ray is blocked or connects from one point to the other.
        /// </summary>
        private static bool CheckRayConnect(Vector3 origin, Vector3 cameraPosition, float maxDistance, int layerMask)
        {
            const float RAY_THRESHOLD = 0.1f;
            const float MIN_RAY_LENGTH = 0.00001f;

            bool rayConnect = false;

            Vector3 rayDirection = origin - cameraPosition;

            if (rayDirection.sqrMagnitude < maxDistance * maxDistance)
            {
                Ray visibleRay = new Ray(cameraPosition, rayDirection);

                float rayLength = rayDirection.magnitude - RAY_THRESHOLD;
                rayLength = Mathf.Max(MIN_RAY_LENGTH, rayLength);

                if (!Physics.Raycast(visibleRay, rayLength, layerMask))
                    rayConnect = true;
            }

            return rayConnect;
        }
    }
}