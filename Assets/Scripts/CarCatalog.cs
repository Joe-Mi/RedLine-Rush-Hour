using System;
using System.Linq;
using UnityEngine;

public class CarCatalog
{
    private readonly PlayerProfile profile;
    private GameObject[] allCars;

    public CarCatalog(PlayerProfile profile)
    {
        this.profile = profile;
        allCars = Resources.LoadAll<GameObject>("player_objs");
    }

    public GameObject[] GetOwned() => Filter(owned: true);
    public GameObject[] GetUnowned() => Filter(owned: false);

    private GameObject[] Filter(bool owned)
    {
        var ownedIds = profile.garage
            .Select(c => c.GetComponent<CarController>().CarData.id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return allCars.Where(prefab =>
        {
            var controller = prefab.GetComponent<CarController>();
            if (controller == null) return !owned; // keep unowned-side default from your original code
            bool isOwned = ownedIds.Contains(controller.CarData.id);
            return isOwned == owned;
        }).ToArray();
    }
}
