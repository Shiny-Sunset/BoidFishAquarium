using UnityEngine;

public class ExitGameButton : MonoBehaviour
{
    /// <summary>
    /// ゲームを終了します。
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("ゲームを終了します...");

#if UNITY_EDITOR
        // Unityエディタの場合、再生モードを停止
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ビルドされたアプリケーションの場合、アプリケーションを終了
        Application.Quit();
#endif
    }
}
