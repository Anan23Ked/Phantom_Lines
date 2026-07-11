// using UnityEngine;
// using TMPro;

// public class mqttPublish : MonoBehaviour
// {
//     public string tag_mqttManager = ""; //to be set on the Inspector panel. It must match one of the mqttManager.cs GameObject
//     [Header("   Case Sensitive!!")]
//     [Tooltip("the topic to publish !!Case Sensitive!! ")]
//     public string topicPublish = ""; //the topic to subscribe
//     public mqttManager _eventSender;
//     private bool _connected;
//     public string myName;
//     private string messagePublish = "";

//     void Awake()
//     {
//         if (GameObject.FindGameObjectsWithTag(tag_mqttManager).Length > 0)
//         {
//             _eventSender = GameObject.FindGameObjectsWithTag(tag_mqttManager)[0].gameObject.GetComponent<mqttManager>();
//             _eventSender.OnConnectionSucceeded += OnConnectionSucceededHandler;
//         }
//         else
//         {
//             Debug.LogError("At least one GameObject with mqttManager component and Tag == tag_mqttManager needs to be provided");
//         }
//     }

//     private void OnConnectionSucceededHandler(bool connected)
//     {
//         _connected = true; //control if we are connected to the MQTT Broker
//     }

//     public void SendLEDIndex(int index)
//     {
        

//         if (!_connected) //publish if connected
//             return;
       
//         _eventSender.topicPublish = topicPublish;
//         _eventSender.messagePublish = index.ToString();
     

//         _eventSender.topicPublish = topicPublish;
//         _eventSender.Publish();
//         Debug.Log("Publish" + messagePublish);
//     }
// }


using UnityEngine;

public class mqttPublish : MonoBehaviour
{
    public string tag_mqttManager = ""; 
    [Header("   Case Sensitive!!")]
    public string topicPublish = ""; 

    private mqttManager _eventSender;
    private bool _connected = false;

    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag_mqttManager);

        if (objs.Length > 0)
        {
            _eventSender = objs[0].GetComponent<mqttManager>();
            _eventSender.OnConnectionSucceeded += OnConnectionSucceededHandler;
        }
        else
        {
            Debug.LogError($"mqttPublish ERROR: No GameObject with tag '{tag_mqttManager}' containing mqttManager found.");
        }
    }

    private void OnConnectionSucceededHandler(bool connected)
    {
        _connected = connected;
    }

    public void SendLEDIndex(int index)
    {
        if (!_connected)
        {
            Debug.LogWarning("mqttPublish: MQTT not connected yet, cannot send LED index.");
            return;
        }

        // Update mqttManager fields
        _eventSender.topicPublish = topicPublish;
        _eventSender.messagePublish = index.ToString();

        // Publish through mqttManager
        _eventSender.Publish();

        Debug.Log($"MQTT Published LED Index: {index}   on Topic: {topicPublish}");
    }
}

