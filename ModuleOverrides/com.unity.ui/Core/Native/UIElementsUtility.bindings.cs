// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Collections.Generic;
using UnityEngine.Bindings;
using UnityEngine.Scripting;
using Unity.Profiling;

namespace UnityEngine.UIElements
{
    // This is the required interface to UIElementsUtility for Runtime game components.
    [NativeHeader("ModuleOverrides/com.unity.ui/Core/Native/UIElementsRuntimeUtilityNative.h")]
    [VisibleToOtherModules("Unity.UIElements")]
    internal static class UIElementsRuntimeUtilityNative
    {
        internal static Action RepaintOverlayPanelsCallback;
        internal static Action UpdateRuntimePanelsCallback;
        internal static Action RepaintOffscreenPanelsCallback;

        [RequiredByNativeCode]
        public static void RepaintOverlayPanels()
        {
            RepaintOverlayPanelsCallback?.Invoke();
        }

        [RequiredByNativeCode]
        public static void UpdateRuntimePanels()
        {
            UpdateRuntimePanelsCallback?.Invoke();
        }

        [RequiredByNativeCode]
        public static void RepaintOffscreenPanels()
        {
            RepaintOffscreenPanelsCallback?.Invoke();
        }

        public extern static void RegisterPlayerloopCallback();
        public extern static void UnregisterPlayerloopCallback();

        public extern static void VisualElementCreation();
    }
}
