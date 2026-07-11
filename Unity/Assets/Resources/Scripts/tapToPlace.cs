using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;

using Lean.Touch;


[RequireComponent(typeof(ARRaycastManager))]
public class tapToPlace : MonoBehaviour
{
    public GameObject gameObjectToInstantiate; //the Prefab GameObject to instantiate in the AR environment. To be added in the inspector window
    private GameObject spawnedObject; //the Prefab Instantiate in the scene. Used internally by the script 
    private ARRaycastManager _arRaycastManager; //part of the XROrigin

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    public float timeThreshold = 0.5f; //User need to tap and hold the finger for at least 0.5 sec to create the content
    public bool isTouching = false;

    //Event design to fire when content is created
    public delegate void ContentVisibleDelegate();
    public event ContentVisibleDelegate _contentVisibleEvent;

    private void Awake()
    {
        _arRaycastManager = GetComponent<ARRaycastManager>();
    }

    private void OnEnable()
    {
        LeanTouch.OnFingerTap += OnFingerTap;
    }

    private void OnDisable()
    {
        LeanTouch.OnFingerTap -= OnFingerTap;
    }

    public bool TryGetTouchPosition(out Vector2 touchPosition)
    {
        if (Touchscreen.current.primaryTouch.press.isPressed)
        {
            isTouching = true;
            touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }
        touchPosition = default;
        isTouching = false;
        timeThreshold = 0;
        return false;
    }

    /* 
    void Update()
     {
      [...]
     }
     */
    private void OnFingerTap(LeanFinger finger)
    {
        if (finger.TapCount == 2) // Check for double tap
        {
            Vector2 touchPosition = finger.ScreenPosition;

            if (_arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                var hitPose = hits[0].pose;

                if (spawnedObject == null)
                {
                    spawnedObject = Instantiate(gameObjectToInstantiate, hitPose.position, hitPose.rotation);
                    _contentVisibleEvent?.Invoke();
                }
                else
                {
                    spawnedObject.transform.position = hitPose.position;
                }
            }
        }
    }
}