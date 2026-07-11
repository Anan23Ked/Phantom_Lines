// using UnityEngine;

// // [RequireComponent(typeof(mqttPublish))]

// // [RequireComponent(typeof(mqttPublish))]
// public class StickIntersection : MonoBehaviour
// {
//     public Transform stickStart;
//     public Transform stickEnd;
//     public LayerMask environmentLayer;
//     public GameObject markerPrefab;
//     private GameObject currMarker;
//     private bool initialized = false;
//     public mqttPublish publisher;

//     public DistanceDashboard dashboard;    // Add this reference


//     public int LEDnos = 144;
  


//     void Start()
//     {
//         if (!TryGetComponent<mqttPublish>(out _))
//             Debug.Log("mqttPublish missing in STICKObject prefab!");

//         if (markerPrefab != null)
//         {
//             currMarker = Instantiate(markerPrefab);
//             currMarker.SetActive(false);   
//             initialized = true;
//         }
//         else
//         {
//             Debug.LogError("ERROR: markerPrefab not assigned in inspector");
//         }

//         if (stickStart == null)
//             Debug.LogError("ERROR: stickStart not assigned");
//         if (stickEnd == null)
//             Debug.LogError("ERROR: stickEnd not assigned");
//     }

// void Update()
// {
//     if (!initialized || stickStart == null || stickEnd == null)
//     {
//         dashboard?.UpdateDistance(null);
//         return;
//     }

//     Vector3 dir = (stickEnd.position - stickStart.position).normalized;
//     float length = Vector3.Distance(stickStart.position, stickEnd.position);

//     if (Physics.Raycast(stickStart.position, dir, out RaycastHit hit, length, environmentLayer))
//     {
//         currMarker.SetActive(true);
//         currMarker.transform.position = hit.point + hit.normal * 0.002f;
//         currMarker.transform.rotation = Quaternion.LookRotation(hit.normal);

//         float hitDistance = Vector3.Distance(stickStart.position, hit.point);

//         // UPDATE DASHBOARD HERE
//         dashboard?.UpdateDistance(hitDistance);

//         // Existing LED logic
//         float t = hitDistance / length;
//         int ledIndex = Mathf.RoundToInt(t * LEDnos - 1);
//         publisher?.SendLEDIndex(ledIndex);
//         if (publisher != null) { publisher.SendLEDIndex(ledIndex); }
//             else
//             {
//                 Debug.LogWarning("mqttPublish component not found; cannot send LED index.");
//             }

//         Debug.Log($"Hit {hit.collider.name}  Dist={hitDistance:F3}  LED={ledIndex}");
//     }
//     else
//     {
//         currMarker.SetActive(false);

//         // No hit → tell UI
//         dashboard?.UpdateDistance(null);
//     }
// }

// }

using UnityEngine;

// [RequireComponent(typeof(mqttPublish))]

// [RequireComponent(typeof(mqttPublish))]
public class StickIntersection : MonoBehaviour
{
    public Transform stickStart;
    public Transform stickEnd;
    public LayerMask environmentLayer;
    public GameObject markerPrefab;
    private GameObject currMarker;
    private bool initialized = false;
    public mqttPublish publisher;

    public int LEDnos = 144;
  


    void Start()
    {
        if (!TryGetComponent<mqttPublish>(out _))
            Debug.Log("mqttPublish missing in STICKObject prefab!");

        if (markerPrefab != null)
        {
            currMarker = Instantiate(markerPrefab);
            currMarker.SetActive(false);   
            initialized = true;
        }
        else
        {
            Debug.LogError("ERROR: markerPrefab not assigned in inspector");
        }

        if (stickStart == null)
            Debug.LogError("ERROR: stickStart not assigned");
        if (stickEnd == null)
            Debug.LogError("ERROR: stickEnd not assigned");
    }

    void Update()
    {
        if (!initialized || stickStart == null || stickEnd == null)
            return;

        Vector3 dir = (stickEnd.position - stickStart.position).normalized;
        float length = Vector3.Distance(stickStart.position, stickEnd.position);

        if (Physics.Raycast(stickStart.position, dir, out RaycastHit hit, length, environmentLayer))
        {
            currMarker.SetActive(true);
            currMarker.transform.position = hit.point + hit.normal * 0.002f;
            currMarker.transform.rotation = Quaternion.LookRotation(hit.normal);

            var envId = hit.collider.GetComponent<EnvironmentIdentifier>();
            string objName = envId != null ? envId.id : hit.collider.name;

            float hitDistance = Vector3.Distance(stickStart.position, hit.point);
            float t = hitDistance / length;
            int ledIndex = Mathf.RoundToInt(t * LEDnos -1);
            publisher?.SendLEDIndex(ledIndex);
            if (publisher != null)
            {
                    publisher.SendLEDIndex(ledIndex);
            }
            else
            {
                Debug.LogWarning("mqttPublish component not found on StickIntersection GameObject.");
            }



            Debug.Log($"Hit: {objName}  point={hit.point}  LED={ledIndex}  t={t:F3}");
        }
        else
        {
            currMarker.SetActive(false);
        }
    }
}
