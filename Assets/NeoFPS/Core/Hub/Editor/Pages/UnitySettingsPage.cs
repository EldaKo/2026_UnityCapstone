#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using NeoFPS.Hub;
using NeoFPS;

namespace NeoFPSEditor.Hub.Pages
{
    public class UnitySettingsPage : HubPage
    {
        // Constant used to check for changes. Each time
        private const int k_TargetLayersVersion = 2;
        private const int k_TargetPhysicsVersion = 6;
        private const int k_TargetInputVersion = 2;
        private const int k_TargetPlayerSettingsVersion = 1;
        private const int k_TargetBuildSettingsVersion = 7;

        // The custom settings json filenames
        private const string k_JsonLayers = "CustomSettings_Layers";
        private const string k_JsonPhysics = "CustomSettings_Physics";
        private const string k_JsonInput = "CustomSettings_Input";
        private const string k_JsonPlayer = "CustomSettings_Player";

        // The custom settings json filenames
        private const string k_ProjectSettingsLayers = "ProjectSettings/TagManager.asset";
        private const string k_ProjectSettingsPhysics = "ProjectSettings/DynamicsManager.asset";
        private const string k_ProjectSettingsInput = "ProjectSettings/InputManager.asset";
        private const string k_ProjectSettingsPlayer = "ProjectSettings/ProjectSettings.asset";

        // Links to the relevant documentation pages
        private const string k_DocsUrlLayers = "https://docs.neofps.com/manual/neofps-layers-and-tags.html";
        private const string k_DocsUrlPhysics = "https://docs.neofps.com/manual/neofps-layers-and-tags.html";
        private const string k_DocsUrlInput = "https://docs.neofps.com/manual/input-settings.html";
        private const string k_DocsUrlPlayer = "https://docs.neofps.com/manual/neofps-installation.html";
        private const string k_DocsUrlBuild = "https://docs.neofps.com/manual/neofps-installation.html";

        // Disable Neo browsers label
        private static readonly GUIContent k_DisableBrowsers = new GUIContent("Disable NeoFPS Object Browsers", "NeoFPS uses custom browsers for object fields in the inspector that narrow the options to prefabs or objects in the selection's child hierarchy. " +
            "This can cause a noticeable pause when opening the browser on older PCs, so you can disable these browsers here if you feel they are hurting more than helping your workflow.");
        
        private ReadmeHeader m_Heading = null;
        public ReadmeHeader heading
        {
            get
            {
                if (m_Heading == null)
                    m_Heading = new ReadmeHeader(LoadIcon("EditorImage_UnityLogoBlack", "EditorImage_UnityLogoWhite"), pageHeader);
                return m_Heading;
            }
        }

        // Scene names
        private const string k_GroupedBuildScenes = "FeatureDemo_";
        private readonly string[] k_FixedBuildScenes = new string[]
        {
            "MainMenu",
            "Loading",
            "DemoFacility_Scene"
        };

        public override string pageHeader
        {
            get { return "Unity Settings";  }
        }

        private MessageType m_Notification = MessageType.None;
        public override MessageType notification
        {
            get { return m_Notification; }
        }

        public override void Awake()
        {
            RefreshNotification();
        }

        public override void OnGUI()
        {
            ReadmeEditorUtility.DrawReadmeHeader(heading, true);
            EditorGUILayout.Space();

            InspectUnitySettings();
            InspectRenderPipelines();
            InspectEditorSettings();
        }

        #region UNITY SETTINGS

        public static int currentLayersVersion
        {
            get { return NeoFpsEditorPrefs.currentLayerSettingsVersion; }
            private set { NeoFpsEditorPrefs.currentLayerSettingsVersion = value; }
        }

        public static int currentPhysicsVersion
        {
            get { return NeoFpsEditorPrefs.currentPhysicsSettingsVersion; }
            private set { NeoFpsEditorPrefs.currentPhysicsSettingsVersion = value; }
        }

        public static int currentInputVersion
        {
            get { return NeoFpsEditorPrefs.currentInputSettingsVersion; }
            private set { NeoFpsEditorPrefs.currentInputSettingsVersion = value; }
        }

        public static int currentPlayerSettingsVersion
        {
            get { return NeoFpsEditorPrefs.currentPlayerSettingsVersion; }
            private set { NeoFpsEditorPrefs.currentPlayerSettingsVersion = value; }
        }

        public static int currentBuildSettingsVersion
        {
            get { return NeoFpsEditorPrefs.currentBuildSettingsVersion; }
            private set { NeoFpsEditorPrefs.currentBuildSettingsVersion = value; }
        }

