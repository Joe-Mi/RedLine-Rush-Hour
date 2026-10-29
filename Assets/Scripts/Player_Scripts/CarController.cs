using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarController : MonoBehaviour
{
    

    [SerializeField] private Stats CarStats;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform gameModel;
    [SerializeField] private MeshRenderer carMesh;
    public MeshRenderer CarMeshRenderer => carMesh;
    public Stats CarData => CarStats;
    
    [SerializeField] HitHandler hitHandler;
    public ObjectiveSystem objectiveSystem;

    private Vector2 input = Vector3.zero;

    [Header("Car Settings")]
    public float accelMultiplier = 8f;
    public float baseAccelMultiplier = 8f;
    public float brakeMultiplier = 50f;
    public float steerMultiplier = 10f;


    //Max and gear system values.
    public float CurrentThrottleInput => rb.linearVelocity.z;
    public int rpms = 0;
    public int gear = 1;
    float maxSpeed = 25;
    float maxSteerVelocity = 15;

    //Player stats
    [SerializeField] private int health = 3;
    float score = 0;
    float startingSpawn = 0;
    float currentlocal = 0;
    float distance = 0;
    public float Score => score;
    bool notDown = true;
    bool isPlayer = true;

    WaitForSeconds wait = new WaitForSeconds(2.5f);

    //Events;
    public event Action<CarController> OnPlayerDown;

    public float CurrentVelocity => rb.linearVelocity.magnitude;

    void Awake() {
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterCar(this);
    }

    void Start()
    {
        isPlayer = CompareTag("Player");

        accelMultiplier = baseAccelMultiplier;
        startingSpawn = transform.position.z;


        if (CarStats != null)
        {
            Debug.Log($"Top Speed : {CarStats.topSpeed}");
            Debug.Log($"Acceleration : {CarStats.acceleration}");
            Debug.Log($"Handling : {CarStats.handling} gold");

            accelMultiplier = CarStats.acceleration;
            baseAccelMultiplier = CarStats.acceleration;
            brakeMultiplier = CarStats.braking;
            steerMultiplier = CarStats.handling;
        }

        if (GameManager.Instance != null)
        {
            // 2. Grab the component attached to the GameManager object
            objectiveSystem = GameManager.Instance.GetComponent<ObjectiveSystem>();
        }
        else
        {
            Debug.LogError("CarController: GameManager.Instance is missing from the scene!");
        }

        if (isPlayer) { 
            StartCoroutine(UpdateLessOftenCO());
        }
        
    }


    void Update()
    {
        if(!notDown)
            { return; }
        
        if (isPlayer && notDown)
        {
            // Only gather input if this is the player car
            input.y = Input.GetAxis("Vertical");
            input.x = Input.GetAxis("Horizontal");

            Steer();
        }

       gameModel.transform.rotation = Quaternion.Euler(0, rb.linearVelocity.x * 2, rb.linearVelocity.x * 0.35f);
    }

    void FixedUpdate()
    {
        if (!notDown) {
            rb.linearDamping = rb.linearVelocity.z * 0.1f;
            rb.linearDamping = Mathf.Clamp(rb.linearDamping, 1.5f, 10);
            return;
        }

        if (rb.linearVelocity.z <= 0)
        {
            rb.linearVelocity = Vector3.zero;
        }

        if (input.y > 0)
        {
            Accelerate();
        } else if (input.y < 0) {
            Brake();
        } else {
            Coast();
        }

    }

    void Coast()
    {
        // Light drag simulates engine braking when off throttle
        rb.linearDamping = Mathf.Lerp(rb.linearDamping, 0.75f, Time.fixedDeltaTime * 3f);
    }

    void Accelerate() 
    {
        rb.linearDamping = 0;
        if (rb.linearVelocity.z >= maxSpeed) return;

        float rpm = Mathf.Clamp01(rb.linearVelocity.z / maxSpeed);
        float torqueCurve = rpm < 0.85f
            ? 1.0f
            : Mathf.Lerp(1.0f, 0.0f, (rpm - 0.85f) / 0.15f);

        rb.AddForce(transform.forward * accelMultiplier * 1.0f * torqueCurve, ForceMode.Force);
    }

    void Brake()
    {
        // Only apply braking force if moving forward
        if (Vector3.Dot(rb.linearVelocity, transform.forward) > 0)
        {
            rb.AddForce(transform.forward * brakeMultiplier * input.y, ForceMode.Force);
        }
    }

    void Steer()
    {
        if (Mathf.Abs(input.x) > 0f)
        {
            // 1. Calculate steering strength
            float speedFactor = Mathf.Clamp01(rb.linearVelocity.z / maxSpeed);

            // 2. Apply force to the side.
            rb.AddForce(rb.transform.right * steerMultiplier * input.x * (0.5f + speedFactor), ForceMode.Force);

            // 3. limit the strenth of the force applied to the x axis to a fraction of the maxSteeringVelocity.
            float normalizedX = rb.linearVelocity.x / maxSteerVelocity;
            normalizedX = Mathf.Clamp(normalizedX, -1.0f, 1.0f);

            // Applies the force/vector to the game objects rigid body.
            rb.linearVelocity = new Vector3(normalizedX * maxSteerVelocity, 0, rb.linearVelocity.z);
        }
        else
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new Vector3(0, 0, rb.linearVelocity.z), Time.fixedDeltaTime * 3);
        }
    }

    IEnumerator UpdateLessOftenCO() 
    {
        while (true)
        {
            score = score + (0.3f * rb.linearVelocity.z);
            currentlocal = transform.position.z;
            distance = startingSpawn - currentlocal;

            reportStats();

            yield return wait;
        }
    }

    public void reportStats() {
        objectiveSystem.ReportDistance(distance);
        objectiveSystem.ReportPerfectUpshift();
        objectiveSystem.ReportScore(score);
        objectiveSystem.ReportSpeed(rb.linearVelocity.z);
    }


    // These function are used by the AICar set their details.
    public void SetInput(Vector2 inputVector)
    {
        input.x = Mathf.Clamp(inputVector.x, -1f, 1f);
        input.y = Mathf.Clamp(inputVector.y, -1f, 1f);
    }

    public void SetMaxSpeed(float newMaxSpeed) 
    {
        maxSpeed = newMaxSpeed;
    }

    public void SetAccelMultiplier(float Multiplier)
    {
        accelMultiplier = baseAccelMultiplier * Multiplier;
    }

    // These functions handle collisions with game objects (AI_bot or enviroment colliders).
    private void OnCollisionEnter(Collision collision)
    {
        
        if (!isPlayer) {
            if (collision.transform.root.CompareTag("Untagged")) { 
                return;
            }

            if (collision.transform.root.CompareTag("CarAI"))
            {
                return;
            }

            return;
        }

        if (isPlayer)
        {
            if(health > 1) {
                Debug.Log($"Hit {collision.collider.name}, health {health}");
                hitHandler.Hit();
                health--;
            } else
            {
                notDown = false;
                Debug.Log($"Hit {collision.collider.name}, health {health} and is now down");
                GameManager.Instance.HandlePlayerDown(this);
                OnPlayerDown?.Invoke( this );
            }
        }


    }
}
