using UnityEngine;


public class DroneDamage : MonoBehaviour
{
    [SerializeField] private GameObject firePrefab;
    [SerializeField] private bool willCrashAndBurn = false;
    [SerializeField] private GameObject loseScreenPanel;

    private Rigidbody droneRb;
    public int collisionCount = 0;
    private bool isImmune = false;
    private float immunityTime = 5f;
    private float immunityTimer;

    void Start()
    {
        // https://docs.unity3d.com/6000.0/Documentation/Manual/choose-collision-detection-mode.html
        // need to check if it'll make much of a difference to use other detection methods

        droneRb = GetComponent<Rigidbody>();
        droneRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (loseScreenPanel != null)
        {
            loseScreenPanel.SetActive(false);
            Debug.Log("LoseScreen panel is disabled at start.");
        }
        else
        {
            Debug.LogWarning("LoseScreen panel reference is not assigned in DroneDamage!");
        }
    }

    void Update()
    {
        if (isImmune)
        {
            immunityTimer -= Time.deltaTime;
            if (immunityTimer <= 0)
            {
                isImmune = false;
                Debug.Log("Immunity ended.");
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isImmune) return;

        float speed = collision.relativeVelocity.magnitude;
        

        if (speed > 3f)
        {
            collisionCount++;
            Debug.Log($"Hard collision! Count: {collisionCount}");

            if (collisionCount >= 3)
            {
                Crash();
            }
            else
            {
                isImmune = true;
                immunityTimer = immunityTime;
                Debug.Log($"Drone immune for {immunityTime} sec.");
            }
        }
    }

    void Crash()
    {
        Debug.Log($"Drone crashed after {collisionCount} hits!");

        if (firePrefab != null)
        {
            Instantiate(firePrefab, transform.position, Quaternion.identity);
        }

        if (willCrashAndBurn)
        {
            GetComponent<DroneControls>().enabled = false;
            return;
        }

        droneRb.isKinematic = true;

        if (loseScreenPanel != null)
        {
            loseScreenPanel.SetActive(true);
            Debug.Log("LoseScreen panel activated after crash.");
        }
    }
}