using UnityEngine;

public class SlotMovement : MonoBehaviour
{
    public float speed = 2f;
    private float distanceOnPath = 0f;
    private bool isInitialized = false;

    public bool isEmpty = true;

    void Start()
    {
        Invoke(nameof(InitializeDistance), 0.1f);
    }

    void InitializeDistance()
    {
        if (ConveyorManager.Instance == null || ConveyorManager.Instance.waypoints.Count == 0) return;

        distanceOnPath = CalculateInitialDistance();
        isInitialized = true;
    }

    float CalculateInitialDistance()
    {
        float minDifference = float.MaxValue;
        int closestSegmentIndex = 0;
        
        var waypoints = ConveyorManager.Instance.waypoints;

        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 p1 = waypoints[i].position;
            Vector3 p2 = waypoints[(i + 1) % waypoints.Count].position;
            
            Vector2 p1_2D = new Vector2(p1.x, p1.y);
            Vector2 p2_2D = new Vector2(p2.x, p2.y);
            Vector2 pos_2D = new Vector2(transform.position.x, transform.position.y);

            float dist1 = Vector2.Distance(p1_2D, pos_2D);
            float dist2 = Vector2.Distance(pos_2D, p2_2D);
            float segmentLength = Vector2.Distance(p1_2D, p2_2D);
            
            float difference = Mathf.Abs((dist1 + dist2) - segmentLength);
            
            if (difference < minDifference)
            {
                minDifference = difference;
                closestSegmentIndex = i;
            }
        }

        float totalDist = 0f;
        for (int i = 0; i < closestSegmentIndex; i++)
        {
            Vector3 p1 = waypoints[i].position;
            Vector3 p2 = waypoints[(i + 1) % waypoints.Count].position;
            totalDist += Vector2.Distance(new Vector2(p1.x, p1.y), new Vector2(p2.x, p2.y));
        }

        Vector3 segStart = waypoints[closestSegmentIndex].position;
        totalDist += Vector2.Distance(new Vector2(segStart.x, segStart.y), new Vector2(transform.position.x, transform.position.y));

        return totalDist;
    }

    void Update()
    {
        if (!isInitialized || ConveyorManager.Instance == null) return;

        distanceOnPath += speed * Time.deltaTime;
        transform.position = ConveyorManager.Instance.GetPositionAtDistance(distanceOnPath);
    }
}
