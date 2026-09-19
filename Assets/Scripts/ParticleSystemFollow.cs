using UnityEngine;

//follows same logic from the rain following ball from the 7th - just slightly altered 

public class ParticleSystemFollow : MonoBehaviour
{
    public Transform target; 
    public Vector3 offset = new Vector3(0, 2, 0); // temp offset to make sure particles are always above the drone

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("Target not assigned to ParticleSystemFollow script on " + gameObject.name);
            enabled = false; 
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        
        transform.position = target.position + offset;
       
        transform.rotation = Quaternion.identity;
    }
}