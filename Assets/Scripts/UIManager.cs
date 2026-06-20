using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("パネル")]
    public GameObject setupPanel;
    public GameObject simulationHUD;
    public GameObject submarineHUD;

    [Header("操作説明テキスト")]
    public GameObject placementControlsText; // 配置モードの操作説明
    public GameObject selectionControlsText; // 選択モードの操作説明

    void OnEnable()
    {
        // イベントの購読を開始
        ObstaclePlacementController.OnPlacementModeChanged += HandlePlacementModeChange;
    }

    void OnDisable()
    {
        // オブジェクトが無効になったら購読を解除（重要）
        ObstaclePlacementController.OnPlacementModeChanged -= HandlePlacementModeChange;
    }

    public void UpdateUIVisibility(GameManager.GameState state)
    {
        setupPanel.SetActive(state == GameManager.GameState.Setup);
        simulationHUD.SetActive(state == GameManager.GameState.Simulating);
        submarineHUD.SetActive(state == GameManager.GameState.Submarine);

        // 初期状態では選択モードのテキストを表示
        if (state == GameManager.GameState.Setup || state == GameManager.GameState.Simulating)
        {
            HandlePlacementModeChange(false);
        }
    }

    private void HandlePlacementModeChange(bool isPlacing)
    {
        placementControlsText.SetActive(isPlacing);
        selectionControlsText.SetActive(!isPlacing);
    }
}