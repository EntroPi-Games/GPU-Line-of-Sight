using UnityEngine;

namespace LOS
{
    /// <summary>
    /// Interface used for LOSSource class
    /// </summary>
    public interface ILOSSource
    {
        Color MaskColor
        {
            get;
            set;
        }

        float MaskIntensity
        {
            get;
            set;
        }

        bool MaskInvert
        {
            get;
            set;
        }

        float DistanceFade
        {
            get;
            set;
        }

        float MinVariance
        {
            get;
            set;
        }

        bool IsVisible
        {
            get;
        }

        Camera SourceCamera
        {
            get;
        }

        Bounds CameraBounds
        {
            get;
        }

        GameObject GameObject
        {
            get;
        }

        Vector4 SourceInfo
        {
            get;
        }
    }
}