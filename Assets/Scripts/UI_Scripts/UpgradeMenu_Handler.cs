using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeMenu_Handler : MonoBehaviour
{
    [SerializeField] public UpgradeSystem upgradeSystem;
    [SerializeField] private TextMeshProUGUI CarTitleText;
    [SerializeField] public Slider[] StatBars;

    private GameObject selectedCar;
    private Stats carStats;
    private int ActiveIndex = 0;

    void Start()
    {      
        RefreshSelectedCar();
        RefreshStatBars();
        SetActive(ActiveIndex);
    }

    // Call this if the player switches cars
    public void RefreshSelectedCar()
    {
        selectedCar = GameManager.Instance.SelectedCar;
        CarController controller = selectedCar.GetComponent<CarController>();

        if (controller == null)
        {
            Debug.LogWarning("CarController not found on selected car.");
            return;
        }

        carStats = controller.CarData;
        CarTitleText.text = carStats.title;
    }

    // Only call when stats actually change, not every frame
    public void RefreshStatBars()
    {
        if (carStats == null || StatBars.Length < 4) return;

        StatBars[0].value = carStats.topSpeed;
        StatBars[1].value = carStats.acceleration;
        StatBars[2].value = carStats.handling;
        StatBars[3].value = carStats.braking;
    }

    public void SetActive(int TargetIndex)
    {
        if (StatBars.Length < 4) return;

        ActiveIndex = TargetIndex;
        Debug.Log($"ActiveIndex: {ActiveIndex}");

        for (int i = 0; i < StatBars.Length; i++)
        {
            // Target the fill rect, not the root Slider Image
            Image fill = StatBars[i].fillRect.GetComponent<Image>();
            if (fill != null)
                fill.color = (i == ActiveIndex) ? Color.green : Color.white;
        }
    }

    public void OnClickUpgrade()
    {
        if (selectedCar == null || carStats == null) return;

        upgradeSystem.Upgrade(ActiveIndex, carStats);

        // Refresh display after upgrade
        RefreshStatBars();
        //SetActive();
    }
}
