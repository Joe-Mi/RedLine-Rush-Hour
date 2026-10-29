using System.Collections;
using UnityEngine;
using UnityEngine.Windows;

public class AI_Handler_V2 : MonoBehaviour
{
    [SerializeField] CarController carController;

    RaycastHit[] raycastHits = new RaycastHit[5];
    bool isCarIsAhead = false;

    WaitForSeconds wait = new WaitForSeconds(0.2f);

    [SerializeField] LayerMask otherCarLayerMask;

    [SerializeField] BoxCollider boxCollider;

    private void Awake()
    {
        if (CompareTag("Player"))
        {
            Destroy(this);
            return;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(UpdateLessOftenCO());
    }

    // Update is called once per frame
    void Update()
    {
        //Set the acceleration & steer input.
        float accelInput = 1.0f;
        float steerInput = 0.0f;

        //breack if (isCarIsAhead).
        if (isCarIsAhead)
            accelInput = -0.5f;

        steerInput = Mathf.Clamp(steerInput, -1.0f, 1.0f);
        carController.SetInput(new Vector2(steerInput, accelInput));
    }

    IEnumerator UpdateLessOftenCO()
    {
        while (true)
        {
            isCarIsAhead = CheckIfOtherCarsIsAhead();
            yield return wait;
        }
    }

    bool CheckIfOtherCarsIsAhead()
    {
        Vector3 center = transform.position
               + transform.forward * (boxCollider.bounds.extents.z + 0.5f);

        Vector3 halfExtents = boxCollider.bounds.extents;

        float castDistance = 10f;

        boxCollider.enabled = false;

        int numberOfHits = Physics.BoxCastNonAlloc(
            transform.position, halfExtents, transform.forward,
            raycastHits, transform.rotation,  // ← make sure this is transform.rotation
            castDistance, otherCarLayerMask
        );

        boxCollider.enabled = true;

        //  Log even when nothing is hit so you can see which cars are blind
        //Debug.Log($"{gameObject.name} hits: {numberOfHits}, " +
        //  $"center: {center}, " +
        //  $"speed: {carController.CurrentVelocity:F1}, " +
        //  $"castEnd: {center + transform.forward * castDistance}");

        return numberOfHits > 0;
    }


    void OnDrawGizmosSelected()
    {
        if (boxCollider == null) return;

        Gizmos.color = isCarIsAhead ? Color.red : Color.green;

        Vector3 halfExtents = boxCollider.bounds.extents;
        float castDistance = 10f; // adjust this to how far ahead you want to check

        // Draw the box where the BoxCast is happening
        Gizmos.DrawWireCube(
            transform.position + transform.forward * castDistance,
            halfExtents * 2
        );
    }

    private void OnEnable()
    {
        //set the max speed (low, high).
        float speed = Random.value < 0.20f ? 22f : 24f;

        carController.SetMaxSpeed(speed);

        //drivingInLane = Random.Range(0, Utils.CarLanes.Length);
    }
}
