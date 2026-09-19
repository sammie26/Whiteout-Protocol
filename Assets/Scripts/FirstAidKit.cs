using UnityEngine;

public class FirstAidKit : MonoBehaviour
{
    [SerializeField] private float maxDropVelocity = 8f;
    public bool IsReadyForPickUp { get; private set; } = false;

    public void Update()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        float linearVelocity = rb.linearVelocity.magnitude;
        if (linearVelocity > 0.1f) Debug.Log($"Linear Velocity: {rb.linearVelocity.magnitude}");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude >= maxDropVelocity)
        {
            Destroy(gameObject);
        }
        else
        {
            IsReadyForPickUp = true;
        }
    }
}
