using System.Collections.Generic;
using UnityEngine;

public class PlayerProgressSystem : MonoBehaviour
{
    [SerializeField] private PlayerProfile playerProfile;
    [SerializeField] private ObjectiveSystem objectiveSystem;

    // Hook this up to your end-of-run UI
    public System.Action<int, int, bool, int> OnRunRewarded;
    // params: xpGained, goldGained, didLevelUp, newLevel

    // Call this from GameManager when the run ends (lives exhausted)
    public void OnRunComplete(float finalScore, float peakSpeed)
    {
        // Update top score and top speed on profile
        bool newTopScore = false;
        if (finalScore > playerProfile.topScore)
        {
            playerProfile.topScore = finalScore;
            newTopScore = true;
        }

        if (peakSpeed > playerProfile.topSpeed)
            playerProfile.topSpeed = peakSpeed;

        // Collect completed objectives and pay out rewards
        List<Objective> completed = objectiveSystem.GetCompletedObjectives();

        int totalXP = 0;
        int totalGold = 0;

        foreach (var obj in completed)
        {
            totalXP += obj.xpReward;
            totalGold += obj.goldReward;
        }

        // Bonus XP for new top score
        if (newTopScore)
            totalXP += 50;

        // Apply rewards
        playerProfile.gold += totalGold;
        bool levelledUp = AddXP(totalXP);

        // Notify UI
        OnRunRewarded?.Invoke(totalXP, totalGold, levelledUp, playerProfile.level);

        // Reset for next run
        objectiveSystem.ResetRunTracking();

        // If levelled up, generate fresh objectives for new level
        if (levelledUp)
            objectiveSystem.GenerateObjectives(playerProfile.level);
    }

    private bool AddXP(int amount)
    {
        playerProfile.xp += amount;
        bool levelledUp = false;

        // Handle multi-level jumps
        while (playerProfile.xp >= playerProfile.xpToNextLevel)
        {
            playerProfile.xp -= playerProfile.xpToNextLevel;
            playerProfile.level++;
            playerProfile.RecalculateXPThreshold();
            levelledUp = true;
            Debug.Log($"Level up! Now level {playerProfile.level}");
        }

        return levelledUp;
    }
}
