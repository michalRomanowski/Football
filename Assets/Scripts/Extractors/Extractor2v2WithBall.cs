using UnityEngine;

public class Extractor2v2WithBall
{
    private GameBehaviourScript game = GameObject.FindObjectOfType(typeof(GameBehaviourScript)) as GameBehaviourScript;

    public float[] Extract(PlayerBehaviourScript player)
    {
        float[] data = new float[10];

        data[0] = player.transform.position.x;
        data[1] = player.transform.position.y;

        foreach (var p in game.Teams[player.Team])
        {
            if (p != player)
            {
                data[2] = p.transform.position.x - player.transform.position.x;
                data[3] = p.transform.position.y - player.transform.position.y;
            }
        }

        var sortedByDistanceOpponents = DistanceSorter.CloneAndSortByDistance(game.Teams[player.OpponentTeam], player.PivotPoint);

        for (int i = 4; i < 8; i += 2)
        {
            data[i] = sortedByDistanceOpponents[i - 8].transform.position.x - player.transform.position.x;
            data[i + 1] = sortedByDistanceOpponents[i - 8].transform.position.y - player.transform.position.y;
        }

        if (player.Team == Team.Away)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] *= -1;
        }

        return data;
    }
}

