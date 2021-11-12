using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int level;
    public List<float> bestTimes;

    public PlayerData(int _level, List<float> _bestTimes)
    {
        level = _level;
        bestTimes = _bestTimes;
    }
}
