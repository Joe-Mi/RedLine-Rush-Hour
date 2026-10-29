using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;

public static class SaveSystem
{
    // --- Serializable data containers (pure data, no Unity object refs) ---

    [Serializable]
    public class PlayerSaveData
    {
        public string playerName;
        public int level;
        public int xp;
        public int xpToNextLevel;
        public int gold;
        public float topScore;
        public float topSpeed;
        public List<string> garageCarIDs = new List<string>(); // car prefab IDs, not GameObjects
    }

    [Serializable]
    public class CarSaveData
    {
        public string carID; // matches a prefab name or key in your car registry
        public float topSpeed;
        public float acceleration;
        public float handling;
        public float braking;
    }

    [Serializable]
    private class SaveFile
    {
        public PlayerSaveData player;
        public List<CarSaveData> cars = new List<CarSaveData>();
    }

    // --- File path ---

    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, "Save.save");

    // --- Public API ---

    public static void Save(PlayerProfile profile, List<Stats> carStats)
    {
        SaveFile file = new SaveFile
        {
            player = ExtractPlayerData(profile),
            cars = ExtractCarData(carStats)
        };

        File.WriteAllText(SavePath, JsonUtility.ToJson(file, true));
        Debug.Log($"Saved to: {SavePath}");
    }

    public static bool Load(PlayerProfile profile, List<Stats> carStats)
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No save file found — using defaults.");
            return false;
        }

        SaveFile file = JsonUtility.FromJson<SaveFile>(File.ReadAllText(SavePath));

        ApplyPlayerData(file.player, profile);
        ApplyCarData(file.cars, carStats);

        Debug.Log("Save loaded.");
        return true;
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    // --- Extractors: runtime objects → plain data ---

    private static PlayerSaveData ExtractPlayerData(PlayerProfile profile)
    {
        var data = new PlayerSaveData
        {
            playerName = profile.playerName,
            level = profile.level,
            xp = profile.xp,
            xpToNextLevel = profile.xpToNextLevel,
            gold = profile.gold,
            topScore = profile.topScore,
            topSpeed = profile.topSpeed,
        };

        // Save car prefab names, not GameObject references
        foreach (GameObject car in profile.garage)
            if (car != null)
                data.garageCarIDs.Add(car.name);

        return data;
    }

    private static List<CarSaveData> ExtractCarData(List<Stats> allCarStats)
    {
        var list = new List<CarSaveData>();
        foreach (Stats s in allCarStats)
        {
            list.Add(new CarSaveData
            {
                carID = s.id,
                topSpeed = s.topSpeed,
                acceleration = s.acceleration,
                handling = s.handling,
                braking = s.braking
            });
        }
        return list;
    }

    // --- Appliers: plain data → runtime objects ---

    private static void ApplyPlayerData(PlayerSaveData data, PlayerProfile profile)
    {
        profile.playerName = data.playerName;
        profile.level = data.level;
        profile.xp = data.xp;
        profile.xpToNextLevel = data.xpToNextLevel;
        profile.gold = data.gold;
        profile.topScore = data.topScore;
        profile.topSpeed = data.topSpeed;
        // Garage GameObjects are restored by GameManager via CarRegistry
    }

    private static void ApplyCarData(List<CarSaveData> savedCars, List<Stats> allCarStats)
    {
        foreach (CarSaveData saved in savedCars)
        {
            Stats match = allCarStats.Find(s => s.id == saved.carID);
            if (match == null) continue;

            match.topSpeed = saved.topSpeed;
            match.acceleration = saved.acceleration;
            match.handling = saved.handling;
            match.braking = saved.braking;
        }
    }
}
