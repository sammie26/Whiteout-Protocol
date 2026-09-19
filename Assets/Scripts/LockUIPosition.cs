using UnityEngine;

//check for more efficient methods to ensures that the UI element stay at a specific position to override changes that might occur due to layout changes or screen resizing

public class LockUIDosition : MonoBehaviour
{
    private RectTransform rectTransform;
    [SerializeField] private Vector2 fixedPosition = new Vector2(-253f, -86f);  // placeholder values to be changed later

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        rectTransform.anchoredPosition = fixedPosition;
    }
}