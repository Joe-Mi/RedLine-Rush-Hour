using System.Linq;
using NUnit.Framework.Internal.Filters;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;

public class CarSelect : MonoBehaviour
{

    // Stores all the game object cars (player models).
    [Header("Available_Cars")]
    [SerializeField] GameObject[] carPrefabs;

    [Header("Configuration")]
    [SerializeField] private PlayerProfile playerProfile;
    [SerializeField] private string folderPath = "player_objs"; // Path inside the Resources folder
    private string idFilter; // Only items with this string in their name

    // This is virtual cmera used to conorl the main camera inGame. 
    [Header("Camera")]
    [SerializeField]
    CinemachineCamera cinemachineCamera;

    // Indicates which scene is active.
    [Header("Scene_Status")]
    [SerializeField] bool IsMainMenu = false;

    // These varibales, store a clone of the slected prefab.
    GameObject instatiatedPlayerCar = null;
    int carIndex = 1;

    static GameObject SelectedCarPrefab = null;
    public GameObject currentCarPrefab => SelectedCarPrefab;

    // Records the car mesh rotation.
    quaternion carRotation = Quaternion.identity;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //populateGarage();
        if (IsMainMenu) {
            instatiatedPlayerCar = Instantiate(carPrefabs[carIndex].GetComponent<CarController>().CarMeshRenderer.gameObject);

            SelectedCarPrefab = carPrefabs[carIndex];

            Debug.Log($"cinemachinecamera working: n.o: {carPrefabs.Length} and index: {carIndex}");
        } else {
            if (SelectedCarPrefab != null)
            {
               instatiatedPlayerCar = Instantiate(SelectedCarPrefab);
            } else {
               instatiatedPlayerCar = Instantiate(carPrefabs[0]);
            }
        }

        if (cinemachineCamera != null)
        {
            cinemachineCamera.Follow = instatiatedPlayerCar.transform;
        }
    }

    void Update()
    {
        if (IsMainMenu)
        {
            instatiatedPlayerCar.transform.Rotate(new Vector3(0, 20, 0) * Time.deltaTime);
            carRotation = instatiatedPlayerCar.transform.rotation;
        }
    }

    void ChangeCar() {
        Destroy( instatiatedPlayerCar );

        instatiatedPlayerCar = Instantiate(carPrefabs[carIndex].GetComponent<CarController>().CarMeshRenderer.gameObject);


        SelectedCarPrefab = carPrefabs[carIndex];

        instatiatedPlayerCar.transform.rotation = carRotation;
    }

    public void OnNextCar() {
        carIndex++;

        if ( carIndex > carPrefabs.Length - 1) {
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
