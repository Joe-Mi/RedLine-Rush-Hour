using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GearSystem : MonoBehaviour
{
    [SerializeField] CarController carController;
    [SerializeField] List<TrailRenderer> trailRenderers = new List<TrailRenderer>(); 

    // Gear definitions — each gear has an RPM range and speed range
    readonly float[] gearMaxSpeeds = { 0, 25f, 40f, 55f, 70f, 90f };
    readonly float[] gearAccelMultipliers = { 0, 8.0f, 0.4f, 0.1f, 0.05f, 0.025f };

    public int CurrentGear { get; private set; } = 1;
    public int currentGear => CurrentGear;
    public bool shiftUp = false;
    public bool shiftDown = false;
    public float CurrentRPM { get; private set; }
    public float currentRPM => CurrentRPM;
    float simulatedRPM = 0f;
    const float rpmRiseSpeed = 1.0f;   // how fast RPM climbs
    const float rpmFallSpeed = 2.0f;   // how fast RPM drops off throttle

    // Perfect shift window — RPM 0-1 normalized
    const float perfectShiftMin = 0.75f;
    const float perfectShiftMax = 0.9f;
    const float _perfectShiftMin = 0.10f;
    const float _perfectShiftMax = 0.25f;

    public event Action<bool> OnShift; // true = perfect shift
    private Coroutine vfxCoroutine;

    void Update()
    {
        UpdateRPM();

        shiftDown = CurrentRPM >= _perfectShiftMin && CurrentRPM <= _perfectShiftMax;
        shiftUp = CurrentRPM >= perfectShiftMin && CurrentRPM <= perfectShiftMax;

        if (Input.GetKeyDown(KeyCode.W))
            TryShiftUp();
        if (Input.GetKeyDown(KeyCode.S))
            TryShiftDown();
    }


    void UpdateRPM()
    {
        float speed = carController.CurrentVelocity;
        float gearMin = CurrentGear > 1 ? gearMaxSpeeds[CurrentGear - 1] : 0f;
        float gearMax = gearMaxSpeeds[CurrentGear];

        // Target RPM is still speed-based
        float targetRPM = Mathf.Clamp01((speed - gearMin) / (gearMax - gearMin));

        // But simulated RPM rises/falls at different rates based on throttle
        bool isOnThrottle = carController.CurrentThrottleInput > 0.1f;
        float rpmSpeed = isOnThrottle ? rpmRiseSpeed : rpmFallSpeed;
        simulatedRPM = Mathf.MoveTowards(simulatedRPM, targetRPM, rpmSpeed * Time.deltaTime);

        CurrentRPM = simulatedRPM;
    }

    private void OnEnable()
    {
        // 2. Subscribe to the event
        OnShift += HandleShiftEvent;
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent memory leaks
        OnShift -= HandleShiftEvent;
    }

    // 3. The listener method that receives the bool from the event
    private void HandleShiftEvent(bool isPerfectShift)
    {
        if (isPerfectShift)
        {
            // If the player shifts perfectly, trigger the VFX
            if (vfxCoroutine != null) StopCoroutine(vfxCoroutine);
            vfxCoroutine = StartCoroutine(initVFX());
        }
    }

    void EnableTrailEmitting(bool val) {
        foreach (TrailRenderer Trail in trailRenderers) {
            if (Trail != null) Trail.emitting = val;
        }
    }

    private void SetTrailAlpha(float alpha)
    {
        foreach (TrailRenderer trail in trailRenderers)
        {
            if (trail == null) continue;

            // Grab the current color gradient of the trail
            Gradient gradient = trail.colorGradient;
            GradientAlphaKey[] alphaKeys = gradient.alphaKeys;

            // Modify the alpha keys dynamically
            for (int i = 0; i < alphaKeys.Length; i++)
            {
                alphaKeys[i].alpha = alpha;
            }

            // Reapply the updated keys back to the trail
            gradient.SetKeys(gradient.colorKeys, alphaKeys);
            trail.colorGradient = gradient;
        }
    }

    IEnumerator initVFX()
    {
        // 1. Instantly turn emitting ON and set trails to full visibility
        Debug.Log($"trails enabled");
        SetTrailAlpha(1f);
        EnableTrailEmitting(true);

        float elapsedTime = 0f;

        // 2. Smoothly fade the alpha over time
        while (elapsedTime < 3f)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / 3f;

            // Lerp alpha from 1 (opaque) to 0 (invisible)
            float currentAlpha = Mathf.Lerp(1f, 0f, normalizedTime);
            SetTrailAlpha(currentAlpha);

            yield return null;
        }

        // 3. Clean up: turn off emitting when fully faded
        EnableTrailEmitting(false);
    }

    void TryShiftUp()
    {
        if (CurrentGear >= MaxGear) return;

        bool isPerfect = CurrentRPM >= perfectShiftMin && CurrentRPM <= perfectShiftMax;

        CurrentGear++;
        carController.SetAccelMultiplier(gearAccelMultipliers[CurrentGear]);
        carController.SetMaxSpeed(gearMaxSpeeds[CurrentGear]);

        if (isPerfect)
        {
            // Reward: score bonus + brief acceleration boost
            carController.SetAccelMultiplier(1.6f);
            StartCoroutine(ResetBoostAfterDelay(1f));
        }

        OnShift?.Invoke(isPerfect);
    }

    void TryShiftDown()
    {
        if (CurrentGear <= 1) return;

        bool isPerfect = CurrentRPM >= _perfectShiftMin && CurrentRPM <= _perfectShiftMax;

        CurrentGear--;
        carController.SetAccelMultiplier(gearAccelMultipliers[CurrentGear]);
        carController.SetMaxSpeed(gearMaxSpeeds[CurrentGear]);

        if (isPerfect)
        {
            // Reward: score bonus + brief acceleration boost
            carController.SetMaxSpeed(gearMaxSpeeds[CurrentGear] * 1.25f);
        }

        OnShift?.Invoke(isPerfect);
    }

    IEnumerator ResetBoostAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        carController.SetAccelMultiplier(gearAccelMultipliers[CurrentGear]);
    }

    int MaxGear => gearMaxSpeeds.Length - 1;
}