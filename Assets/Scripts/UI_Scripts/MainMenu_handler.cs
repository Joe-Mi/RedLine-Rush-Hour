using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu_Handler : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField] private List<GameObject> pages = new List<GameObject>();

    [Header("Profile Display")]
    [SerializeField] private PlayerProfile playerProfile;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI topScoreText;

    void Start()
    {
        GoTo(MenuPage.Main);
        RefreshProfileDisplay();
    }

    private void RefreshProfileDisplay()
    {
        if (playerProfile == null) return;
        playerNameText.text = playerProfile.playerName;
        levelText.text = $"Level {playerProfile.level}";
        goldText.text = $"$ {playerProfile.gold.ToString()}";
        topScoreText.text = playerProfile.topScore.ToString("0000");
    }

    public void GoTo(MenuPage page)
    {
        for (int i = 0; i < pages.Count; i++)
            pages[i].SetActive(i == (int)page);

        GameManager.Instance.GoToPage(page);
    }

    // Named navigation — cleaner than magic index numbers in the Inspector


    public void initStage() => GameManager.Instance.StartGame();
    public void ToMainPage() => GoTo(MenuPage.Main);

    public void ToUpgradePage() => GoTo(MenuPage.Upgrade);
    public void ToGaragePage() => GoTo(MenuPage.Garage);

    public void ToStorePage() => GoTo(MenuPage.Store);
}
