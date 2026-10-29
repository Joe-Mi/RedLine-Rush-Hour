using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerProfile", menuName = "Player/PlayerProfile")]
public class PlayerProfile : ScriptableObject
{
    [Header("Identity")]
    public string playerName;

    [Header("Progress")]
    public int level = 1;
    public int xp = 0;
    public int xpToNextLevel = 100; // scales with level

    [Header("Currency")]
    public int gold = 0;

    [Header("Performance")]
    public float topScore = 0f;
    public float topSpeed = 0f;

    [Header("Active_Car")]
    public string car_id = "";

    [Header("Garage")]
    public List<GameObject> garage = new List<GameObject>();

    // Called when levelling up to scale XP threshold
    public void RecalculateXPThreshold()
    {
        xpToNextLevel = 100 + (level * 50);
    }
}