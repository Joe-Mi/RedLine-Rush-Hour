using UnityEngine;

public class CarDisplayer : MonoBehaviour
{
    // Stores all the game object cars (player models).
    [Header("Available_Cars")]
    [SerializeField] GameObject[] carPrefabs;

    // These varibales, store a clone of the slected prefab.
    private GameObject instatiatedPlayerCar = null;
    int carIndex = 0;

    void OnEnable()
    {
        if (GameManager.Instance.carCatalog.GetOwned() == null)
        {
            Debug.Log($"carCatalog is null");
        }
        else
        {
            carPrefabs = GameManager.Instance.carCatalog.GetOwned();
        }

        var selectedCar = GameManager.Instance.SelectedCar;
        instatiatedPlayerCar = Instantiate(selectedCar.GetComponent<CarController>().CarMeshRenderer.gameObject);

        Debug.Log($"Script working: n.o: {carPrefabs.Length} and index: {carIndex}");
    }
    void OnDisable()
    {
        if (instatiatedPlayerCar != null) Destroy(instatiatedPlayerCar);
    }

    void ChangeCar()
    {
        if (instatiatedPlayerCar != null) Destroy(instatiatedPlayerCar);

        GameObject carPrefab = carPrefabs[carIndex].GetComponent<CarController>().CarMeshRenderer.gameObject;
        instatiatedPlayerCar = Instantiate(carPrefab);

        GameManager.Instance.SetSelectedCar(carPrefabs[carIndex]);
    }

    public void OnNextCar()
    {
        carIndex++;

        if (carIndex > carPrefabs.Length - 1)
        {
            carIndex = 0;
        }

        ChangeCar();
    }

    public void OnPreviousCar()
    {
        carIndex--;

        if (carIndex < 0)
        {
            carIndex = carPrefabs.Length - 1;
        }

        ChangeCar();
    }
}
