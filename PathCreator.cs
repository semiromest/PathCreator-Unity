using UnityEngine;

public class PathCreator : MonoBehaviour
{
    public Transform[] waypoints;
    public int resolution = 10;
    public float speed = 5f;
    public bool loop = false;

    private int currentWaypointIndex = 0;
    private Vector3[] pathPoints;
    private float distanceTravelled = 0f;

    private void Start()
    {
        GeneratePathPoints();
    }

    private void Update()
    {
        if (currentWaypointIndex < pathPoints.Length)
        {
            distanceTravelled += speed * Time.deltaTime;

            if (loop)
            {
                distanceTravelled %= GetPathLength();
            }

            transform.position = GetPointOnPath(distanceTravelled);
            transform.rotation = GetRotationOnPath(distanceTravelled);

            if (transform.position == pathPoints[currentWaypointIndex])
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= pathPoints.Length)
                {
                    if (loop)
                    {
                        currentWaypointIndex = 0;
                    }
                    else
                    {
                        // Path completed
                        // Do something else or stop object movement
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        GeneratePathPoints();

        if (pathPoints.Length > 1)
        {
            for (int i = 0; i < pathPoints.Length - 1; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
            }

            if (loop)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(pathPoints[pathPoints.Length - 1], pathPoints[0]);
            }
        }
    }

    private void GeneratePathPoints()
    {
        int numPoints = waypoints.Length * resolution;
        pathPoints = new Vector3[numPoints];
        int index = 0;

        for (int i = 0; i < waypoints.Length; i++)
        {
            int nextIndex = (i + 1) % waypoints.Length;
            float tStep = 1f / resolution;

            for (int j = 0; j < resolution; j++)
            {
                float t = j * tStep;
                pathPoints[index] = CalculateBezierPoint(waypoints[i].position, waypoints[i].position + waypoints[i].forward, waypoints[nextIndex].position - waypoints[nextIndex].forward, waypoints[nextIndex].position, t);
                index++;
            }
        }
    }

    private Vector3 CalculateBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 point = uuu * p0 + 3f * uu * t * p1 + 3f * u * tt * p2 + ttt * p3;
        return point;
    }

    private float GetPathLength()
    {
        float length = 0f;
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            length += Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
        }
        return length;
    }

    private Vector3 GetPointOnPath(float distance)
    {
        float pathLength = GetPathLength();
        float normalizedDistance = distance / pathLength;

        int segmentIndex = Mathf.FloorToInt(normalizedDistance * (pathPoints.Length - 1));
        float segmentLength = pathLength / (pathPoints.Length - 1);

        float t = (distance - segmentIndex * segmentLength) / segmentLength;
        return Vector3.Lerp(pathPoints[segmentIndex], pathPoints[segmentIndex + 1], t);
    }

    private Quaternion GetRotationOnPath(float distance)
    {
        float pathLength = GetPathLength();
        float normalizedDistance = distance / pathLength;

        int segmentIndex = Mathf.FloorToInt(normalizedDistance * (pathPoints.Length - 1));
        Vector3 forward = (pathPoints[segmentIndex + 1] - pathPoints[segmentIndex]).normalized;

        Quaternion fromRotation = Quaternion.LookRotation(forward);
        Quaternion toRotation = Quaternion.LookRotation((pathPoints[segmentIndex + 2] - pathPoints[segmentIndex + 1]).normalized);

        return Quaternion.Slerp(fromRotation, toRotation, normalizedDistance * (pathPoints.Length - 1) - segmentIndex);
    }
}