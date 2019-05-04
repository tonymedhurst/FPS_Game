/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using System;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Editor.Managers
{
    /// <summary>
    /// Draws the inspector for an addon that has been installed.
    /// </summary>
    public abstract class AddonInspector
    {
        /// <summary>
        /// Draws the addon inspector.
        /// </summary>
        public abstract void DrawInspector();
    }

    /// <summary>
    /// Draws a list of all of the available addons.
    /// </summary>
    [OrderedEditorItem("Addons", 11)]
    public class AddonsManager : Manager
    {
        private string[] m_ToolbarStrings = { "Installed Addons", "Available Addons" };
        [SerializeField] private bool m_DrawInstalledAddons = true;

        private AddonInspector[] m_AddonInspectors;
        private string[] m_AddonNames;

        private static GUIStyle s_AddonTitle;
        private static GUIStyle AddonTitle
        {
            get
            {
                if (s_AddonTitle == null) {
                    s_AddonTitle = new GUIStyle(InspectorStyles.CenterBoldLabel);
                    s_AddonTitle.fontSize = 14;
                    s_AddonTitle.alignment = TextAnchor.MiddleLeft;
                }
                return s_AddonTitle;
            }
        }

        /// <summary>
        /// Stores the information about the addon.
        /// </summary>
        private class AvailableAddon
        {
            private const int c_IconSize = 78;

            private int m_ID;
            private string m_Name;
            private string m_AddonURL;
            private string m_Description;
            private bool m_Installed;
            private Texture2D m_Icon;
            private MainManagerWindow m_MainManagerWindow;

#if UNITY_2018_3_OR_NEWER
            private UnityEngine.Networking.UnityWebRequest m_IconRequest;
            private UnityEngine.Networking.DownloadHandlerTexture m_TextureDownloadHandler;
#else
            private WWW m_IconRequest;
#endif

            /// <summary>
            /// Constructor for the AvailableAddon class.
            /// </summary>
            public AvailableAddon(int id, string name, string iconURL, string addonURL, string description, string type, MainManagerWindow mainManagerWindow)
            {
                m_ID = id;
                m_Name = name;
                m_AddonURL = addonURL;
                m_Description = description;
                // The addon is installed if the type exists.
                m_Installed = UltimateCharacterController.Utility.UnityEngineUtility.GetType(type) != null;
                m_MainManagerWindow = mainManagerWindow;

                // Start loading the icon as soon as the url is retrieved.
#if UNITY_2018_3_OR_NEWER
                m_TextureDownloadHandler = new UnityEngine.Networking.DownloadHandlerTexture();
                m_IconRequest = UnityEngine.Networking.UnityWebRequest.Get(iconURL);
                m_IconRequest.downloadHandler = m_TextureDownloadHandler;
                m_IconRequest.SendWebRequest();
#else
                m_IconRequest = new WWW(iconURL);
#endif
            }

            /// <summary>
            /// Draws the inspector for the available addon.
            /// </summary>
            public void DrawAddon()
            {
                if (m_IconRequest != null) {
                    if (m_IconRequest.isDone) {
                        if (string.IsNullOrEmpty(m_IconRequest.error)) {
#if UNITY_2018_3_OR_NEWER
                            m_Icon = m_TextureDownloadHandler.texture;
#else
                            m_Icon = m_IconRequest.texture;
#endif
                        }
                        m_IconRequest = null;
                    } else {
                        m_MainManagerWindow.Repaint();
                    }
                }

                // Draw the addon details.
                EditorGUILayout.BeginHorizontal();
                if (m_Icon != null) {
                    GUILayout.Label(m_Icon);
                }

                EditorGUILayout.BeginVertical();
                var name = m_Name;
                if (m_Installed) {
                    name += " (INSTALLED)";
                }
                EditorGUILayout.LabelField(name, InspectorStyles.BoldLabel, GUILayout.Height(20));
                EditorGUILayout.BeginHorizontal();
                if (!string.IsNullOrEmpty(m_AddonURL) && GUILayout.Button("Overview", GUILayout.MaxWidth(150))) {
                    Application.OpenURL(m_AddonURL);
                }
                if (GUILayout.Button("Asset Store", GUILayout.MaxWidth(150))) {
                    Application.OpenURL("https://opsive.com/asset/UltimateCharacterController/AssetRedirect.php?asset=" + m_ID);
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
                EditorGUILayout.LabelField(m_Description, InspectorStyles.WordWrapLabel);
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }
        }

        private Vector2 m_ScrollPosition;
#if UNITY_2018_3_OR_NEWER
        private UnityEngine.Networking.UnityWebRequest m_AddonsReqest;
#else
        private WWW m_AddonsReqest;
#endif
        private AvailableAddon[] m_AvailableAddons;

        /// <summary>
        /// Initialize the manager after deserialization.
        /// </summary>
        public override void Initialize(MainManagerWindow mainManagerWindow)
        {
            base.Initialize(mainManagerWindow);

            BuildInstalledAddons();
        }

        /// <summary>
        /// Draws the Manager.
        /// </summary>
        public override void OnGUI()
        {
            var toolbarSelection = GUILayout.Toolbar(m_DrawInstalledAddons ? 0 : 1, m_ToolbarStrings, EditorStyles.toolbarButton);
            m_DrawInstalledAddons = toolbarSelection == 0;
            GUILayout.Space(10);

            if (m_DrawInstalledAddons) {
                DrawInstalledAddons();
            } else {
                DrawAvailableAddons();
            }
        }

        /// <summary>
        /// Draws the inspector for all installed addons.
        /// </summary>
        private void DrawInstalledAddons()
        {
            if (m_AddonInspectors == null || m_AddonInspectors.Length == 0) {
                GUILayout.Label("No addons are currently installed.\n\nSelect the \"Available Addons\" tab to see a list of all of the available addons.");
                return;
            }

            for (int i = 0; i < m_AddonInspectors.Length; ++i) {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label(m_AddonNames[i], InspectorStyles.LargeBoldLabel);
                GUILayout.Space(4);
                m_AddonInspectors[i].DrawInspector();
                if (i != m_AddonInspectors.Length - 1) {
                    GUILayout.Space(10);
                }
                EditorGUILayout.EndVertical();
            }
        }

        /// <summary>
        /// Finds and create an instance of the inspectors for all of the installed addons.
        /// </summary>
        private void BuildInstalledAddons()
        {
            var addonInspectors = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var addonIndexes = new List<int>();
            for (int i = 0; i < assemblies.Length; ++i) {
                var assemblyTypes = assemblies[i].GetTypes();
                for (int j = 0; j < assemblyTypes.Length; ++j) {
                    // Must implement AddonInspector.
                    if (!typeof(AddonInspector).IsAssignableFrom(assemblyTypes[j])) {
                        continue;
                    }

                    // Ignore abstract classes.
                    if (assemblyTypes[j].IsAbstract) {
                        continue;
                    }

                    // A valid inspector class.
                    addonInspectors.Add(assemblyTypes[j]);
                    var index = addonIndexes.Count;
                    if (assemblyTypes[j].GetCustomAttributes(typeof(OrderedEditorItem), true).Length > 0) {
                        var item = assemblyTypes[j].GetCustomAttributes(typeof(OrderedEditorItem), true)[0] as OrderedEditorItem;
                        index = item.Index;
                    }
                    addonIndexes.Add(index);
                }
            }

            // Do not reinitialize the inspectors if they are already initialized and there aren't any changes.
            if (m_AddonInspectors != null && m_AddonInspectors.Length == addonInspectors.Count) {
                return;
            }

            // All of the manager types have been found. Sort by the index.
            var inspectorTypes = addonInspectors.ToArray();
            Array.Sort(addonIndexes.ToArray(), inspectorTypes);

            m_AddonInspectors = new AddonInspector[addonInspectors.Count];
            m_AddonNames = new string[addonInspectors.Count];

            // The inspector types have been found and sorted. Add them to the list.
            for (int i = 0; i < inspectorTypes.Length; ++i) {
                m_AddonInspectors[i] = Activator.CreateInstance(inspectorTypes[i]) as AddonInspector;

                var name = InspectorUtility.SplitCamelCase(inspectorTypes[i].Name);
                if (addonInspectors[i].GetCustomAttributes(typeof(OrderedEditorItem), true).Length > 0) {
                    var item = inspectorTypes[i].GetCustomAttributes(typeof(OrderedEditorItem), true)[0] as OrderedEditorItem;
                    name = item.Name;
                }
                m_AddonNames[i] = name;
            }
        }

        /// <summary>
        /// Draws all of the addons that are currently available.
        /// </summary>
        private void DrawAvailableAddons()
        {
            if (m_AvailableAddons == null && m_AddonsReqest == null) {
#if UNITY_2018_3_OR_NEWER
                m_AddonsReqest = UnityEngine.Networking.UnityWebRequest.Get("https://opsive.com/asset/UltimateCharacterController/AddonsList.txt");
                m_AddonsReqest.SendWebRequest();
#else
                m_AddonsReqest = new WWW("https://opsive.com/asset/UltimateCharacterController/AddonsList.txt");
#endif
            } else if (m_AvailableAddons == null && m_AddonsReqest.isDone && string.IsNullOrEmpty(m_AddonsReqest.error)) {
#if UNITY_2018_3_OR_NEWER
                var splitAddons = m_AddonsReqest.downloadHandler.text.Split('\n');
#else
                var splitAddons = m_AddonsReqest.text.Split('\n');
#endif
                m_AvailableAddons = new AvailableAddon[splitAddons.Length];
                var count = 0;
                for (int i = 0; i < splitAddons.Length; ++i) {
                    if (string.IsNullOrEmpty(splitAddons[i])) {
                        continue;
                    }

                    // The data must contain info on the addon name, id, icon, addon url, description, and type.
                    var addonData = splitAddons[i].Split(',');
                    if (addonData.Length < 6) {
                        continue;
                    }

                    m_AvailableAddons[count] = new AvailableAddon(int.Parse(addonData[0].Trim()), addonData[1].Trim(), addonData[2].Trim(), addonData[3].Trim(), addonData[4].Trim(), addonData[5].Trim(), m_MainManagerWindow);
                    count++;
                }

                if (count != m_AvailableAddons.Length) {
                    Array.Resize(ref m_AvailableAddons, count);
                }
                m_AddonsReqest = null;
            } else if (m_AddonsReqest != null) {
                m_MainManagerWindow.Repaint();
            }

            // Draw the addons once they are loaded.
            if (m_AvailableAddons != null && m_AvailableAddons.Length > 0) {
                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
                // Draw each addon.
                for (int i = 0; i < m_AvailableAddons.Length; ++i) {
                    m_AvailableAddons[i].DrawAddon();
                    if (i != m_AvailableAddons.Length - 1) {
                        GUILayout.Space(10);
                    }
                }
                EditorGUILayout.EndScrollView();
            } else {
                if (m_AddonsReqest != null && m_AddonsReqest.isDone && !string.IsNullOrEmpty(m_AddonsReqest.error)) {
                    EditorGUILayout.LabelField("Error: Unable to retrieve addons.");
                } else {
                    EditorGUILayout.LabelField("Retrieveing the list of current addons...");
                }
            }
        }
    }
}