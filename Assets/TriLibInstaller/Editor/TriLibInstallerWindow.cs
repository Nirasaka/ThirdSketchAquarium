using UnityEditor;
using UnityEngine;

namespace TriLibInstaller
{
    [InitializeOnLoad]
    public class TriLibInstallerWindow : EditorWindow
    {
        private static string _packagePath;
        private const string SuppressKey = "TriLibInstallerSuppress";
        private const string CurrentVersion = "2.6.4";
        private bool _suppress;

        static TriLibInstallerWindow()
        {
            EditorApplication.delayCall += AutoShow;
        }

        [MenuItem("TriLib/Extract TriLib Package")]
        public static void ShowFromMenu()
        {
            _packagePath = FindPackage();
            if (string.IsNullOrEmpty(_packagePath))
            {
                EditorUtility.DisplayDialog("TriLib Installer", "The package (trilibcontents.unitypackage) was not found in the Assets folder.", "OK");
                return;
            }
            if (IsAlreadyInstalled())
            {
                EditorUtility.DisplayDialog("TriLib Installer", "TriLib " + CurrentVersion + " is already installed. No installation is required.", "OK");
                return;
            }
            OpenWindow();
        }

        private static void AutoShow()
        {
            if (EditorPrefs.GetBool(SuppressKey, false))
            {
                return;
            }
            _packagePath = FindPackage();
            if (!string.IsNullOrEmpty(_packagePath) && !IsAlreadyInstalled())
            {
                OpenWindow();
            }
        }

        private static void OpenWindow()
        {
            var window = GetWindow<TriLibInstallerWindow>(true, "TriLib Installer", true);
            window.minSize = new Vector2(660f, 284f);
            window._suppress = EditorPrefs.GetBool(SuppressKey, false);
        }

        private static string FindPackage()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:DefaultAsset"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("trilibcontents.unitypackage"))
                {
                    return path;
                }
            }
            return null;
        }

        private static bool IsAlreadyInstalled()
        {
            foreach (var guid in AssetDatabase.FindAssets("TriLibInstalledVersion t:TextAsset"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("TriLibInstalledVersion.txt"))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                    if (asset != null)
                    {
                        return asset.text.Trim() == CurrentVersion;
                    }
                }
            }
            return false;
        }

        private static string GetInstalledVersion()
        {
            foreach (var guid in AssetDatabase.FindAssets("TriLibInstalledVersion t:TextAsset"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("TriLibInstalledVersion.txt"))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                    if (asset != null)
                    {
                        return asset.text.Trim();
                    }
                }
            }
            return null;
        }

        private static bool ConfirmVersionMismatch(string installedVersion)
        {
            return EditorUtility.DisplayDialog(
                "TriLib Installer - Version Mismatch Detected",
                "An existing TriLib installation was detected (version " + installedVersion + ").\n\n" +
                "It is strongly recommended that you back up and delete your TriLib folder before proceeding.\n\n" +
                "Do you want to extract the package anyway?",
                "Extract Anyway",
                "Cancel"
            );
        }

        private static bool OldInstallExists()
        {
            foreach (var guid in AssetDatabase.FindAssets("TriLibCore t:DefaultAsset"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("TriLibCore.dll"))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ConfirmOldInstall()
        {
            return EditorUtility.DisplayDialog(
                "TriLib Installer - Old Installation Detected",
                "An outdated TriLib installation was detected (TriLibCore.dll found).\n\n" +
                "It is strongly recommended that you back up and delete your TriLib folder before proceeding.\n\n" +
                "Do you want to extract the package anyway?",
                "Extract Anyway",
                "Cancel"
            );
        }

        private static void TriggerImport()
        {
            AssetDatabase.ImportPackage(_packagePath, true);
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(8f);
            GUILayout.BeginVertical();
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("TriLib " + CurrentVersion + " Installer", EditorStyles.boldLabel);
            EditorGUILayout.Space(6f);
            EditorGUILayout.HelpBox(
                "\nThis TriLib version requires replacing all existing TriLib files.\n\n" +
                "Before proceeding, if you already have an existing TriLib installation:\n" +
                "1. Back up your TriLib folder.\n" +
                "2. Close Unity and delete the TriLib folder using your OS file explorer (Windows Explorer / Finder). Do not delete it from inside Unity.\n" +
                "3. Reopen Unity and use this dialog to extract the update.\n\n" +
                "Once you have done that, click \"Extract Package\" to install TriLib.\n",
                MessageType.Warning
            );
            EditorGUILayout.Space(8f);
            EditorGUI.BeginChangeCheck();
            _suppress = EditorGUILayout.ToggleLeft(" Do not show this dialog anymore", _suppress);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(SuppressKey, _suppress);
            }
            EditorGUILayout.Space(10f);
            if (GUILayout.Button("Extract Package", GUILayout.Height(32f)))
            {
                var installedVersion = GetInstalledVersion();
                if (installedVersion != null && installedVersion != CurrentVersion && !ConfirmVersionMismatch(installedVersion))
                {
                    return;
                }
                if (OldInstallExists() && !ConfirmOldInstall())
                {
                    return;
                }
                TriggerImport();
                Close();
            }
            EditorGUILayout.Space(4f);
            if (GUILayout.Button("Cancel", GUILayout.Height(24f)))
            {
                Close();
            }
            GUILayout.EndVertical();
            GUILayout.Space(8f);
            GUILayout.EndHorizontal();
        }
    }
}