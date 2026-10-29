using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum MenuPage { Main, Garage, Store, Upgrade }

[DefaultExecutionOrder(-5)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Key Variables")]
    [SerializeField] private PlayerProfile playerProfile;
    public CarCatalog carCatalog;

    // Main Menu variable and arrays.
    public GameObject SelectedCar { get; private set; }
    public event Action<MenuPage> OnPageChanged;

    [Header("InGame Systems & Variables")]
    [SerializeField] private PlayerProgressSystem progressSystem;
    [SerializeField] public ObjectiveSystem objectiveSystem;

    public bool IsGame { get; private set; } = false;
    public bool IsGameOver { get; private set; } = false;
    public bool Ispause { get; private set; } = false;

    // Events UI listens to
    public System.Action OnRunStarted;
    public System.Action<float> OnRunEnded; // passes final score

    private CarController subscribedCar;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        carCatalog = new CarCatalog(playerProfile);

        if (SelectedCar == null)
        {
            var owned = carCatalog.GetOwned();
            if (owned.Length > 0) SelectedCar = owned[0];
        }
    }

    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Always clean up the old subscription (the old car is destroyed anyway)
        UnsubscribeFromCar();

        // Reset per-run state every time a scene loads
        Time.timeScale = 1f;
        Ispause = false;
        IsGameOver = false;

        if (scene.name == "Stage")
        {
            IsGame = true;

            //GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            //if (playerObj != null && playerObj.TryGetComponent(out CarController car))
            //{
            //    subscribedCar = car;
            //    subscribedCar.OnPlayerDown += HandlePlayerDown;
            //}
            //else
            //{
            //    Debug.LogError("GameManager: No Player/CarController found in Stage scene.");
            //}

            OnRunStarted?.Invoke();
        }
        else
        {
            IsGame = false;
        }
    }

    public void RegisterCar(CarController car)
    {
        UnsubscribeFromCar();
        subscribedCar = car;
        subscribedCar.OnPlayerDown += HandlePlayerDown;
    }

    // Collects Stats from all cars in the garage
    private List<Stats> GetAllCarStats()
    {
        var statsList = new List<Stats>();
        foreach (GameObject car in playerProfile.garage)
        {
            if (car == null) continue;
            Stats s = car.GetComponent<CarController>()?.CarData;
            if (s != null) statsList.Add(s);
        }
        return statsList;
    }

    public void HandlePlayerDown(CarController car)
    {
        if (IsGameOver) return;
        IsGameOver = true;

        float finalScore = car.Score;
        float peakSpeed = objectiveSystem.peakSpeedThisRun;

        // Reward the player
        progressSystem.OnRunComplete(finalScore, peakSpeed);

        // Notify UI
        OnRunEnded?.Invoke(finalScore);
    }

    void UnsubscribeFromCar()
    {
        if (subscribedCar != null)
            subscribedCar.OnPlayerDown -= HandlePlayerDown;
        subscribedCar = null;
    }

    public void StartGame()
    {
        SaveSystem.Load(playerProfile, GetAllCarStats());
        SceneManager.LoadScene("Stage"); // sceneLoaded handles the rest
    }

    public void RestartRun()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        SaveSystem.Save(playerProfile, GetAllCarStats());
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main_menu");
    }

    //Main menu UI fucntion and calls.
    public void SetSelectedCar(GameObject car) => SelectedCar = car;

    public void GoToPage(MenuPage page) => OnPageChanged?.Invoke(page);

    //Ingame Resume and Pause.
    public void PauseRun()
    {
        Ispause = true;
        Time.timeScale = 1f;
    }

    public void ResumeRun()
    {
        Ispause = false;
        if (!IsGameOver)
        {
            Time.timeScale = 1f;
        }
    }
}
