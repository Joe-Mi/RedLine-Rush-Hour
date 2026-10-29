using UnityEngine;

public class HitHandler : MonoBehaviour
{
    [SerializeField] Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    public void Hit()
    {
        rb.AddForce(new Vector3(0, 0, -75), ForceMode.Force);
    }
}
