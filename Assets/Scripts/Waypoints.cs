using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoints : MonoBehaviour
{
    int WAYPOINT_LAYER = 7;
    public GameObject[] waypoints;
    int currentIndex = 0;
    bool clockwise = false;
    Grid grid;

    void Start()
    {
        grid = GameObject.Find("Grid").GetComponent<Grid>();

        // Snap waypoints to grid
        foreach (GameObject waypoint in waypoints)
        {
            WaypointSettings ws = waypoint.GetComponent<WaypointSettings>();
            bool snapToGridX = ws.snapToGridX;
            bool snapToGridY = ws.snapToGridY;
            Vector3Int cellPosition = grid.WorldToCell(waypoint.transform.position);
            Vector3 gridPosition =  grid.GetCellCenterWorld(cellPosition);
            float newX = snapToGridX ? gridPosition.x : waypoint.transform.position.x;
            float newY = snapToGridY ? gridPosition.y : waypoint.transform.position.y; 
            waypoint.transform.position = new Vector3(newX, newY, waypoint.transform.position.z);
        }

        // Create colliders
        for (int i = 0; i<waypoints.Length; i++)
        {
            int nextIndex = GetNextIndex(i, false);
            int prevIndex = GetNextIndex(i, true);
            Vector3 curWaypointPosition = waypoints[i].transform.position;
            Vector3 nextWaypointPosition = waypoints[nextIndex].transform.position;
            Vector3 prevWaypointPosition = waypoints[prevIndex].transform.position;
            GameObject waypointCollider = new GameObject("Collider");
            waypointCollider.layer = WAYPOINT_LAYER;
            waypointCollider.transform.parent = transform;
            WaypointCollider wpcol = waypointCollider.AddComponent<WaypointCollider>();
            wpcol.index = i;
            wpcol.nextIndex = nextIndex;
            wpcol.prevIndex = prevIndex;
            wpcol.curWaypointPos = curWaypointPosition;
            wpcol.prevWaypointPos = prevWaypointPosition;
            wpcol.nextWaypointPos = nextWaypointPosition;
            BoxCollider2D col = waypointCollider.AddComponent<BoxCollider2D>();
            Vector3 dir = nextWaypointPosition - curWaypointPosition;
            waypointCollider.transform.position = (curWaypointPosition + nextWaypointPosition) / 2;
            float width = Mathf.Abs(dir.x);
            float height = Mathf.Abs(dir.y);

            // Rotate for diagonals
            if (width > 0.01f && height > 0.01f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                angle = width > height ? angle : angle - 90f;  
                waypointCollider.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            }

            // Set thickness of collider
            if (width > height)
            {
                height = 1;
                width += 1;
            }
            else 
            {
                height += 1;
                width = 1;
            }

            col.size = new Vector2(width, height);
        }
    }

    public int getCurrentIndex()
    {
        return currentIndex;
    }

    public void SetCurrentPath(int index, bool isClockwise)
    {
        currentIndex = index;
        clockwise = isClockwise;
    }

    public void SetClockwise(bool value)
    {
        clockwise = value;
    }

    public int GetNextIndex(int _currentIndex, bool _clockwise)
    {
        int nextIndex;
        if (!_clockwise)
        {
            nextIndex = _currentIndex + 1;
            if (nextIndex >= waypoints.Length)
            {
                nextIndex = 0;
            }
        }
        else 
        {
            nextIndex = _currentIndex - 1;
            if (nextIndex < 0)
            {
                nextIndex = waypoints.Length - 1;
            }
        }

        return nextIndex;
    }

    public Vector3 GetWaypointPositionAt(int index)
    {
        return waypoints[index].transform.position;
    }

    public Vector3 GenerateNextWaypointPosition()
    {
        if (!clockwise)
        {
            currentIndex++;
            if (currentIndex >= waypoints.Length)
            {
                currentIndex = 0;
            }
        }
        else 
        {
            currentIndex--;
            if (currentIndex < 0)
            {
                currentIndex = waypoints.Length - 1;
            }
        }

        return waypoints[currentIndex].transform.position;
    }
}
