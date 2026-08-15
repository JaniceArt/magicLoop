using UnityEngine;
using System.Collections.Generic;

public class ConveyorManager : MonoBehaviour
{
    public static ConveyorManager Instance { get; private set; }

    public Transform conveyorPath;
    
    [HideInInspector]
    public List<Transform> waypoints = new List<Transform>();

    private float[] segmentLengths;
    public float TotalPathLength { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (conveyorPath != null)
        {
            foreach (Transform child in conveyorPath)
            {
                waypoints.Add(child);
            }
        }

        CalculatePathLengths();
    }

    void CalculatePathLengths()
    {
        if (waypoints.Count < 2) return;

        segmentLengths = new float[waypoints.Count];
        TotalPathLength = 0f;

        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 p1 = waypoints[i].position;
            Vector3 p2 = waypoints[(i + 1) % waypoints.Count].position;
            
            float length = Vector2.Distance(new Vector2(p1.x, p1.y), new Vector2(p2.x, p2.y));
            segmentLengths[i] = length;
            TotalPathLength += length;
        }
    }

    public Vector3 GetPositionAtDistance(float distance)
    {
        if (waypoints.Count == 0 || TotalPathLength == 0) return Vector3.zero;

        distance = Mathf.Repeat(distance, TotalPathLength);

        float currentDist = 0f;

        for (int i = 0; i < waypoints.Count; i++)
        {
            float segmentLength = segmentLengths[i];
            
            if (distance <= currentDist + segmentLength)
            {
                float t = (distance - currentDist) / segmentLength;
                
                Vector3 p1 = waypoints[i].position;
                Vector3 p2 = waypoints[(i + 1) % waypoints.Count].position;
                
                return Vector3.Lerp(p1, p2, t);
            }
            
            currentDist += segmentLength;
        }

        return waypoints[0].position; 
    }
}
