using UnityEngine;
using TMPro;
using System.Collections.Generic;


public class HUDLayout : MonoBehaviour
{
    
    [SerializeField] private TextMeshProUGUI liftForceText;
    [SerializeField] private TextMeshProUGUI propellerSpeedText;
    [SerializeField] private TextMeshProUGUI seaAltitudeText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI distanceText;

    
    [SerializeField] private Move moveScript;
    [SerializeField] private DroneControls droneControlsScript;
    [SerializeField] private DroneDamage droneDamageScript;

  
    [SerializeField] private List<SurvivorBehaviour> survivors;

    [SerializeField] private SurvivorCoordinator survivorCoordinator;

    
    [SerializeField] private GameObject cameraHUD;

    private Transform droneTransform;
    private bool isHudVisible = true;

    void Start()
    {
        survivors = survivorCoordinator.SurvivorSpawnedList;
       
        if (moveScript == null) moveScript = GetComponent<Move>();
        if (droneControlsScript == null) droneControlsScript = GetComponent<DroneControls>();
        if (droneDamageScript == null) droneDamageScript = GetComponent<DroneDamage>();


        
        droneTransform = transform;

        //survivors = survivorCoordinator.SurvivorSpawnedList;
        // log survivor details
        if (survivors == null || survivors.Count == 0)
        {
            Debug.LogWarning("No survivors assigned in DroneHUD!");

            Debug.LogWarning($"Survivors = {survivors}");
            Debug.LogWarning($"Survivors Count = {survivors.Count}");
        }
        else
        {
            Debug.Log("Found " + survivors.Count + " survivors assigned in DroneHUD.");
            foreach (SurvivorBehaviour survivor in survivors)
            {
                if (survivor != null)
                {
                    Debug.Log("Survivor assigned: " + survivor.name + " (Active: " + survivor.gameObject.activeSelf + ", Position: " + survivor.transform.position + ")");
                }
                else
                {
                    Debug.LogWarning("Null survivor reference in survivors array!");
                }
            }
        }
      
        
        ToggleHudVisibility(isHudVisible);
    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.F1))
        {
            isHudVisible = !isHudVisible;
            ToggleHudVisibility(isHudVisible);
        }

        
        if (isHudVisible) // updates based off of whether it's visible or not
        {
            UpdateHUD();
        }


    }

    void UpdateHUD()
    {
        
        if (liftForceText != null)
        {
            liftForceText.text = "LIFT: " + moveScript.liftForce.ToString("F1");
        }

        
        if (propellerSpeedText != null && droneControlsScript != null)
        {
            float rotationSpeed = moveScript.liftForce * droneControlsScript.rotationScale;
            propellerSpeedText.text = "PROP SPD: " + rotationSpeed.ToString("F0") + "RPM";
        }

        
        if (seaAltitudeText != null)
        {
            float altitude = droneTransform.position.y;
            seaAltitudeText.text = "SEA ALT: " + altitude.ToString("F0") + "m";
        }

        
        if (damageText != null && droneDamageScript != null)
        {
            float damagePercent = (droneDamageScript.collisionCount / 3f) * 100f;
            damageText.text = "DMG: " + damagePercent.ToString("F0") + "%";
        }

        
        if (distanceText != null)
        {
            if (survivors == null || survivors.Count == 0)
            {
                Debug.LogWarning("No survivors assigned in UpdateHUD!");
                distanceText.text = "DST. FROM CLOSEST TARGET: NO TARGETS";
                return;
            }

            float closestDistance = float.MaxValue;
            GameObject closestSurvivor = null;
            foreach (SurvivorBehaviour survivor in survivors)
            {
                if (survivor != null && survivor.gameObject.activeSelf)
                {
                    // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Vector3.Distance.html
                    float distance = Vector3.Distance(droneTransform.position, survivor.transform.position);
                    Debug.Log("Distance to " + survivor.name + ": " + distance);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestSurvivor = survivor.gameObject;
                    }
                }
            }

            if (closestSurvivor != null)
            {
                Debug.Log("Closest survivor: " + closestSurvivor.name + " at distance: " + closestDistance);
                
                // makes sure to update character chosen based off of whether or not they were saved
                bool isSaved = false;
                SurvivorBehaviour closestSurvivorBehaviour = closestSurvivor.GetComponent<SurvivorBehaviour>();
                if (closestSurvivorBehaviour != null && survivorCoordinator != null)
                {
                    foreach (var savedSurvivor in survivorCoordinator.SurvivorSavedList)
                    {
                        if (savedSurvivor.name.ToLower() == closestSurvivorBehaviour.name.ToLower())
                        {
                            isSaved = true;
                            break;
                        }
                    }
                }

                if (closestDistance <= 800f && !isSaved)
                {
                    distanceText.text = "DST. FROM CLOSEST TARGET: " + closestSurvivor.name + " (" + closestDistance.ToString("F0") + "m)";
                }
                else
                {
                    distanceText.text = "DST. FROM CLOSEST TARGET: OUT OF RANGE";
                }
            }
            else
            {
                Debug.LogWarning("No valid closest survivor found!");
                distanceText.text = "DST. FROM CLOSEST TARGET: NO TARGETS";
            }
        }
    }

    void ToggleHudVisibility(bool isVisible)
    {
        if (cameraHUD != null)
        {
            cameraHUD.SetActive(isVisible);
        }
    }
}