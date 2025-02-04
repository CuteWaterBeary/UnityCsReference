// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Utils;
using UnityEditor.PackageManager;

namespace UnityEditor
{
    internal class BuildEventsHandlerPostProcess : IPostprocessBuildWithReport
    {
        [Serializable]
        internal struct SceneViewInfo
        {
            public int total_scene_views;
            public int num_of_2d_views;
            public bool is_default_2d_mode;
        }

        [Serializable]
        internal struct BuildPackageIds
        {
            public string[] package_ids;
        }

        [Serializable]
        internal struct AndroidBuildFeature
        {
            public string name;
            public bool required;
        }

        [Serializable]
        internal struct AndroidBuildPermissions
        {
            public AndroidBuildFeature[] features;
            public string[] permissions;
        }

        private static bool s_EventSent = false;
        private static int s_NumOfSceneViews = 0;
        private static int s_NumOf2dSceneViews = 0;
        private const string s_GradlePath = "Temp/gradleOut/build/intermediates/merged_manifests";
        private const string s_StagingArea = "Temp/StagingArea";
        private const string s_AndroidManifest = "AndroidManifest.xml";

        public int callbackOrder {get { return 0; }}
        public void OnPostprocessBuild(BuildReport report)
        {
            ReportSceneViewInfo();
            ReportBuildPackageIds(report.GetFiles());
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                ReportBuildTargetPermissions(report.summary.options);
            }
        }

        private string SanitizePackageId(UnityEditor.PackageManager.PackageInfo packageInfo)
        {
            if (packageInfo.source == UnityEditor.PackageManager.PackageSource.Registry)
                return packageInfo.packageId;
            return packageInfo.name + "@" + Enum.GetName(typeof(UnityEditor.PackageManager.PackageSource), packageInfo.source).ToLower();
        }

        private void ReportBuildPackageIds(BuildFile[] buildFiles)
        {
            List<string> managedLibraries = new List<string>();
            foreach (BuildFile file in buildFiles)
            {
                if (file.role == "ManagedLibrary" || file.role == "dll")
                    managedLibraries.Add(file.path);
            }

            var matchingPackages = UnityEditor.PackageManager.PackageInfo.GetForAssemblyFilePaths(managedLibraries);
            var packageIds = matchingPackages.Select(item => SanitizePackageId(item)).ToArray();
            if (packageIds.Length > 0)
                EditorAnalytics.SendEventBuildPackageList(new BuildPackageIds() { package_ids = packageIds });
        }

        private void ReportSceneViewInfo()
        {
            Object[] views = Resources.FindObjectsOfTypeAll(typeof(SceneView));
            int numOf2dSceneViews = 0;
            foreach (SceneView view in views)
            {
                if (view.in2DMode)
                    numOf2dSceneViews++;
            }
            if ((s_NumOfSceneViews != views.Length) || (s_NumOf2dSceneViews != numOf2dSceneViews) || !s_EventSent)
            {
                s_EventSent = true;
                s_NumOfSceneViews = views.Length;
                s_NumOf2dSceneViews = numOf2dSceneViews;
                EditorAnalytics.SendEventSceneViewInfo(new SceneViewInfo()
                {
                    total_scene_views = s_NumOfSceneViews, num_of_2d_views = s_NumOf2dSceneViews,
                    is_default_2d_mode = EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode2D
                });
            }
        }

        internal static string GetMergedManifestPath(BuildOptions buildOptions)
        {
            string manifestFilePath = Path.Combine(s_StagingArea, s_AndroidManifest);
            if (EditorUserBuildSettings.androidBuildSystem == AndroidBuildSystem.Gradle)
            {
                var path = $"Library/Bee/Android/Prj/{PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android)}/Gradle/launcher/build/intermediates/merged_manifests";
                manifestFilePath = (buildOptions & BuildOptions.Development) == 0
                    ? Paths.Combine(path, "release", s_AndroidManifest)
                    : Paths.Combine(path, "debug", s_AndroidManifest);
            }
            return manifestFilePath;
        }

        private void ReportBuildTargetPermissions(BuildOptions buildOptions)
        {
            List<string> permissionsList = new List<string>();
            List<AndroidBuildFeature> featuresList = new List<AndroidBuildFeature>();
            string manifestFilePath = GetMergedManifestPath(buildOptions);

            XmlDocument manifestFile = new XmlDocument();
            if (File.Exists(manifestFilePath))
            {
                manifestFile.Load(manifestFilePath);
                XmlNodeList permissions = manifestFile.GetElementsByTagName("uses-permission");
                XmlNodeList permissionsSdk23 = manifestFile.GetElementsByTagName("uses-permission-sdk-23");
                XmlNodeList features = manifestFile.GetElementsByTagName("uses-feature");
                foreach (XmlNode permission in permissions)
                {
                    XmlNode attribute = permission.Attributes ? ["android:name"];
                    if (attribute != null)
                        permissionsList.Add(attribute.Value);
                }
                if (permissionsSdk23 != null)
                {
                    foreach (XmlNode permission in permissionsSdk23)
                    {
                        XmlNode attribute = permission.Attributes?["android:name"];
                        if (attribute != null)
                            permissionsList.Add(attribute.Value);
                    }
                }

                foreach (XmlNode feature in features)
                {
                    XmlNode attribute = feature.Attributes ? ["android:name"];
                    if (attribute != null)
                    {
                        if (!bool.TryParse(feature.Attributes?["android:required"]?.Value, out bool featureRequired))
                        {
                            featureRequired = true;
                        }
                        featuresList.Add(new BuildEventsHandlerPostProcess.AndroidBuildFeature() { name = attribute.Value, required = featureRequired });
                    }
                }

                EditorAnalytics.SendEventBuildTargetPermissions(new AndroidBuildPermissions()
                {
                    features = featuresList.ToArray(),
                    permissions = permissionsList.ToArray()
                });
            }
        }
    }

    internal class BuildCompletionEventsHandler
    {
        [Serializable]
        struct BuildLibrariesInfo
        {
            public bool ar_plugin_loaded;
            public string[] build_libraries;
        }

        public static void ReportPostBuildCompletionInfo(List<string> libraries)
        {
            if (libraries != null)
            {
                EditorAnalytics.SendEventBuildFrameworkList(new BuildLibrariesInfo()
                {
                    ar_plugin_loaded = libraries.Contains("System/Library/Frameworks/ARKit.framework"),
                    build_libraries = libraries.ToArray()
                });
            }
        }
    }
} // namespace
