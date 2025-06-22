using System.Collections;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    public static GameUIController Instance;

    [SerializeField] private TextMeshProUGUI _gemsCollectedText;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private Button _inGamePauseButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private Button _pauseResumeButton;
    [SerializeField] private Button _pauseQuitButton;
    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _fpsButton;
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private GameObject fpsCanvas;
    [SerializeField] private TMP_Text _levelText;


    private bool _isPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Warning: Multiple instances of GameUIController detected. Destroying new instance", Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        UpdateGemsCollectedText(0);
    }

    private void OnEnable()
    {
        _quitButton.onClick.AddListener(OnQuitButton);
        _pauseResumeButton.onClick.AddListener(OnResumeButton);
        _pauseQuitButton.onClick.AddListener(OnQuitButton);
        _inGamePauseButton.onClick.AddListener(OnInGamePauseButton);
        _newGameButton.onClick.AddListener(OnNewGameButton);
        _fpsButton.onClick.AddListener(OnFpsButton);
    }

    private void OnDisable()
    {
        _quitButton.onClick.RemoveAllListeners();
        _pauseResumeButton.onClick.RemoveAllListeners();
        _pauseQuitButton.onClick.RemoveAllListeners();
        _inGamePauseButton.onClick.RemoveAllListeners();
        _newGameButton.onClick.RemoveAllListeners();
        _fpsButton.onClick.RemoveAllListeners();
    }

    private void Start()
    {
        _gameOverPanel.SetActive(false);
        _pausePanel.SetActive(false);
        fpsCanvas.SetActive(false);
    }

    public void UpdateHealthBar(float value)
    {
        _healthSlider.value = value;
    }

    public void UpdateLevelText(float value)
    {
        _levelText.text = $"Level {value}";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleGamePause();
        }
    }

    private void ToggleGamePause()
    {
        _isPaused = !_isPaused;
        _pausePanel.SetActive(_isPaused);
        SetEcsEnabled(!_isPaused);
    }

    private void SetEcsEnabled(bool shouldEnable)
    {
        var defaultWorld = World.DefaultGameObjectInjectionWorld;
        if (defaultWorld == null) return;

        var initializationSystemGroup = defaultWorld.GetExistingSystemManaged<InitializationSystemGroup>();
        if (initializationSystemGroup != null)
            initializationSystemGroup.Enabled = shouldEnable;

        var simulationSystemGroup = defaultWorld.GetExistingSystemManaged<SimulationSystemGroup>();
        if (simulationSystemGroup != null)
            simulationSystemGroup.Enabled = shouldEnable;
    }

    public void UpdateGemsCollectedText(int gemsCollected)
    {
        _gemsCollectedText.text = $"{gemsCollected:N0}";
    }

    public void ShowGameOverUI()
    {
        StartCoroutine(ShowGameOverUICoroutine());
    }

    private IEnumerator ShowGameOverUICoroutine()
    {
        yield return new WaitForSeconds(1.5f);
        _gameOverPanel.SetActive(true);
    }

    private void OnResumeButton()
    {
        ToggleGamePause();
    }

    private void OnQuitButton()
    {
        SetEcsEnabled(true); // ECS sistemlerini tekrar aktif et
        SceneManager.LoadScene(0); // Ana sahneye dönüþ
    }

    private void OnInGamePauseButton()
    {
        ToggleGamePause();
    }

    private void OnNewGameButton()
    {
        SetEcsEnabled(true); // ECS sistemlerini tekrar aktif et
        SceneManager.LoadScene(1); // Ana sahneye dönüþ
    }

    private void OnFpsButton()
    {
        if (fpsCanvas.activeSelf)
        {
            fpsCanvas.SetActive(false);
        }
        else
        {
            fpsCanvas.SetActive(true);
        }
    }
}
