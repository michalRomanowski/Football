using UnityEngine;
using System.Collections.Generic;

public static class DistanceSorter
{
    public static List<PlayerBehaviourScript> CloneAndSortByDistance(List<PlayerBehaviourScript> players, Vector3 x)
    {
        List<PlayerBehaviourScript> ret = new List<PlayerBehaviourScript>();

        Dictionary<PlayerBehaviourScript, float> sqrDistances = new Dictionary<PlayerBehaviourScript, float>();

        foreach (var p in players)
        {
            var i = 0;

            for (i = 0; i < ret.Count && (x - p.PivotPoint).sqrMagnitude > sqrDistances[ret[i]]; i++) { }

            ret.Insert(i, p);
        }

        return ret;
    }
}