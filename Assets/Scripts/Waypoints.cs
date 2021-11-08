using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoints : MonoBehaviour
{
    int WALL_LAYER = 6;
    public Grid grid;
    public GameObject[] waypoints;
    int currentIndex = 0;
    bool clockwise = false;

    void Start()
    {
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
    }

    public void SetClockwise(bool value)
    {
        clockwise = value;
    }

    public Vector3 HandleArbitraryPosition(Vector3 pos, bool isClockwise)
    {
        float lowestDistance = 1000;
        int bestIndex = 0;
        int index = 0;
        foreach (GameObject waypoint in waypoints)
        {
            float curDistance = Vector2.Distance(waypoint.transform.position, pos);
            // Check that no wall is in the way
            RaycastHit2D hit = Physics2D.Raycast(pos, waypoint.transform.position - pos, curDistance, 1 << WALL_LAYER);
            if (curDistance < lowestDistance && !hit)
            {
                lowestDistance = curDistance;
                bestIndex = index;
            }
            index++;
        }

        int nextIndex = GetNextIndex(bestIndex, false);
        int prevIndex = GetNextIndex(bestIndex, true);

        Vector2 curPos = new Vector2(pos.x, pos.y);
        Vector2 curWaypointPos = new Vector2(waypoints[bestIndex].transform.position.x, waypoints[bestIndex].transform.position.y);
        Vector2 nextWaypointPos = new Vector2(waypoints[nextIndex].transform.position.x, waypoints[nextIndex].transform.position.y);
        Vector2 prevWaypointPos = new Vector2(waypoints[prevIndex].transform.position.x, waypoints[prevIndex].transform.position.y);

        Vector2 curDir = (curPos - curWaypointPos).normalized;
        Vector2 nextDir = (nextWaypointPos - curWaypointPos).normalized;
        Vector2 prevDir = (prevWaypointPos - curWaypointPos).normalized;

        clockwise = isClockwise;
        if (Vector2.Dot(curDir, nextDir) > Vector2.Dot(curDir, prevDir))
        {
            if (isClockwise)
            {
                currentIndex = nextIndex;
                return waypoints[nextIndex].transform.position;
            }
            else
            {
                currentIndex = bestIndex;
                return waypoints[bestIndex].transform.position;
            }

        }
        else 
        {
            if (!isClockwise)
            {
                currentIndex = prevIndex;
                return waypoints[prevIndex].transform.position;
            }
            else
            {
                currentIndex = bestIndex;
                return waypoints[bestIndex].transform.position;                
            }

        }
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

    public Vector3 GetNextWaypointPosition()
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
