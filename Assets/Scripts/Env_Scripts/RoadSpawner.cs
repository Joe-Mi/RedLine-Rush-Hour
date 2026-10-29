using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RoadSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] sectionsPrefabs;
    [SerializeField] private int poolSize = 20;
    [SerializeField] private int activeSections = 10;
    [SerializeField] private float sectionLength = 300f;

    private List<GameObject> sectionsPool = new List<GameObject>();
    private List<GameObject> activeList = new List<GameObject>();

    private Transform playerCarTransform;
    [SerializeField] private HorizonCurveController curveController;

    private WaitForSeconds waitFor100ms = new WaitForSeconds(0.1f);
    private float nextSpawnZ = 0f;

    void Start()
    {
        GameObject playerCar = GameObject.FindGameObjectWithTag("Player");
        playerCarTransform = playerCar.transform;

        // Builds a pool of sections
        for (int i = 0; i < poolSize; i++)
        {
            GameObject prefab = sectionsPrefabs[i % sectionsPrefabs.Length];
            GameObject section = Instantiate(prefab);
            section.SetActive(false);
            sectionsPool.Add(section);
        }

        // Activate initial sections
        for (int i = 0; i < activeSections; i++)
        {
            GameObject section = GetInactiveSection();
            section.transform.position = new Vector3(0, 0, nextSpawnZ);
            section.SetActive(true);
            activeList.Add(section);

            nextSpawnZ += sectionLength; // advance spawn position
        }

        StartCoroutine(UpdateLessOftenCO());
    }

    IEnumerator UpdateLessOftenCO()
    {
        while (true)
        {
            UpdateSectionPositions();
            yield return waitFor100ms;
        }
    }


    void UpdateSectionPositions()
    {
        for (int i = 0; i < activeList.Count; i++)
        {
            if (activeList[i].transform.position.z - playerCarTransform.position.z < -sectionLength)
            {
                GameObject oldSection = activeList[i];
                oldSection.SetActive(false);

                // 2. Safely grab the corresponding inactive section matching the updated ID
                GameObject newSection = GetInactiveSection();
                float Length = newSection.GetComponent<sectionHandler>().stats.length;
                Debug.Log(sectionLength);

                float distanceAhead = nextSpawnZ - playerCarTransform.position.z;

                newSection.transform.position = new Vector3(0, 0, nextSpawnZ);
                newSection.SetActive(true);

                activeList[i] = newSection;
                nextSpawnZ += sectionLength;
            }
        }
    }

    GameObject GetInactiveSection()
    {
        foreach (var sec in sectionsPool)
        {

            if (!sec.activeInHierarchy) return sec;
        }
        // Fallback: reuse first
        return sectionsPool[0];
    }

}



