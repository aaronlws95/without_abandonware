using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointCollider : MonoBehaviour
{
    public int index;
    public int nextIndex; // next waypoint in CCW dir
    public int prevIndex;
    public Vector3 curWaypointPos;
    public Vector3 prevWaypointPos;
    public Vector3 nextWaypointPos;
}