        void InspectUnitySettings()
        {
            GUILayout.Label("NeoFPS requires various project settings to be applied in order to function correctly. This includes custom layers, custom input axes, and an optimised layer collision matrix.", ReadmeEditorUtility.bodyStyle);

            // Invalid input error
            if (s_InvalidInput)
                ShowInputError();

            // Out of date warning
            string message;
            bool outOfDate = ShowOutOfDateWarning(out message);
            if (outOfDate)
            {
                GUILayout.Space(2);
                EditorGUILayout.HelpBox(message, MessageType.Warning);
            }

            // Apply all
            EditorGUILayout.Space();
            GUILayout.Label("Easy Mode", ReadmeEditorUtility.h2Style);
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Hit the button to automatically apply all the latest settings that NeoFPS requires to function properly.", EditorStyles.wordWrappedLabel);

            if (GUILayout.Button("Apply All Required Settings") && EditorUtility.DisplayDialog("Warning", "This will overwrite a number of your project's settings.", "OK", "Cancel"))
                ApplyAllSettings();

            if (outOfDate)
            {
                GUILayout.Label("If you are confident your settings are correct (for example you have copied the settings from an existing NeoFPS project) then you can mark all as up to date and you won't be reminded until the version number next increases.", EditorStyles.wordWrappedLabel);
                
                if (GUILayout.Button("Mark All Settings As Up To Date"))
                    MarkAllAsGood();
            }

            GUILayout.EndVertical();

            // Apply Individual Settings
            EditorGUILayout.Space();
            GUILayout.Label("Individual settings", ReadmeEditorUtility.h2Style);
            if (EditorStyles.helpBox != null)
                GUILayout.BeginVertical(EditorStyles.helpBox);
            else
                GUILayout.BeginVertical();

            GUILayout.Label("If you are importing NeoFPS into an existing project then automatically applying the required settings could interfere with your project settings.\n\nIf you want, you can apply individual settings or learn about what NeoFPS requires. Hitting \"Apply Manually\" will flag the relevant settings as up to date and open the relevant unity settings for editing.\n", EditorStyles.wordWrappedLabel);

            // Layers and Tags
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Layers and Tags:", EditorStyles.boldLabel, GUILayout.Width(200));
            if (GUILayout.Button("Apply Automatically"))
                ApplyLatestLayerSettings(false);
            if (GUILayout.Button("Apply Manually"))
                ShowLayerSettings();
            if (GUILayout.Button("Learn More"))
                Application.OpenURL(k_DocsUrlLayers);
            GUILayout.EndHorizontal();

            // Layers and Tags
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Physics:", EditorStyles.boldLabel, GUILayout.Width(200));
            if (GUILayout.Button("Apply Automatically"))
                ApplyLatestPhysicsSettings(false);
            if (GUILayout.Button("Apply Manually"))
                ShowPhysicsSettings();
            if (GUILayout.Button("Learn More"))
                Application.OpenURL(k_DocsUrlPhysics);
            GUILayout.EndHorizontal();

            // Input
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Input Settings:", EditorStyles.boldLabel, GUILayout.Width(200));
            if (GUILayout.Button("Apply Automatically"))
                ApplyLatestInputSettings(false);
            if (GUILayout.Button("Apply Manually"))
                ShowInputSettings();
            if (GUILayout.Button("Learn More"))
                Application.OpenURL(k_DocsUrlInput);
            GUILayout.EndHorizontal();

            // Build Settings
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Build Settings:", EditorStyles.boldLabel, GUILayout.Width(200));
            if (GUILayout.Button("Apply Automatically"))
                ApplyLatestBuildSettings(false);
            if (GUILayout.Button("Apply Manually"))
                ShowBuildSettings();
            if (GUILayout.Button("Learn More"))
                Application.OpenURL(k_DocsUrlBuild);
            GUILayout.EndHorizontal();

            // Player Settings
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Player Settings:", EditorStyles.boldLabel, GUILayout.Width(200));
            if (GUILayout.Button("Apply Automatically"))
                ApplyLatestPlayerSettings(false);
            if (GUILayout.Button("Apply Manually"))
                ShowPlayerSettings();
            if (GUILayout.Button("Learn More"))
                Application.OpenURL(k_DocsUrlPlayer);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        void ApplyLatestLayerSettings(bool silent)
        {
            if (!silent && !EditorUtility.DisplayDialog("Warning", "This will overwrite your project's existing layers and tags\n\nSorting layers will not be affected.", "OK", "Cancel"))
                return;

            // Create the layer settings intermediate
            var settings = ScriptableObject.CreateInstance<SettingsIntermediate_Layers>();

            // Load values from JSON
            if (!settings.LoadFromJsonAsset(k_JsonLayers))
            {
                Debug.LogError("Couldn't load layers settings JSON.");
                return;
            }

            // Apply to ProjectSettings
            if (settings.ApplySettings(k_ProjectSettingsLayers))
                currentLayersVersion = k_TargetLayersVersion;

            if (!silent)
                RefreshNotification();
        }

        void ApplyLatestPhysicsSettings(bool silent)
        {
            if (!silent && !EditorUtility.DisplayDialog("Warning", "This will overwrite your project's layer collision matrix.\n\nOther physics settings will not be affected.", "OK", "Cancel"))
                return;

            // Create the physics settings intermediate
            var physicsIntermediate = ScriptableObject.CreateInstance<SettingsIntermediate_Physics>();

            // Load values from JSON
            if (!physicsIntermediate.LoadFromJsonAsset(k_JsonPhysics))
            {
                Debug.LogError("Couldn't load physics settings JSON.");
                return;
            }

            // Apply to ProjectSettings
            if (physicsIntermediate.ApplySettings(k_ProjectSettingsPhysics))
                currentPhysicsVersion = k_TargetPhysicsVersion;

            if (!silent)
                RefreshNotification();
        }

        void ApplyLatestInputSettings(bool silent)
        {
            if (!silent && !EditorUtility.DisplayDialog("Warning", "This will overwrite your project's input axes.", "OK", "Cancel"))
                return;

            // Create the settings intermediate
            var settings = ScriptableObject.CreateInstance<SettingsIntermediate_Input>();

            // Load values from JSON
            if (!settings.LoadFromJsonAsset(k_JsonInput))
            {
                Debug.LogError("Couldn't load input settings JSON.");
                return;
            }

            // Apply to ProjectSettings
            if (settings.ApplySettings(k_ProjectSettingsInput))
                currentInputVersion = k_TargetInputVersion;

            ResetInputCheck();

            if (!silent)
                RefreshNotification();
        }

        void ApplyLatestPlayerSettings(bool silent)
        {
            if (!silent && !EditorUtility.DisplayDialog("Warning", "This will overwrite the active color space of the project.\n\nNo other settings will be affected.", "OK", "Cancel"))
                return;

            // Create the settings intermediate
            var settings = ScriptableObject.CreateInstance<SettingsIntermediate_Player>();

            // Load values from JSON
            if (!settings.LoadFromJsonAsset(k_JsonPlayer))
            {
                Debug.LogError("Couldn't load player settings JSON.");
                return;
            }

            // Apply to ProjectSettings
            if (settings.ApplySettings(k_ProjectSettingsPlayer))
                currentPlayerSettingsVersion = k_TargetPlayerSettingsVersion;

            // Reset hub textures (switching lighting modes messes them up
            NeoFpsHubEditor.ResetTextures();

            if (!silent)
                RefreshNotification();
        }

        void ApplyLatestBuildSettings(bool silent)
        {
            currentBuildSettingsVersion = k_TargetBuildSettingsVersion;

            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool changed = false;
            string[] guids;

            // Get fixed build scenes
            for (int i = 0; i < k_FixedBuildScenes.Length; ++i)
            {
                // Get guid
                guids = AssetDatabase.FindAssets(k_FixedBuildScenes[i] + " t:Scene");
                if (guids.Length == 0)
                {
                    Debug.LogError("Couldn't find NeoFPS demo scene: " + k_FixedBuildScenes[i]);
                    break;
                }
                GUID guid = new GUID(guids[0]);

                // Check if it's already in the list
                int found = GetSceneIndex(buildScenes, guid);
                if (found == i)
                    continue;

                if (found == -1)
                {
                    // Not found. Add it at the correct index
                    buildScenes.Add(null);
                    for (int j = i + 1; j < buildScenes.Count; ++j)
                        buildScenes[j] = buildScenes[j - 1];
                    buildScenes[i] = new EditorBuildSettingsScene(guid, true);
                }
                else
                {
                    var swap = buildScenes[i];
                    buildScenes[i] = buildScenes[found];
                    buildScenes[found] = swap;
                }

                changed = true;
            }

            // Get grouped build scenes
            guids = AssetDatabase.FindAssets(k_GroupedBuildScenes + " t:Scene");
            if (guids.Length == 0)
            {
                Debug.LogError("No NeoFPS feature demo scenes found.");
                return;
            }

            // Check each one and add to list if not found
            for (int i = 0; i < guids.Length; ++i)
            {
                GUID guid = new GUID(guids[i]);

                int found = GetSceneIndex(buildScenes, guid);
                if (found == -1)
                {
                    buildScenes.Add(new EditorBuildSettingsScene(guid, true));
                    changed = true;
                }
            }

            if (changed)
            {
                EditorBuildSettings.scenes = buildScenes.ToArray();
            }

            if (!silent)
                RefreshNotification();
        }

        int GetSceneIndex(List<EditorBuildSettingsScene> scenes, GUID guid)
        {
            for (int i = 0; i < scenes.Count; ++i)
                if (scenes[i].guid == guid)
                    return i;
            return -1;
        }

        void ShowLayerSettings()
        {
            SettingsService.OpenProjectSettings("Project/Tags and Layers");
            currentLayersVersion = k_TargetLayersVersion;
            RefreshNotification();
        }

        void ShowPhysicsSettings()
        {
            SettingsService.OpenProjectSettings("Project/Physics");
            currentPhysicsVersion = k_TargetPhysicsVersion;
            RefreshNotification();
        }

        void ShowInputSettings()
        {
            SettingsService.OpenProjectSettings("Project/Input");
            currentInputVersion = k_TargetInputVersion;
            RefreshNotification();
        }

        void ShowBuildSettings()
        {
            EditorApplication.ExecuteMenuItem("File/Build Settings...");
            currentBuildSettingsVersion = k_TargetBuildSettingsVersion;
            RefreshNotification();
        }

        void ShowPlayerSettings()
        {
            SettingsService.OpenProjectSettings("Project/Player");
            currentPlayerSettingsVersion = k_TargetPlayerSettingsVersion;
            RefreshNotification();
        }

        void MarkAllAsGood()
        {
            currentLayersVersion = k_TargetLayersVersion;
            currentPhysicsVersion = k_TargetPhysicsVersion;
            currentInputVersion = k_TargetInputVersion;
            currentBuildSettingsVersion = k_TargetBuildSettingsVersion;
            currentPlayerSettingsVersion = k_TargetPlayerSettingsVersion;
            RefreshNotification();
        }

        public static bool ShowOutOfDateWarning(out string message)
        {
            bool show = false;
            show |= currentLayersVersion < k_TargetLayersVersion;
            show |= currentPhysicsVersion < k_TargetPhysicsVersion;
            show |= currentInputVersion < k_TargetInputVersion;
            show |= currentPlayerSettingsVersion < k_TargetPlayerSettingsVersion;

            if (!show)
                message = string.Empty;
            else
            {
                string msg = "It looks like some of the Unity settings that NeoFPS requires have changed since you last ran the wizard.\n\nThe following settings need updating:";
                if (currentLayersVersion < k_TargetLayersVersion)
                    msg += "\n- Layers and Tags";
                if (currentPhysicsVersion < k_TargetPhysicsVersion)
                    msg += "\n- Physics";
                if (currentInputVersion < k_TargetInputVersion)
                    msg += "\n- Input";
                if (currentPlayerSettingsVersion < k_TargetPlayerSettingsVersion)
                    msg += "\n- Player Settings";
                if (currentBuildSettingsVersion < k_TargetBuildSettingsVersion)
                    msg += "\n- Build Settings";
                msg += "\n\nThe Hub will keep showing on start until you apply the required settings or mark the settings as up to date below.";
                message = msg;
            }

            return show;
        }

        public void ShowInputError()
        {
            EditorGUILayout.Space();

            GUILayout.Label("Input Handling", ReadmeEditorUtility.h2Style);

            var oldColour = GUI.color;
            GUI.color = NeoFpsEditorGUI.errorRed;
            EditorGUILayout.LabelField("This project is set up to use the Input System only, but NeoFPS defaults to the Input Manager (changing this would break existing projects).", ReadmeEditorUtility.bodyStyle);
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("You can fix this by changing your Input Handling setting in the Player Settings to \"Input Manager (Legacy)\" or \"Both\", or by importing the NeoFPS Input System extension and running the provided conversion tools. If you choose to stick with the Input System approach then be warned that you may need to run these tools with each update of NeoFPS.", ReadmeEditorUtility.bodyStyle);
            GUI.color = oldColour;

            EditorGUILayout.Space();
            ReadmeEditorUtility.DrawWebLink("See the NeoFPS Documentation for Instructions", "https://docs.neofps.com/manual/input-inputsystem.html");

            EditorGUILayout.Space();
        }

        void ApplyAllSettings()
        {
            ApplyLatestLayerSettings(true);
            ApplyLatestPhysicsSettings(true);
            ApplyLatestInputSettings(true);
            ApplyLatestBuildSettings(true);
            ApplyLatestPlayerSettings(true);
            ResetInputCheck();
            RefreshNotification();
        }

        #endregion

        #region RENDER PIPELINES

        public const int targetBirpVersion = 1;
        public const int targetUrpVersion = 3;
        public const int targetHdrpVersion = 2;

        private const string k_UrpPackageName = "com.unity.render-pipelines.universal";
        private const string k_UrpMinVersion = "17.0.4";
        private const string k_HdrpPackageName = "com.unity.render-pipelines.high-definition";
        private const string k_HdrpMinVersion = "10.9.0";
        private const string k_PostProcessingPackageName = "com.unity.postprocessing";
        private const string k_BuiltInExtensionPackage = "NeoFpsRenderPipeline_BIRP_v2";
        private const string k_UrpExtensionPackage = "NeoFpsRenderPipeline_URP_v3";
        private const string k_HdrpExtensionPackage = "NeoFpsRenderPipeline_HDRP_v3";

        private const string k_BuiltInGuide = "https://docs.neofps.com/manual/graphics-birp.html";
        private const string k_UrpGuide = "https://docs.neofps.com/manual/graphics-urp.html";
        private const string k_HdrpGuide = "https://docs.neofps.com/manual/graphics-hdrp.html";
        private const string k_UrpUnityDocs = "https://docs.unity3d.com/Manual/com.unity.render-pipelines.universal.html";
        private const string k_HdrpUnityDocs = "https://docs.unity3d.com/Manual/com.unity.render-pipelines.high-definition.html";

        private bool m_ImportInProgress = false;
        private int m_PipelineCheckCounter = 0;
        private bool m_UrpInstalled = false;
        private bool m_HdrpInstalled = false;
        private bool m_HdrpParticleShadersInstalled = false;
        private bool m_PostProcessingInstalled = false;

        private static GUIContent m_LabelBirpOK = null;
        public static GUIContent labelBirpOK
        {
            get
            {
                if (m_LabelBirpOK == null)
                    m_LabelBirpOK = new GUIContent(" Project is compatible with the built-in render pipeline", NeoFpsEditorGUI.mgKeyFoundTexture);
                return m_LabelBirpOK;
            }
        }

        private static GUIContent m_LabelBirpNoPP = null;
        public static GUIContent labelBirpNoPP
        {
            get
            {
                if (m_LabelBirpNoPP == null)
                    m_LabelBirpNoPP = new GUIContent(" Post-Processing Stack is not installed (optional)", NeoFpsEditorGUI.mgKeyNotFoundTexture);
                return m_LabelBirpNoPP;
            }
        }

        private static GUIContent m_LabelBirpPPInstalled = null;
        public static GUIContent labelBirpPPInstalled
        {
            get
            {
                if (m_LabelBirpPPInstalled == null)
                    m_LabelBirpPPInstalled = new GUIContent(" Post-Processing Stack is installed", NeoFpsEditorGUI.mgKeyFoundTexture);
                return m_LabelBirpPPInstalled;
            }
        }

        private static GUIContent m_LabelUrpOK = null;
        public static GUIContent labelUrpOK
        {
            get
            {
                if (m_LabelUrpOK == null)
                    m_LabelUrpOK = new GUIContent(" Project is fully compatible with the universal render pipeline", NeoFpsEditorGUI.mgKeyFoundTexture);
                return m_LabelUrpOK;
            }
        }

        private static GUIContent m_LabelUrpOldUnity = null;
        public static GUIContent labelUrpOldUnity
        {
            get
            {
                if (m_LabelUrpOldUnity == null)
                    m_LabelUrpOldUnity = new GUIContent(" URP Requires Unity 2020.1 or newer", NeoFpsEditorGUI.mgKeyNotFoundTexture);
                return m_LabelUrpOldUnity;
            }
        }

        private static GUIContent m_LabelUrpNotInstalled = null;
        public static GUIContent labelUrpNotInstalled
        {
            get
            {
                if (m_LabelUrpNotInstalled == null)
                    m_LabelUrpNotInstalled = new GUIContent(" The URP package is not installed or out of date", NeoFpsEditorGUI.mgKeyNotFoundTexture);
                return m_LabelUrpNotInstalled;
            }
        }

        private static GUIContent m_LabelHdrpOK = null;
        public static GUIContent labelHdrpOK
        {
            get
            {
                if (m_LabelHdrpOK == null)
                    m_LabelHdrpOK = new GUIContent(" Project is fully compatible with the high-definition render pipeline", NeoFpsEditorGUI.mgKeyFoundTexture);
                return m_LabelHdrpOK;
            }
        }

        private static GUIContent m_LabelHdrpOldUnity = null;
        public static GUIContent labelHdrpOldUnity
        {
            get
            {
                if (m_LabelHdrpOldUnity == null)
                    m_LabelHdrpOldUnity = new GUIContent(" HDRP Requires Unity 2020.1 or newer", NeoFpsEditorGUI.mgKeyNotFoundTexture);
                return m_LabelHdrpOldUnity;
            }
        }

        private static GUIContent m_LabelHdrpNotInstalled = null;
        public static GUIContent labelHdrpNotInstalled
        {
            get
            {
                if (m_LabelHdrpNotInstalled == null)
                    m_LabelHdrpNotInstalled = new GUIContent(" The HDRP package is not installed or out of date", NeoFpsEditorGUI.mgKeyNotFoundTexture);
                return m_LabelHdrpNotInstalled;
            }
        }

        private static GUIContent m_LabelHdrpShaders = null;
        public static GUIContent labelHdrpShaders
        {
            get
            {
                if (m_LabelHdrpShaders == null)
                    m_LabelHdrpShaders = new GUIContent(" NeoFPS requires the HDRP particle system sample shaders (see guide)", NeoFpsEditorGUI.mgKeyNotFoundTexture);
                return m_LabelHdrpShaders;
            }
        }

        public static bool potentialBreakingUpgrade
        {
            get { return !NeoFpsEditorPrefs.unity6Checked && NeoFpsEditorPrefs.renderPipeline == RenderPipelineSetting.BuiltIn; }
        }

        public static RenderPipelineSetting currentRenderPipeline
        {
//#if NEOFPS_INTERNAL
//            get { return RenderPipelineSetting.URP; }
//#else
            get { return NeoFpsEditorPrefs.renderPipeline; }
//#endif
            private set { NeoFpsEditorPrefs.renderPipeline = value; }
        }

        void ImportCompleted()
        {
            m_ImportInProgress = false;
        }

        void CheckPipelineCompatibility()
        {
            // Will reset to 0 on domain reload such as package import
            // Uses counter instead of bool, since Shader.Find() on first frame will cause Unity Collab to explode and crash Unity
            if (++m_PipelineCheckCounter != 4)
                return;

            // Check if packages are installed
            m_PostProcessingInstalled = PackageDependencyChecker.IsPackageInstalled(k_PostProcessingPackageName, null);
            m_UrpInstalled = PackageDependencyChecker.IsPackageInstalled(k_UrpPackageName, k_UrpMinVersion);
            m_HdrpInstalled = PackageDependencyChecker.IsPackageInstalled(k_HdrpPackageName, k_HdrpMinVersion);

            // Check for HDRP particle shaders
            var s = Shader.Find("Shader Graphs/ParticleLit");
            if (s == null)
                s = Shader.Find("Shader Graphs/Particles/ParticleLit");
            if (s == null)
                s = Shader.Find("Shaders/ParticleLit");
            m_HdrpParticleShadersInstalled = (s != null);
        }

        bool ShowRenderPipelineWarning(out string message)
        {
            if (potentialBreakingUpgrade)
            {
                message = "The demo assets provided with NeoFPS have changed for Unity 6 to make the experience of setting up a new project easier.\n\nWith Unity 6, Unity have switched the default new project to use URP and the input system, without support for the legacy input manager. Since the news that Unity will no longer be adding features to HDRP going forwards, I have decided to switch the demo assets to match the preferred Unity setup moving forwards.\n\nSorry for any problems this causes.";
                return true;
            }
            else
            {
                if (currentRenderPipeline == RenderPipelineSetting.Unknown)
                {
                    message = "NeoFPS has not been set up for a specific render pipeline. Please select the desired pipeline from the list below.";
                    return true;
                }
            }

            message = string.Empty;
            return false;
        }

        void InspectRenderPipelines()
        {
            // Render pipeline setup and install
            EditorGUILayout.Space();
            GUILayout.Label("Render Pipeline", ReadmeEditorUtility.h2Style);

            // Check pipeline compatibility
            CheckPipelineCompatibility();

            string message;
            bool showRPWarning = ShowRenderPipelineWarning(out message);
            if (showRPWarning)
            {
                GUILayout.Space(2);
                EditorGUILayout.HelpBox(message, MessageType.Error);
            }

            if (EditorStyles.helpBox != null)
                GUILayout.BeginVertical(EditorStyles.helpBox);
            else
                GUILayout.BeginVertical();

            GUILayout.Label("NeoFPS comes with unity packages containing alternative shaders, replacement materials and demo prefabs for the different render pipelines.\n\nSelecting a render pipeline from the buttons below will import the required unity packages and extract the relevant package files included ith NeoFPS. Switching render pipelines below will not remove the existing unity packages and assets, but some demo materials may be overwritten.", EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space();
            if (currentRenderPipeline == RenderPipelineSetting.Unknown)
            {
                GUI.color = new Color(1f, 0.2f, 0.2f);
                GUILayout.Label("No render pipeline set up with NeoFPS please select and import one below.", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = Color.cyan;
                GUILayout.Label("NeoFPS is currently set up for the following render pipeline: " + currentRenderPipeline, EditorStyles.boldLabel);
                GUI.color = Color.white;
            }
            EditorGUILayout.Space();

#if NEOFPS_INTERNAL
            if (GUILayout.Button("TEMP RESET"))
            {
                currentRenderPipeline = RenderPipelineSetting.Unknown;
                RefreshNotification();
            }
#endif

            GUILayout.EndVertical();

            InspectBIRP();
            InspectURP();
            InspectHDRP();
        }

        void InspectBIRP()
        {
            if (EditorStyles.helpBox != null)
                GUILayout.BeginVertical(EditorStyles.helpBox);
            else
                GUILayout.BeginVertical();

            if (currentRenderPipeline == RenderPipelineSetting.BuiltIn)
            {
                GUI.color = Color.cyan;
                GUILayout.Label("Built-In Render Pipeline (Current)", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }
            else
                GUILayout.Label("Built-In Render Pipeline", EditorStyles.boldLabel);

            GUI.color = Color.green;
            GUILayout.Label(labelBirpOK);
            GUI.color = Color.white;

            if (!m_PostProcessingInstalled)
                GUILayout.Label(labelBirpNoPP);
            else
                GUILayout.Label(labelBirpPPInstalled);

            GUILayout.Label(string.Empty);
            var rect = GUILayoutUtility.GetLastRect();

            rect.width -= 4;
            rect.width /= 3;
            //rect.width -= 2;
            //rect.width /= 2;

            if (m_ImportInProgress)
                GUI.enabled = false;

            // NeoFPS Docs
            if (GUI.Button(rect, "Built-In RP Guide"))
                Application.OpenURL(k_BuiltInGuide);

            // Import post-processing
            rect.x += rect.width + 2;
            if (m_PostProcessingInstalled)
                GUI.enabled = false;
            if (GUI.Button(rect, "Import Post Processing"))
            {
                m_ImportInProgress = true;
                PackageDependencyChecker.InstallPackage(k_PostProcessingPackageName, null, ImportCompleted);
            }
            GUI.enabled = true;

            // Apply pipeline
            rect.x += rect.width + 2;
            if (GUI.Button(rect, "Import And Apply"))
            {
                currentRenderPipeline = RenderPipelineSetting.BuiltIn;
                NeoFpsEditorPrefs.unity6Checked = true;
                var guids = AssetDatabase.FindAssets(k_BuiltInExtensionPackage + " t:DefaultAsset");
                if (guids.Length > 0)
                    AssetDatabase.ImportPackage(AssetDatabase.GUIDToAssetPath(guids[0]), true);
                RefreshNotification();
            }

            GUI.enabled = true;

            GUILayout.EndVertical();
        }

        void InspectURP()
        {
            if (EditorStyles.helpBox != null)
                GUILayout.BeginVertical(EditorStyles.helpBox);
            else
                GUILayout.BeginVertical();

            if (currentRenderPipeline == RenderPipelineSetting.URP)
            {
                GUI.color = Color.cyan;
                GUILayout.Label("Universal Render Pipeline (Current)", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }
            else
                GUILayout.Label("Universal Render Pipeline", EditorStyles.boldLabel);

#if !UNITY_2020_1_OR_NEWER

            bool compatible = false;
                        
            GUILayout.Label(labelUrpOldUnity);

#else

            bool compatible = m_UrpInstalled;

            if (m_UrpInstalled)
            {
                GUI.color = Color.green;
                GUILayout.Label(labelUrpOK);
                GUI.color = Color.white;
            }
            else
            {
                GUILayout.Label(labelUrpNotInstalled);
            }

#endif

            GUILayout.Label(string.Empty);
            var rect = GUILayoutUtility.GetLastRect();

            rect.width -= 4;
            rect.width /= 3;

            if (m_ImportInProgress)
                GUI.enabled = false;

            // Guide
            if (GUI.Button(rect, "Universal RP Guide"))
                Application.OpenURL(k_UrpGuide);

            // Docs
            rect.x += rect.width + 2;
            if (GUI.Button(rect, "Unity URP Docs"))
                Application.OpenURL(k_UrpUnityDocs);

            // Apply pipeline
            rect.x += rect.width + 2;
            GUI.enabled = compatible;
            if (currentRenderPipeline == RenderPipelineSetting.Unknown || (potentialBreakingUpgrade && currentRenderPipeline == RenderPipelineSetting.BuiltIn))
            {
                if (GUI.Button(rect, "Use URP (Default Project Setup)"))
                {
                    currentRenderPipeline = RenderPipelineSetting.URP;
                    NeoFpsEditorPrefs.unity6Checked = true;
                    RefreshNotification();
                }
            }
            else
            {
                if (GUI.Button(rect, "Re-import And Apply"))
                {
                    currentRenderPipeline = RenderPipelineSetting.URP;
                    NeoFpsEditorPrefs.unity6Checked = true;
                    var guids = AssetDatabase.FindAssets(k_UrpExtensionPackage + " t:DefaultAsset");
                    if (guids.Length > 0)
                        AssetDatabase.ImportPackage(AssetDatabase.GUIDToAssetPath(guids[0]), true);
                    RefreshNotification();
                }
            }
            GUI.enabled = true;

            GUILayout.EndVertical();
        }

        void InspectHDRP()
        {
            if (EditorStyles.helpBox != null)
                GUILayout.BeginVertical(EditorStyles.helpBox);
            else
                GUILayout.BeginVertical();

            if (currentRenderPipeline == RenderPipelineSetting.HDRP)
            {
                GUI.color = Color.cyan;
                GUILayout.Label("High-Definition Render Pipeline (Current)", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }
            else
                GUILayout.Label("High-Definition Render Pipeline", EditorStyles.boldLabel);

#if !UNITY_2020_1_OR_NEWER

            bool compatible = false;

            GUILayout.Label(labelHdrpOldUnity);

#else

            bool compatible = m_HdrpInstalled && m_HdrpParticleShadersInstalled;

            if (compatible)
            {
                GUI.color = Color.green;
                GUILayout.Label(labelHdrpOK);
                GUI.color = Color.white;
            }
            else
            {
                if (!m_HdrpInstalled)
                    GUILayout.Label(labelHdrpNotInstalled);
                if (!m_HdrpParticleShadersInstalled)
                {
                    GUILayout.Label(labelHdrpShaders);                    

                    // Re-scan button
                    var scanRect = GUILayoutUtility.GetLastRect();
                    scanRect.x += scanRect.width - 80;
                    scanRect.width = 80;
                    if (GUI.Button(scanRect, "Re-Scan"))
                    {
                        // Check for HDRP particle shaders
                        var s = Shader.Find("Shader Graphs/ParticleLit");
                        m_HdrpParticleShadersInstalled = (s != null);
                    }
                }
            }

#endif

            GUILayout.Label(string.Empty);
            var rect = GUILayoutUtility.GetLastRect();

            rect.width -= 4;
            rect.width /= 3;

            if (m_ImportInProgress)
                GUI.enabled = false;

            // Guide
            if (GUI.Button(rect, "High-Definition RP Guide"))
                Application.OpenURL(k_HdrpGuide);

            // Docs
            rect.x += rect.width + 2;
            if (GUI.Button(rect, "Unity HDRP Docs"))
                Application.OpenURL(k_HdrpUnityDocs);

            // Apply pipeline
            rect.x += rect.width + 2;
            GUI.enabled = compatible;
            if (GUI.Button(rect, "Import And Apply"))
            {
                currentRenderPipeline = RenderPipelineSetting.HDRP;
                NeoFpsEditorPrefs.unity6Checked = true;
                var guids = AssetDatabase.FindAssets(k_HdrpExtensionPackage + " t:DefaultAsset");
                if (guids.Length > 0)
                    AssetDatabase.ImportPackage(AssetDatabase.GUIDToAssetPath(guids[0]), true);
                RefreshNotification();
            }
            GUI.enabled = true;

            GUILayout.EndVertical();
        }

#endregion

        #region EDITOR SETTINGS

        void InspectEditorSettings()
        {
            // Render pipeline setup and install
            EditorGUILayout.Space();
            GUILayout.Label("Editor Settings", ReadmeEditorUtility.h2Style);

            EditorGUI.BeginChangeCheck();
            bool disableNeoObjectBrowsers = EditorGUILayout.ToggleLeft(k_DisableBrowsers, NeoFpsEditorPrefs.disableNeoObjectBrowsers);
            if (EditorGUI.EndChangeCheck())
                NeoFpsEditorPrefs.disableNeoObjectBrowsers = disableNeoObjectBrowsers;
        }

        #endregion

        #region INPUT CHECKS

#pragma warning disable CS0414
        private static bool s_InputChecked = false;
#pragma warning restore CS0414

        private static bool s_InvalidInput = false;

        static void CheckInput()
        {
#if !ENABLE_LEGACY_INPUT_MANAGER && !NEOFPS_LOAD_ON_DEMAND
            if (!s_InputChecked)
            {
                var inputManager = Resources.Load<NeoFpsInputManagerBase>("FpsManager_Input");
                s_InvalidInput = (inputManager is NeoFpsInputManager);
                s_InputChecked = true;
            }
#endif
        }

        [InitializeOnLoadMethod]
        static void ResetInputCheck()
        {
            s_InputChecked = false;
        }

        public override void OnEnable()
        {
            base.OnEnable();

            s_InputChecked = false;
            CheckInput();
        }

        #endregion

        private static bool s_UpgradeWarning = false;

        public void RefreshNotification ()
        {
            if (CheckIsOutOfDate())
                m_Notification = MessageType.Error;
            else
                m_Notification = MessageType.None;
        }

        public static bool CheckIsOutOfDate()
        {
            if (currentLayersVersion < k_TargetLayersVersion) return true;
            if (currentPhysicsVersion < k_TargetPhysicsVersion) return true;
            if (currentInputVersion < k_TargetInputVersion) return true;
            if (currentPlayerSettingsVersion < k_TargetPlayerSettingsVersion) return true;
            if (currentRenderPipeline == RenderPipelineSetting.Unknown) return true;

            if (potentialBreakingUpgrade)
            {
                if (!s_UpgradeWarning)
                    Debug.LogWarning("The NeoFPS demo assets have been changed to use URP and the input system for Unity 6. Please visit the NeoFPS Hub");
                s_UpgradeWarning = true;
                return true;
            }

            CheckInput();
            if (s_InvalidInput)
                return true;

            return false;
        }
    }
}

#endif