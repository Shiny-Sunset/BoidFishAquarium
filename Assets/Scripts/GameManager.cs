using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // List<Boid> を使用するために必要

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum ExecutionMode { CPU, ECS }
    public enum GameState { Setup, Simulating, Submarine }

    [Header("モード設定")]
    public ExecutionMode currentMode = ExecutionMode.CPU;
    public GameState currentState = GameState.Setup;

    [Header("コンポーネント参照")]
    public UIManager uiManager;
    public BoidManager boidManager;
    public Spawner spawner;
    public Boid boidPrefab;
    public BoidSettings boidSettings;
    public Color boidColor = Color.white;
    public PlayerInteractionController interactionController;
    public ObstaclePlacementController obstacleController;
    public SubmarineController submarineController;
    public Camera topDownCamera;
    public Camera submarineCamera;

    private float currentTopDownCameraAngle = 180f;
    public float topDownCameraDistance = 20f; // 水槽の中心からの距離

    [Header("セットアップ用UI")]
    public Slider fishCountSlider;
    public TMP_Dropdown modeSelector;
    public TMP_Text fishCountText;

    private int fishCount;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        modeSelector.onValueChanged.AddListener(SetExecutionMode);
        fishCountSlider.onValueChanged.AddListener(delegate { UpdateFishCountText(); });

        // 初期設定
        SetExecutionMode(modeSelector.value);
        UpdateFishCountText();
        SwitchState(GameState.Setup);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleSubmarineMode();
        }

        // シミュレーション中にBackspaceキーでリセット
        if ((currentState == GameState.Simulating || currentState == GameState.Submarine) && Input.GetKeyDown(KeyCode.Backspace))
        {
            ResetSimulation();
        }

        // SETUP状態またはシミュレーション状態でのみカメラ回転を処理
        if (currentState == GameState.Setup || currentState == GameState.Simulating)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                RotateTopDownCamera(90f);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                RotateTopDownCamera(-90f);
            }
        }
    }

    private void RotateTopDownCamera(float angle)
    {
        currentTopDownCameraAngle += angle;
        currentTopDownCameraAngle = Mathf.Repeat(currentTopDownCameraAngle, 360f);

        // 水槽の中心 (0,0,0) を基準にカメラの位置を計算
        Vector3 tankCenter = Vector3.zero; // 水槽の中心
        float cameraHeight = topDownCamera.transform.position.y; // カメラの現在の高さを維持

        // 新しい位置を計算
        // Quaternion.Euler(0, currentTopDownCameraAngle, 0) でY軸周りの回転を作成
        // その回転をVector3.forwardに適用し、topDownCameraDistanceで距離を調整
        Vector3 offset = Quaternion.Euler(0, currentTopDownCameraAngle, 0) * Vector3.forward * topDownCameraDistance;
        topDownCamera.transform.position = new Vector3(tankCenter.x + offset.x, cameraHeight, tankCenter.z + offset.z);

        // カメラが水槽の中心を見るように回転を設定
        topDownCamera.transform.LookAt(tankCenter);
    }

    public void SetExecutionMode(int modeIndex)
    {
        currentMode = (ExecutionMode)modeIndex;

        // CPUモードのみをサポートするため、常にtrue
        obstacleController.enabled = true;
        obstacleController.SetMode(true); 

        // スライダーの最大値はUIで設定されるため、ここでは変更しない
        // fishCountSlider.maxValue = (currentMode == ExecutionMode.CPU) ? 200 : 10000;
        UpdateFishCountText();
    }

    void UpdateFishCountText()
    {
        fishCountText.text = $"Fish Count: {(int)fishCountSlider.value}";
    }

    public void SwitchState(GameState newState)
    {
        currentState = newState;

        bool isSetup = (newState == GameState.Setup);
        bool isSimulating = (newState == GameState.Simulating);
        bool isSubmarine = (newState == GameState.Submarine);

        obstacleController.enabled = isSetup || isSimulating || isSubmarine;
        submarineController.enabled = isSubmarine;
        interactionController.enabled = isSimulating || isSubmarine;
        boidManager.enabled = isSimulating || isSubmarine; // BoidManagerをシミュレーション中または潜水モード中に有効にする

        topDownCamera.enabled = !isSubmarine;
        submarineCamera.enabled = isSubmarine;

        uiManager.UpdateUIVisibility(newState);
    }

    public void OnStartSimulationButtonPressed()
    {
        fishCount = (int)fishCountSlider.value;

        if (obstacleController != null)
        {
            obstacleController.StopPlacementAndClearPreviews();
        }

        // Spawner を使用して Boid を生成
        List<Boid> spawnedBoids = spawner.SpawnBoids(fishCount, boidPrefab, boidColor, boidSettings);
        // 生成された Boid を BoidManager に渡す
        boidManager.SetBoids(spawnedBoids);

        SwitchState(GameState.Simulating);
    }

    public void ToggleSubmarineMode()
    {
        if (currentState == GameState.Simulating || currentState == GameState.Submarine)
        {
            SwitchState(currentState == GameState.Simulating ? GameState.Submarine : GameState.Simulating);
        }
    }

    public void ResetSimulation()
    {
        // すべてのBoidを削除
        if (boidManager != null) {
            // BoidManagerが管理しているBoidのリストを取得し、それぞれをDestroy
            // BoidManagerにBoidをクリアするメソッドを追加する必要がある
            boidManager.ClearBoids();
        }

        // 餌をすべて削除
        GameObject[] foodObjects = GameObject.FindGameObjectsWithTag("Food");
        foreach (GameObject food in foodObjects)
        {
            Destroy(food);
        }

        // 障害物プレビューをクリア
        if (obstacleController != null) {
            obstacleController.StopPlacementAndClearPreviews();
        }

        // 状態をSETUPに戻す
        SwitchState(GameState.Setup);
    }

    public Vector3 GetCurrentBounds()
    {
        // BoidManager または BoidSettings に境界の定義がある場合、それを使用する
        // 現時点ではプレースホルダーの値を返す
        return Vector3.one * 50; 
    }
}
