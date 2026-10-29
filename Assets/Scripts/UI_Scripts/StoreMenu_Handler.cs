using System.Linq;
using NUnit.Framework.Internal.Filters;
using UnityEngine;

public class StoreMenu_Handler : MonoBehaviour
{
    [Header("Array of store gameObjects")]
    [SerializeField] GameObject[] featuredCars;

    [Header("Configuration")]
    [SerializeField] PlayerProfile playerProfile;

    [Header("Active Index & Mesh")]
    public int ActiveIndex = 0;
    GameObject displayMesh;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        featuredCars = GameManager.Instance.carCatalog.GetUnowned();

        displayMesh = Instantiate(featuredCars[ActiveIndex].GetComponent<CarController>().CarMeshRenderer.gameObject);

        Debug.Log($"Successfully populated garage array with {featuredCars.Length} objects in garage");
    }
    void OnDisable()
    {
        if (displayMesh != null) Destroy(displayMesh);
    }

    public void DispalyCar(int index) {
        Destroy(displayMesh);

        var meshSource = featuredCars[index].GetComponent<CarController>().CarMeshRenderer.gameObject;
        displayMesh = Instantiate(meshSource);
    }

    public void next() {
        ActiveIndex++;

        if (ActiveIndex > featuredCars.Length - 1)
        {
            ActiveIndex = 0;
        }

        DispalyCar(ActiveIndex);
    }
    public void prev() {
        ActiveIndex--;

        if (ActiveIndex < 0)
        {
            ActiveIndex = 0;
        }

        DispalyCar(ActiveIndex);
    }
    public void purchase() {
        int availableBalance = playerProfile.gold;
        if (availableBalance >= 500)
        {
            //code to ensure the player doesnt already own the car
            var selectedCar = featuredCars[ActiveIndex];

            // 2. Add it to the player's garage 
            playerProfile.garage.Add(selectedCar);

            // 3. Filter out the specific car instance from the array
            featuredCars = featuredCars.Where(val => val != selectedCar).ToArray();

            // 4. Update the balance
            playerProfile.gold = availableBalance - 500;
        }
        else {
            Debug.Log($"Not enough gold to complete Purchase.");
        }
    }
}
