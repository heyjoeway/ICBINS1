namespace UnityEditor.Utility
{
 
    /// <summary>
    /// Disables the editor Auto Refresh asset import behavior when in playing mode and restores it on return to edit mode.
    /// </summary>
    [InitializeOnLoad]
    public static class DisableAssetAutoImportOnPlay
    {
 
        /// <summary>
        /// Due to InitializeOnLoadAttribute, this static Constructor will be called when this editor assembly loads (on startup and on AppDomain restart after compile).
        /// </summary>
        static DisableAssetAutoImportOnPlay()
        {
            EditorApplication.playModeStateChanged += DisableAssetAutoImportOnPlay.OnEditorApplicationPlayModeStateChanged;
        }

 
        /// <summary>
        /// Called when the EditorApplication.playModeStateChanged event fires
        /// </summary>
        /// <param name="playingState"></param>
        private static void OnEditorApplicationPlayModeStateChanged(PlayModeStateChange playingState)
        {
            switch (playingState)
            {
                // Called the moment after the user presses the Play button.
                case PlayModeStateChange.ExitingEditMode:
                    break;
 
                // Called when the initial scene is loaded and first rendered, after ExitingEditMode..
                case PlayModeStateChange.EnteredPlayMode:
                    if (EditorPrefs.HasKey("kAutoRefresh"))
                        EditorPrefs.SetBool("kAutoRefresh", false);
                    break;
 
                // Called the moment after the user presses the Stop button.
                //case PlayModeStateChange.ExitingPlayMode:
                //    break;
 
                // Called after the current scene is unloaded, after ExitingPlayMode.
                case PlayModeStateChange.EnteredEditMode:
                    if (EditorPrefs.HasKey("kAutoRefresh"))
                        EditorPrefs.SetBool("kAutoRefresh", true);
                    break;
 
                default:
                    break;
            }
 
        }
 
    }
 
}