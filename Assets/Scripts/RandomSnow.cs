using UnityEngine;

public class RandomSnow : MonoBehaviour
{
    [SerializeField] private Terrain terrain;
    [SerializeField] private float targetSnowCoverage = 0.8f; 

    void Start()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain not assigned!");
            return;
        }

        ApplyExactTerrainCoverage();
    }

    void ApplyExactTerrainCoverage()
    {
        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;

 
        Debug.Log($"Alphamap Width: {width}, Height: {height}, Layers: {terrainData.alphamapLayers}");
        if (terrainData.alphamapLayers < 2)
        {
            Debug.LogError("Terrain must have at least 2 texture layers (mountain and snow)!");
            return;
        }

        float[,,] splatmap = terrainData.GetAlphamaps(0, 0, width, height);

        // note: snow is layer 1 and mountain is layer 0
        int snowLayerIndex = 1;
        int mountainLayerIndex = 0;

        
        (int x, int y, float snowWeight)[] points = new (int, int, float)[width * height];  // calcualtes for snow weights
        int index = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float snowWeight = Random.value;
                splatmap[y, x, snowLayerIndex] = snowWeight;
                splatmap[y, x, mountainLayerIndex] = 1f - snowWeight;
                points[index] = (x, y, snowWeight);
                index++;
            }
        }

        // sorts the snow points 
        System.Array.Sort(points, (a, b) => a.snowWeight.CompareTo(b.snowWeight));

        // sets to the 80% thingy requirement 
        int totalPoints = width * height;
        int snowPoints = Mathf.RoundToInt(totalPoints * targetSnowCoverage);
        Debug.Log($"Target Snow Coverage: {targetSnowCoverage * 100f:F2}%, Total Points: {totalPoints}, Snow Points: {snowPoints}, Expected Coverage: {(float)snowPoints / totalPoints * 100f:F2}%");

      

        int snowAssigned = 0;
        for (int i = 0; i < totalPoints; i++)
        {
            int x = points[i].x;
            int y = points[i].y;
            if (i < snowPoints)
            {
                splatmap[y, x, snowLayerIndex] = 1f; // full snow situation 
                splatmap[y, x, mountainLayerIndex] = 0f; // no mountain
                snowAssigned++;
            }
            else
            {
                splatmap[y, x, snowLayerIndex] = 0f; // no snow
                splatmap[y, x, mountainLayerIndex] = 1f; // full mountain
            }
        }
        Debug.Log($"Points Assigned to Snow: {snowAssigned}");

        // apply the modified stuff
        terrainData.SetAlphamaps(0, 0, splatmap);

        // check if its actually 80%
        float preCoverageCheck = CalculateSnowCoverage(splatmap, width, height, snowLayerIndex);
        Debug.Log($"Snow Coverage in Splatmap Before Final Calculation: {(preCoverageCheck / totalPoints * 100f):F2}%");

        
        float finalSnowCoverage = CalculateSnowCoverage(splatmap, width, height, snowLayerIndex);
        float coveragePercentage = finalSnowCoverage / totalPoints * 100f;
        Debug.Log($"Final Snow Coverage: {coveragePercentage:F2}% (Total Snow: {finalSnowCoverage}, Total Points: {totalPoints})");
    }

    private float CalculateSnowCoverage(float[,,] splatmap, int width, int height, int snowLayerIndex)
    {
        float totalSnow = 0f;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                totalSnow += splatmap[y, x, snowLayerIndex];
            }
        }
        return totalSnow;
    }
}