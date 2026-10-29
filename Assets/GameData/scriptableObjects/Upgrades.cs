using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade")]
public abstract class Upgrade : ScriptableObject
{
    public Sprite icon { get; private set; }
    public string upgradeName;
    public string description { get; private set; }
    //public ResourceAmount cost = new ResourceAmount(ResourceType.Research, 500);

    //[Button]
    public abstract void DoUpgrade();
}
