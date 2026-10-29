using System.Collections;
using UnityEngine;

public class HorizonCurveController : MonoBehaviour
{
    // Smaller values = slower change. 
    // 0.00001f means it takes roughly 25 seconds to move from 0 to 0.00025f
    [SerializeField] private float changeSpeed = 0.00001f;

    private float[] curveAmount = { 0.00025f, 0f, -0.00025f };
    private static readonly int SideCurveID = Shader.PropertyToID("_SidewaysStrength");

    private float currentCurveValue = 0f;
    private float targetCurveValue = 0f;

    void Start()
    {
        StartCoroutine(UpdateTargetCO());
        StartCoroutine(ApplyCurveCO());
    }

    // Pick a new target every 20-30 seconds
    IEnumerator UpdateTargetCO()
    {
        while (true)
        {
            targetCurveValue = curveAmount[Random.Range(0, curveAmount.Length)];
            yield return new WaitForSeconds(Random.Range(20f, 30f));
        }
    }

    // Constantly "crawl" toward that target every frame
    IEnumerator ApplyCurveCO()
    {
        while (true)
        {
            currentCurveValue = Mathf.MoveTowards(
                currentCurveValue,
                targetCurveValue,
                changeSpeed * Time.deltaTime
            );

            Shader.SetGlobalFloat(SideCurveID, currentCurveValue);
            yield return null;
        }
    }
}


