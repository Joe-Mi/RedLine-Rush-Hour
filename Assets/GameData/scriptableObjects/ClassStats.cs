using System.Collections.Generic;
using UnityEngine;

public enum StatType { 
    topSpeed ,
    acceleration,
    handling,
    braking
};

[CreateAssetMenu(fileName = "Stats", menuName = "Scriptable Objects/ Stats")]
public class Stats : ScriptableObject
{
    [Header("Car Details")]
    public string title = "placeHolder";
    public string id = "00001";

    [Header("Engine Config")]
    public float topSpeed = 150f;
    public float acceleration = 10f;

    [Header("Handling Config")]
    public float handling = 5f;
    public float braking = 25f;

}
