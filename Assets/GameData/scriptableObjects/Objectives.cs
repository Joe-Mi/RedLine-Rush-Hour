using System;
using UnityEngine;
public enum ObjectiveType
{
    ReachTopSpeed,        // reach X km/h
    PerfectUpshifts,      // land X perfect upshifts in one run
    ScoreTarget,          // reach X score in one run
    DistanceTravelled,    // travel X metres
    WinStreak             // complete X runs without failing
}

[System.Serializable]
public class Objective
{
    public string title;
    public ObjectiveType type;
    public float target;
    public float currentProgress;
    public bool isCompleted;

    // Rewards on completion
    public int xpReward;
    public int goldReward;

    //intiallize an objective instance, using the input values.
    public Objective(string title, ObjectiveType type, float target, int xpReward, int goldReward)
    {
        this.title = title;
        this.type = type;
        this.target = target;
        this.xpReward = xpReward;
        this.goldReward = goldReward;
        currentProgress = 0f;
        isCompleted = false;
    }


    //used to uupdat the obejctive UI element, the progress bar in particular.
    public void UpdateProgress(float value)
    {
        if (isCompleted) return;
        currentProgress = Mathf.Max(currentProgress, value); // for peak-value types
        if (currentProgress >= target)
        {
            isCompleted = true;
            Debug.Log($"Objective complete: {title}");
        }
    }

    // For cumulative types (upshifts, distance) use Add instead
    public void AddProgress(float value)
    {
        if (isCompleted) return;
        currentProgress += value;
        if (currentProgress >= target)
        {
            isCompleted = true;
            Debug.Log($"Objective complete: {title}");
        }
    }
}
