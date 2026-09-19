using UnityEngine;

public class DroneControls : MonoBehaviour
{
    [SerializeField] private GameObject firstAidKitPrefab; 
    [SerializeField] private Move moveScript; 
    [SerializeField] private Transform[] propellers; 
    [SerializeField] public float rotationScale = 60f; 
    private AudioSource audioSource; 

    void Start()
    {
        
        if (moveScript == null)
        {
            moveScript = GetComponent<Move>();
            if (moveScript == null)
            {
                Debug.LogError("Move script not found on DroneControls GameObject!");
            }
        }
        if (firstAidKitPrefab == null)
        {
            Debug.LogWarning("First Aid Kit prefab not assigned in DroneControls!");
        }
        
        audioSource = GetComponent<AudioSource>(); // entry of the propeller sound 
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Return))
        {
            DropFirstAidKit();
        }

        if (Input.GetKey(KeyCode.Space) && moveScript != null)
        {
            RotatePropellers();
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            audioSource.Stop();
        }

        
        AdjustLiftForce();
    }

    void DropFirstAidKit()
    {
        if (firstAidKitPrefab != null)
        {
            
            Vector3 spawnPosition = transform.position + Vector3.down * 0.5f;
            Instantiate(firstAidKitPrefab, spawnPosition, Quaternion.identity);
        }
    }

    void RotatePropellers()
    {
        if (propellers == null || moveScript == null) return;

       
        float rotationSpeed = moveScript.liftForce * rotationScale; // calcuates the rotation speed based off of the lift force for proportionality 


        foreach (Transform propeller in propellers)
        {
            if (propeller != null)
            {
                propeller.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime, Space.Self);
            }
        }
    }

    void AdjustLiftForce()
    {
        if (moveScript == null) return;

        
        if (Input.GetKeyDown(KeyCode.M)) // increases lift speed with M key
        {
            moveScript.liftForce = Mathf.Min(moveScript.liftForce + 1f, 25f);
            Debug.Log("LiftForce increased to " + moveScript.liftForce.ToString("F2") + " at " + Time.time);
        }


        
        if (Input.GetKeyDown(KeyCode.N)) // decreases lift speed with N key
        {
            moveScript.liftForce = Mathf.Max(moveScript.liftForce - 1f, 5f);
            Debug.Log("LiftForce decreased to " + moveScript.liftForce.ToString("F2") + " at " + Time.time);
        }
    }
}