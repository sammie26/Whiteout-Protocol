using System;
using System.Collections.Generic;
using UnityEngine;

public class SurvivorCoordinator : MonoBehaviour
{
    
    [System.Serializable] // to store survivor spawn data for Inspector
    public struct SurvivorSpawnData
    {
        public string survivorName;
        public Vector3 spawnPosition;
    }

    [SerializeField] private List<SurvivorBehaviour> survivorPrefabs = new List<SurvivorBehaviour>() { null, null, null };
    [SerializeField] private List<SurvivorBehaviour> survivorSpawnedList = new List<SurvivorBehaviour>();
    public List<SurvivorBehaviour> SurvivorSpawnedList => survivorSpawnedList;

    [SerializeField] private List<SurvivorBehaviour> survivorSavedList = new List<SurvivorBehaviour>();
    public List<SurvivorBehaviour> SurvivorSavedList => survivorSavedList;

    public int SurvivorSavedCount => survivorSavedList.Count;

    [SerializeField] private GameObject winScreenPanel;
    [SerializeField] public Terrain terrain;
    [SerializeField] private float maxSlopeDeg = 10f;
    [SerializeField] private float maxHeight = 160f;
    [SerializeField] private float minDistance = 70f;
    [SerializeField] private int maxSpawnAttempts = 1000;

    [SerializeField] private GameObject drone = null;

    
    [SerializeField] private GameObject kateCheck;
    [SerializeField] private GameObject peteCheck;
    [SerializeField] private GameObject steveCheck;

    
    private Dictionary<string, Vector3> survivorSpawnLocations = new Dictionary<string, Vector3>(); // dictionary to store spawn locations

    // inspector-visible list of spawn locations
    [SerializeField] private List<SurvivorSpawnData> inspectorSpawnLocations = new List<SurvivorSpawnData>();

  

    private void Awake()
    {
        ValidateSurvivorPrefabsList();
        SpawnSurvivors();
    }

    private void Start()
    {
       
       
        if (kateCheck != null) kateCheck.SetActive(false);
        if (peteCheck != null) peteCheck.SetActive(false);
        if (steveCheck != null) steveCheck.SetActive(false);
    }

    private void Update()
    {
        survivorSavedList = GetSurvivorsSaved();

       
        if (SurvivorSavedCount >= survivorSpawnedList.Count)
        {
            winScreenPanel.SetActive(true);
            drone.GetComponent<Rigidbody>().isKinematic = true;
        }

        
        UpdateBlueChecks();
    }

    private void SpawnSurvivors()
    {
        
        survivorSpawnLocations.Clear();
        inspectorSpawnLocations.Clear();

        foreach (SurvivorBehaviour survivor in survivorPrefabs)
        {
            bool survivorSpawned = false;
            for (int _ = 0; _ <= maxSpawnAttempts; _++)
            {
                float sizeX = terrain.terrainData.size.x;
                float sizeZ = terrain.terrainData.size.z;

                float randomLocalX = UnityEngine.Random.Range(0f, sizeX);
                float randomLocalZ = UnityEngine.Random.Range(0f, sizeZ);

                Vector3 normalAtPoint = terrain.terrainData.GetInterpolatedNormal(randomLocalX / sizeX, randomLocalZ / sizeZ);
                float slopeAngle = Vector3.Angle(normalAtPoint, Vector3.up);

                float height = terrain.SampleHeight(new Vector3(randomLocalX, 0, randomLocalZ));

                Vector3 positionToSpawn = new Vector3(randomLocalX, height, randomLocalZ);
                float distanceFromNearestSurvivor = GetMinimumDistanceFromSurvivors(positionToSpawn, survivorSpawnedList);

                if (slopeAngle <= maxSlopeDeg && height <= maxHeight && distanceFromNearestSurvivor >= minDistance)
                {
                    SurvivorBehaviour newSurvivor = Instantiate(survivor, positionToSpawn, Quaternion.identity);
                    newSurvivor.drone = drone;
                    newSurvivor.name = newSurvivor.name.Replace("(Clone)", "");
                    survivorSpawnedList.Add(newSurvivor);
                    survivorSpawned = true;

                    
                    survivorSpawnLocations[newSurvivor.name] = positionToSpawn; // to store spawn location in dictionary

                    
                    inspectorSpawnLocations.Add(new SurvivorSpawnData
                    {
                        survivorName = newSurvivor.name,
                        spawnPosition = positionToSpawn
                    });

                    Debug.Log($"Spawned {newSurvivor.name} at coordinates: {positionToSpawn}");

                    break;
                }
            }

            if (!survivorSpawned) throw new Exception($"Could not find suitable location to spawn {survivor}");
        }

        
        Debug.Log("Survivor Spawn Locations:");
        foreach (var entry in survivorSpawnLocations)
        {
            Debug.Log($"Survivor: {entry.Key}, Spawn Coordinates: {entry.Value}");
        }
    }

    private void ValidateSurvivorPrefabsList()
    {
        foreach (object el in survivorPrefabs)
        {
            if (el is SurvivorBehaviour) continue;
            throw new ArgumentException("Please make sure that the SurvivorPrefabsList is filled with survivors.");
        }
    }

    private List<SurvivorBehaviour> GetSurvivorsSaved()
    {
        List<SurvivorBehaviour> savedList = new List<SurvivorBehaviour>();
        foreach (SurvivorBehaviour survivor in survivorSpawnedList)
        {
            if (survivor.ObtainedHealthKit)
            {
                savedList.Add(survivor);
                Debug.Log($"{survivor.name} has been saved! ObtainedHealthKit: {survivor.ObtainedHealthKit}");
            }
        }
        return savedList;
    }

    private float GetMinimumDistanceFromSurvivors(Vector3 position, List<SurvivorBehaviour> survivors)
    {
        float minDistance = float.MaxValue;
        foreach (SurvivorBehaviour survivor in survivors)
        {
            Vector3 delta = survivor.transform.position - position;
            delta.y = 0;
            float distance = delta.magnitude;
            if (distance < minDistance) minDistance = distance;
        }
        return minDistance;
    }

    private void UpdateBlueChecks()
    {
        if (kateCheck == null || peteCheck == null || steveCheck == null)
        {
            Debug.LogError("One or more blue check UI elements are not assigned!");
            return;
        }

        kateCheck.SetActive(false);
        peteCheck.SetActive(false);
        steveCheck.SetActive(false);
        Debug.Log("All checks reset to inactive");

        foreach (SurvivorBehaviour survivor in survivorSavedList)
        {
            Debug.Log($"Processing saved survivor: {survivor.name}");
            switch (survivor.name.ToLower())
            {
                case "kate":
                    kateCheck.SetActive(true);
                    Debug.Log("Activating KATECHECK");
                    break;
                case "pete":
                    peteCheck.SetActive(true);
                    Debug.Log("Activating PETECHECK");
                    break;
                case "steve":
                    steveCheck.SetActive(true);
                    Debug.Log("Activating STEVECHECK");
                    break;
                default:
                    Debug.LogWarning($"Survivor name {survivor.name} does not match any case!");
                    break;
            }
        }
    }
}