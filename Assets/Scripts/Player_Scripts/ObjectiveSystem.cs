using System.Collections.Generic;
using UnityEngine;

public class ObjectiveSystem : MonoBehaviour
{
    [SerializeField] private PlayerProfile playerProfile;

    public List<Objective> activeObjectives = new List<Objective>();

    // Live run tracking
    public int perfectUpshiftsThisRun = 0;
    public float distanceThisRun = 0f;
    public float peakSpeedThisRun = 0f;
    public float scoreThisRun = 0f;

    // Events — hook these up from CarController / GameManager
    public System.Action<List<Objective>> OnObjectivesCompleted;

    void Start()
    {
        GenerateObjectives(playerProfile.level);
    }

    // Generates 3 objectives scaled to the players current level
    public void GenerateObjectives(int level)
    {
        activeObjectives.Clear();

        float scale = 1f + (level - 1) * 0.25f; // 25% harder per level
        int xpBase = 50 + (level * 25);
        int goldBase = 20 + (level * 10);

        activeObjectives.Add(new Objective(
            $"Reach {80 + level * 10} km/h",
            ObjectiveType.ReachTopSpeed,
            80f + level * 10f,
            xpBase, goldBase
        ));

        activeObjectives.Add(new Objective(
            $"Land {2 + level} perfect upshifts",
            ObjectiveType.PerfectUpshifts,
            2f + level,
            xpBase + 25, goldBase + 10
        ));

        activeObjectives.Add(new Objective(
            $"Score {Mathf.RoundToInt(500 * scale)} points",
            ObjectiveType.ScoreTarget,
            500f * scale,
            xpBase + 50, goldBase + 20
        ));


        //int i = 0;
        //while (i <= 3) {



        //    i++;
        //}
    }

    // --- Called by CarController during a run ---

    public void ReportSpeed(float speed)
    {
        peakSpeedThisRun = Mathf.Max(peakSpeedThisRun, speed);
        UpdateObjectivesOfType(ObjectiveType.ReachTopSpeed, peakSpeedThisRun);
    }

    public void ReportPerfectUpshift()
    {
        perfectUpshiftsThisRun++;
        AddProgressToObjectivesOfType(ObjectiveType.PerfectUpshifts, 1f);
    }

    public void ReportDistance(float delta)
    {
        distanceThisRun = delta;
        AddProgressToObjectivesOfType(ObjectiveType.DistanceTravelled, delta);
    }

    public void ReportScore(float score)
    {
        scoreThisRun = score;
        UpdateObjectivesOfType(ObjectiveType.ScoreTarget, score);
    }

    // --- End of run ---

    public List<Objective> GetCompletedObjectives()
    {
        return activeObjectives.FindAll(o => o.isCompleted);
    }

    public void ResetRunTracking()
    {
        perfectUpshiftsThisRun = 0;
        distanceThisRun = 0f;
        peakSpeedThisRun = 0f;
        scoreThisRun = 0f;
    }

    // --- Internals ---

    private void UpdateObjectivesOfType(ObjectiveType type, float value)
    {
        foreach (var obj in activeObjectives)
            if (obj.type == type) obj.UpdateProgress(value);
    }

    private void AddProgressToObjectivesOfType(ObjectiveType type, float value)
    {
        foreach (var obj in activeObjectives)
            if (obj.type == type) obj.AddProgress(value);
    }
}
