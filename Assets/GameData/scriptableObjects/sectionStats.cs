using System.Collections.Generic;
using UnityEngine;


public enum direction
{
    left,
    right,
    up,
    down
};

[CreateAssetMenu(fileName = "SectionStat", menuName = "Scriptable Objects/ Section_Stat")]
public class SectionStat : ScriptableObject
{
    [Header("Car Details")]
    public int group = 1;
    public int id = 1;

    [Header("size Config")]
    public float length = 300f;

    [Header("ports Config")]
    public direction exit = direction.up;
    public direction entries = direction.down;

}
