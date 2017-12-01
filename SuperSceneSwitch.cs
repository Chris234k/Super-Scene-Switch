using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SuperSceneSwitch : EditorWindow
{
    struct SceneData
    {
        public string name;
        public string path;

        public SceneData(string name, string path)
        {
            this.name = name;
            this.path = path;
        }
    }

    static SceneData[] knownScenes;
    static EditorWindow window;
    static bool isWindowPositioned;

    public Vector2 scrollPos;

    [MenuItem("Tools/Super Scene Switch _F1")]
    public static void ShowWindow()
    {
        window = GetWindow(typeof(SuperSceneSwitch));
        isWindowPositioned = false;
    }

    void OnGUI()
    {
        if ( Application.isPlaying ) // TODO(Chris) Not really sure what to do with the window while playing. Don't want to close it (in case it is docked)
        {
            isWindowPositioned = true; // Prevent ShowWindow from queuing up a window position change while the application is running
            return;
        }

        if ( !isWindowPositioned && window )
        {
            // Position window so that you don't have to move your mouse to click the first scene
            Vector2 mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

            float width = 200, height = 200;
            window.position = new Rect(mousePos.x - width / 2, mousePos.y, width, height);

            isWindowPositioned = true;
        }

        if ( knownScenes != null && knownScenes.Length > 0 ) // Scene data is available
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            GUILayout.BeginVertical();

            for ( int i = 0; i < knownScenes.Length; i++ )
            {
                GUILayout.BeginHorizontal();

                if ( GUILayout.Button(knownScenes[i].name) )
                {
                    if ( EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() ) // Confirm user wants to leave scene with unsaved work (passthrough if scene is not dirty)
                    {
                        EditorSceneManager.OpenScene(knownScenes[i].path);
                        Close(); // Close SuperSceneSwitch, it has done its job
                    }
                }

                if ( EditorSceneManager.playModeStartScene != null && EditorSceneManager.playModeStartScene.name == knownScenes[i].name ) // New in 2017.1, play button loads different scene than the open one
                {
                    GUI.backgroundColor = Color.gray; // Highlight scene that will play
                }

                if ( GUILayout.Button("|>", GUILayout.ExpandWidth(false)) ) // Click to set play mode start scene
                {
                    SetPlayModeStartScene(knownScenes[i]);
                }

                GUI.backgroundColor = Color.white; // Reset colors (in case this scene is the play mode scene)

                GUILayout.EndHorizontal();
            }

            if ( GUILayout.Button("[Reload Scene Info]") ) // If the build scene list updates while SuperSceneSwitch is open, a force reload will update the list
            {
                knownScenes = GetScenes();
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
        else
        {
            if ( GUILayout.Button("[Load Scene Info]", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)) )
            {
                knownScenes = GetScenes();
            }
        }
    }

    static SceneData[] GetScenes()
    {
        SceneData[] scenes = new SceneData[EditorBuildSettings.scenes.Length];

        for ( int i = 0; i < EditorBuildSettings.scenes.Length; i++ ) // ROBUSTNESS(Chris) Option to load all scenes in project
        {
            var scene = EditorBuildSettings.scenes[i];

            string name = scene.path.Substring(scene.path.LastIndexOf('/') + 1);
            name = name.Substring(0, name.Length - 6);

            scenes[i] = new SceneData(name, scene.path);
        }

        return scenes;
    }

    void SetPlayModeStartScene(SceneData data)
    {
        SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(data.path);
        if ( scene != null )
        {
            if ( EditorSceneManager.playModeStartScene != null && EditorSceneManager.playModeStartScene.name == data.name ) // Clear if the current scene was passed in
            {
                EditorSceneManager.playModeStartScene = null;
            }
            else
            {
                EditorSceneManager.playModeStartScene = scene;
            }

        }
        else
        {
            Debug.Log("Could not find scene " + data.path);
        }
    }
}
