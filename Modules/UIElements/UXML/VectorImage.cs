// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;

namespace UnityEngine.UIElements
{
    internal enum GradientType
    {
        Linear,
        Radial
    }

    internal enum AddressMode
    {
        Wrap,
        Clamp,
        Mirror
    }

    [Serializable]
    internal struct VectorImageVertex
    {
        public Vector3 position;
        public Color32 tint;
        public Vector2 uv;
        public UInt32 settingIndex;
    }

    [Serializable]
    internal struct GradientSettings
    {
        public GradientType gradientType;
        public AddressMode addressMode;
        public Vector2 radialFocus;
        public RectInt location;
    }

    [Serializable]
    public class VectorImage : ScriptableObject
    {
        [SerializeField] internal Texture2D atlas = null;
        [SerializeField] internal VectorImageVertex[] vertices = null;
        [SerializeField] internal UInt16[] indices = null;
        [SerializeField] internal GradientSettings[] settings = null;
        [SerializeField] internal Vector2 size = Vector2.zero;
    }
}
