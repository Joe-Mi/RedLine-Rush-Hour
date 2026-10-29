using UnityEngine;

public class UpgradeSystem : MonoBehaviour
{

    public void Upgrade(int statIndex, Stats car ){
        if (car == null) Debug.Log($"{car} is null");

        if (statIndex == 0) {
            car.topSpeed = car.topSpeed + 20f;
        } else if(statIndex == 1 ){
            car.acceleration = car.acceleration + 5f;
        } else if (statIndex == 2 ){
            car.handling = car.handling + 8f;
        } else if (statIndex == 3 ){
            car.braking = car.braking + 5f;
        } else
        {
            Debug.Log($"{statIndex} is out of bounds");
        }
    }

}
