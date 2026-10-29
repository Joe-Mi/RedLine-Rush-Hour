using UnityEngine;
using Unity.Cinemachine;
using Unity.Mathematics;

public class CarSpawner : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] CinemachineCamera cinemachineCamera;
    GameObject instatiatedPlayerCar = null;
    static GameObject SelectedCarPrefab = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SelectedCarPrefab = GameManager.Instance.SelectedCar;

        if (SelectedCarPrefab != null)
        {
            instatiatedPlayerCar = Instantiate(SelectedCarPrefab);
        } else {
            Debug.Log("SelectedCarPrefab == null");
        }

        if (cinemachineCamera != null)
        {
            cinemachineCamera.Follow = instatiatedPlayerCar.transform;
            Debug.Log($"cinemachinecamera working");
        }
    }
}
