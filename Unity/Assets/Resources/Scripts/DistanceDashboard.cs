using UnityEngine;
using TMPro;

public class DistanceDashboard : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI distanceText;

    private float? currentDistance = null;   // nullable float

    void Start()
    {
        if (distanceText == null)
            Debug.LogError("DistanceDashboard: distanceText not assigned in Inspector!");
    }

    public void UpdateDistance(float? distance)
    {
        currentDistance = distance;

        if (distance == null)
        {
            distanceText.text = "Move around to find out length";
        }
        else
        {
            distanceText.text = $"Distance from plane:\n{distance.Value:F2} m";
        }
    }
}
