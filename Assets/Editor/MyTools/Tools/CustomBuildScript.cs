using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Invariable;



namespace MyTools
{
    public class CustomBuildScript
    {
        [MenuItem("VastStarryRiver/打包/打包成APK文件", false, 30)]
        public static void PackageProject_Android()
        {
            SetAndroidKeystore();

            if (Directory.Exists(ConfigUtils.m_buildPath))
            {
                Directory.Delete(ConfigUtils.m_buildPath, true);
            }

            ConfigUtils.InitDirectory(ConfigUtils.m_buildPath);

            PackageProject(BuildTarget.Android, ConfigUtils.m_buildPath + "/SpectraAbyss.apk");
        }

        [MenuItem("VastStarryRiver/打包/复制文件到CDN目录", false, 31)]
        public static void MoveFileToCND()
        {
            if (Directory.Exists(ConfigUtils.m_cdnPath))
            {
                Directory.Delete(ConfigUtils.m_cdnPath, true);
            }

            ConfigUtils.InitDirectory(ConfigUtils.m_cdnPath);

            MoveBundleToCND();
        }



        private static void MoveBundleToCND()
        {
            string path = AssetBundleTool.GetOutPath();

            if (!Directory.Exists(path))
            {
                return;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            FileInfo[] fileInfos = directoryInfo.GetFiles();

            foreach (var item in fileInfos)
            {
                string sourceFilePath = item.FullName.Replace("\\", "/");
                string targetFilePath = ConfigUtils.m_cdnPath + "/" + Path.GetFileName(sourceFilePath);
                File.Copy(sourceFilePath, targetFilePath);
            }
        }

        private static void SetAndroidKeystore()
        {
            string keystorePath = ConfigUtils.m_keystorePath; // Keystore 文件路径

            // 确保 keystore 文件存在
            if (!File.Exists(keystorePath))
            {
                Debug.LogError("Keystore文件不存在: " + keystorePath);
                return;
            }

            // 设置 keystore 信息
            PlayerSettings.Android.keystoreName = keystorePath;
            PlayerSettings.Android.keystorePass = "149630764"; // Keystore 密码
            PlayerSettings.Android.keyaliasName = "spectraabyss"; // Alias 名称
            PlayerSettings.Android.keyaliasPass = "149630764"; // Alias 密码
        }

        private static void PackageProject(BuildTarget target, string locationPathName)
        {
            // 获取所有场景
            string[] scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);

            // 设置构建选项
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = locationPathName, // 打包的输出路径
                target = target,
                options = BuildOptions.None
            };

            BuildPipeline.BuildPlayer(buildPlayerOptions);
        }
    }
}