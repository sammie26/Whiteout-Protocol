using UnityEngine;

public class ToggleCircle : MonoBehaviour
{
   
    [SerializeField] private SpriteRenderer greenCircleRenderer;
    [SerializeField] private SpriteRenderer redCircleRenderer;

   
    private SurvivorCoordinator survivorCoordinator;

   
    private string survivorName;
    private Terrain terrain; 

    private void Awake()
    {
        // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Object.FindFirstObjectByType.html
        survivorCoordinator = Object.FindFirstObjectByType<SurvivorCoordinator>();
        if (survivorCoordinator == null)
        {
            Debug.LogError("SurvivorCoordinator not found in the scene!");
        }

        
        if (survivorCoordinator != null)
        {
            terrain = survivorCoordinator.terrain; 
        }

        if (terrain == null)
        {
            Debug.LogError("Terrain not found via SurvivorCoordinator in ToggleCircle!");
        }

        
        if (greenCircleRenderer == null || redCircleRenderer == null)
        {
            Debug.LogError("Green or Red circle Sprite Renderer not assigned in ToggleCircle!");
        }

        
        survivorName = transform.parent.name.Replace("(Clone)", "").Trim(); // removes (Clone) to match SurvivorCoordinator
        if (string.IsNullOrEmpty(survivorName))
        {
            Debug.LogError("Could not determine survivor name from parent GameObject!");
        }
        else
        {
            Debug.Log($"ToggleCircle assigned to survivor: {survivorName}");
        }
    }

    private void Start()
    {
        
        UpdateCircles();
    }

    private void Update()
    {
        
        UpdateCircles();
    }

    private void UpdateCircles()
    {
        if (survivorCoordinator == null || string.IsNullOrEmpty(survivorName) || terrain == null) return;

        
        bool isSaved = false;
        SurvivorBehaviour currentSurvivor = null;
        foreach (var survivor in survivorCoordinator.SurvivorSpawnedList)
        {
            if (survivor.name.ToLower() == survivorName.ToLower())
            {
                currentSurvivor = survivor;
                foreach (var savedSurvivor in survivorCoordinator.SurvivorSavedList)
                {
                    if (savedSurvivor.name.ToLower() == survivorName.ToLower())
                    {
                        isSaved = true;
                        break;
                    }
                }
                break;
            }
        }

        if (currentSurvivor == null) return;

        
        Vector3 survivorPosition = currentSurvivor.transform.position;
        float terrainHeight = terrain.SampleHeight(survivorPosition) + 0.5f; 
        transform.position = new Vector3(survivorPosition.x, terrainHeight, survivorPosition.z);

        
        if (isSaved) // to toggle the green/red circle visibility 
        {
            greenCircleRenderer.enabled = true;  
            redCircleRenderer.enabled = false;   
        }
        else
        {
            redCircleRenderer.enabled = true;    
            greenCircleRenderer.enabled = false; 
        }
    }
}