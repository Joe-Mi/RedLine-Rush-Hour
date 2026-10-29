using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;

public class AICarSpawner : MonoBehaviour
{
    Transform playerTransform;
    [SerializeField] GameObject[] AIcarPrefabs;
    [SerializeField] GameObject[] AIcarPrefabPool = new GameObject[20];

    Queue<int[]> spawnPoints = new Queue<int[]>();

    int difficulty = 1;
    public CarController carController;

    //overlap check.
    [SerializeField] LayerMask otherCarLayerMask;
    Collider[] overLapCheckCollider = new Collider[1];

    WaitForSeconds wait = new WaitForSeconds(0.25f);
    WaitForSeconds spawnWait = new WaitForSeconds(3f);

    //timing how frequently we spawn cars.
    float lastTimeSpawnedCar = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int PrefabIndex = 0;
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        carController = playerTransform.GetComponent<CarController>();

        for (int i = 0; i < AIcarPrefabPool.Length; i++) {
            AIcarPrefabPool[i] = Instantiate(AIcarPrefabs[PrefabIndex]);
            AIcarPrefabPool[i].SetActive(false);

            PrefabIndex++;

            if (PrefabIndex > AIcarPrefabs.Length - 1)
            { 
                PrefabIndex = 0;
            }
        }

        StartCoroutine(UpdateLessOftenCO());
        if(spawnPoints.Count >= 4)
        {
            StartCoroutine(SpawnLessOftenCO());
        }
        
    }

    IEnumerator UpdateLessOftenCO() 
    {
        while (true) {
            if (carController.Score >= 250 ) { 
              difficulty = 2;
            };

            spawnPositions();
            cleanUpCars();

            yield return wait;
        }
    }

    IEnumerator SpawnLessOftenCO()
    {
        while (true)
        {
            Spawn();

            yield return spawnWait;
        }
    }


    void spawnPositions()
    {
        // Define patterns as 0/1 matrices
        int[,] pattern1 = {
            {1, 1, 0, 1},
            {0, 0, 0, 0},
            {0, 0, 1, 0},
            {0, 0, 0, 0}
        };

        int[,] pattern2 = {
            {1, 1, 0, 0},
            {0, 0, 0, 0},
            {0, 0, 1, 1},
            {0, 0, 0, 0}
        };

        int[,] pattern3 = {
            {0, 1, 0, 0},
            {0, 0, 0, 0},
            {1, 0, 1, 1},
            {0, 0, 0, 0}
        };

        int[,] pattern4 = {
            {0, 1, 0, 1},
            {0, 0, 0, 0},
            {1, 0, 1, 0},
            {0, 0, 0, 0}
        };

        int[][,] patterns = { pattern1, pattern2, pattern3, pattern4 };

        int randomIndex = UnityEngine.Random.Range(0, patterns.Length);
        int[,] chosenPattern = patterns[randomIndex];

        // Convert matrix entries into world positions
        for (int row = 0; row < 4; row++)
        {
            for (int lane = 0; lane < Utils.CarLanes.Length; lane++)
            {
                if (chosenPattern[row, lane] == 1)
                {
                    int[] pos = { lane, row };
                    spawnPoints.Enqueue(pos);
                }
            }
        }
    }

    void SpawnNewCars()
    {
        //if (Time.time - lastTimeSpawnedCar < 0.5) return;
        if (spawnPoints.Count == 0) return;

        GameObject carToSpawn = null;
        Rigidbody rb = null;

        foreach (GameObject go in AIcarPrefabPool)
        {
            if (!go.activeInHierarchy)
            {
                carToSpawn = go;
                break;
            }
        }
        if (carToSpawn == null) return;

        int[] Position = spawnPoints.Dequeue();
        float laneX = Utils.CarLanes[Position[0]];
        Vector3 SpawnPosition = new Vector3(0, 0, 0);

        SpawnPosition = new Vector3(laneX, 1.8f, playerTransform.position.z + 150 + Position[1] * 25f);
        //Debug.Log($"lane : {laneX}, " +
        //  $"player position z: {playerTransform.position.z}, " +
        //  $"spawn position z: {playerTransform.position.z + 150 + Position[1] * 12.5f}");

        Vector3 halfExtents = new Vector3(1.5f, 2.5f, 5f);
        Debug.Log($"this is the current lvl = {difficulty}");

        if (Physics.OverlapBoxNonAlloc(
            SpawnPosition,
            halfExtents,
            overLapCheckCollider,
            Quaternion.identity,
            otherCarLayerMask
        ) > 0)
        {
            return;
        }

        
        carToSpawn.transform.position = SpawnPosition;
        
        rb = carToSpawn.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.ResetInertiaTensor();

        }
        else {
            Debug.Log("no ai rigid boy to clear");
        }

        carToSpawn.SetActive(true);
        lastTimeSpawnedCar = Time.time;
    }

    void Spawn()
    {
        int n = 0;
        foreach (GameObject go in AIcarPrefabPool) 
        {
            if (!go.activeInHierarchy)
            { n++; }
        }
        if (n >= 6)
        {
            Debug.Log($"Spawn run");
            int i = 0;
            for (i = 0; i < 4; i++)
            {
                SpawnNewCars();
            }
            return;
        }
    }

    void cleanUpCars() {
        foreach (GameObject go in AIcarPrefabPool)
        {
            if (!go.activeInHierarchy)
                { continue; }

            if (go.transform.position.z - playerTransform.position.z > 350)
                go.SetActive(false);

            if (go.transform.position.z - playerTransform.position.z < -10)
                go.SetActive(false);

        }
    }
}
