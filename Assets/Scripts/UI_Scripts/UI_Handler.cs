using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

public class UI_Handler : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI gearText;
    [SerializeField] TextMeshProUGUI speedText;
    [SerializeField] Image shiftIndicatorImage;

    [Header("Game Over")]
    [SerializeField] TextMeshProUGUI gameOverText;
    [SerializeField] CanvasGroup gameOverCanvasGroup;

    [Header("Pause")]
    [SerializeField] public GameObject Objective_UIobj;
    [SerializeField] Transform vertical_content;
    [SerializeField] CanvasGroup pauseCanvasGroup;

    public ObjectiveSystem objectiveSystem;
    private CarController playerCar;
    private GearSystem gearSystem;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerCar = player.GetComponent<CarController>();
        gearSystem = player.GetComponent<GearSystem>();

        SetGroup(pauseCanvasGroup, false);
        SetGroup(gameOverCanvasGroup, false);

        dispay_objective();
    }

    void OnEnable()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;
        //gm.OnRunStarted += HandleRunStarted;
        gm.OnRunEnded += HandleRunEnded;
        objectiveSystem = gm.objectiveSystem;
    }
    void OnDisable()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;
        //gm.OnRunStarted -= HandleRunStarted;
        gm.OnRunEnded -= HandleRunEnded;
        objectiveSystem = null;
    }

    void Update()
    {
        // Pure display — no logic
        scoreText.text = playerCar.Score.ToString("000000");
        gearText.text = gearSystem.currentGear.ToString("0");
        speedText.text = playerCar.CurrentVelocity.ToString("000") + " kmh";
        RpmMeter();

        if (gearSystem.shiftUp)
            gearText.color = Color.green;
        else if (gearSystem.shiftDown)
            gearText.color = Color.yellow;
        else
            gearText.color = Color.white;
    }

    public void RpmMeter() {
        float rpms = gearSystem.currentRPM * 0.75f;

        // its a 3 quater donut, so we start at 0.25, and go to 1.0
        shiftIndicatorImage.fillAmount = rpms;
    }

    private void HandleRunEnded(float finalScore)
    {
        gameOverText.text = $"Total Score {finalScore:000000}";
        StartCoroutine(GameOverAnimationCO());
    }

    public void dispay_objective() {
        foreach ( Objective _objective in objectiveSystem.activeObjectives) {
            GameObject newRow = Instantiate(Objective_UIobj, vertical_content);

            Transform textTransform = newRow.transform.Find("Objective_title");
            if (textTransform != null)
            {
                textTransform.GetComponent<TMP_Text>().text = _objective.title;
            }

            Transform rewardtextTransform = newRow.transform.Find("Rewards_text");
            if (rewardtextTransform != null)
            {
                rewardtextTransform.GetComponent<TMP_Text>().text = _objective.goldReward.ToString("000") + "$";
            }

            // 3. Access the Slider by its exact hierarchy name
            Transform sliderTransform = newRow.transform.Find("Objective_progress");
            if (sliderTransform != null)
            {
                sliderTransform.GetComponent<Slider>().maxValue = _objective.target;
                sliderTransform.GetComponent<Slider>().value = _objective.currentProgress;
            }

            // 4. Access the Checkbox (Toggle) by its exact hierarchy name
            Transform toggleTransform = newRow.transform.Find("IsComplete_toggel");
            if (toggleTransform != null)
            {
                toggleTransform.GetComponent<Toggle>().isOn = _objective.isCompleted;
            }
        } 
    }

    public void update_objectives() {
        List<Objective> ObjectiveList = new List<Objective>();
        ObjectiveList = objectiveSystem.activeObjectives;
        int i = 0;

        foreach (Transform row in vertical_content) {

            Transform textTransform = row.Find("Objective_title");
            if (textTransform != null)
            {
                textTransform.GetComponent<TMP_Text>().text = ObjectiveList[i].title;
            }

            Transform rewardtextTransform = row.Find("Rewards_text");
            if (rewardtextTransform != null)
            {
                rewardtextTransform.GetComponent<TMP_Text>().text = ObjectiveList[i].goldReward.ToString("000") + "$";
            }

            // 3. Access the Slider by its exact hierarchy name
            Transform sliderTransform = row.Find("Objective_progress");
            if (sliderTransform != null)
            {
                sliderTransform.GetComponent<Slider>().value = ObjectiveList[i].currentProgress;
            }

            // 4. Access the Checkbox (Toggle) by its exact hierarchy name
            Transform toggleTransform = row.Find("IsComplete_toggel");
            if (toggleTransform != null)
            {
                toggleTransform.GetComponent<Toggle>().isOn = ObjectiveList[i].isCompleted;
            }

            i++;
        }
    }

    void SetGroup(CanvasGroup g, bool visible)
    {
        g.alpha = visible ? 1f : 0f;
        g.interactable = visible;
        g.blocksRaycasts = visible;
    }

    public void Pause()
    {
        GameManager.Instance.PauseRun();
        SetGroup(pauseCanvasGroup, true);
        update_objectives();
    }

    public void Resume()
    {
        GameManager.Instance.ResumeRun();
        SetGroup(pauseCanvasGroup, false);
    }

    IEnumerator GameOverAnimationCO()
    {
        yield return new WaitForSecondsRealtime(3.0f);
        SetGroup(gameOverCanvasGroup, true);
        while (gameOverCanvasGroup.alpha < 0.8f)
        {
            gameOverCanvasGroup.alpha = Mathf.MoveTowards(
                gameOverCanvasGroup.alpha, 1.0f, Time.deltaTime * 2);
            yield return null;
        }
    }

    // Buttons delegate to GameManager — UI never touches SceneManager
    public void OnRestartClicked() => GameManager.Instance.RestartRun();
    public void OnQuitClicked() => GameManager.Instance.GoToMainMenu();
}
