using UnityEngine;

public class Extractor1v1WithoutBall : IExtractor {
    private GameBehaviourScript game = GameObject.FindObjectOfType(typeof(GameBehaviourScript)) as GameBehaviourScript;

    public float[] Extract(PlayerBehaviourScript player)
    {
        float[] data = new float[6];

        data[0] = player.transform.position.x;
        data[1] = player.transform.position.y;

        var ball = GameObject.Find("Ball");
        data[2] = ball.transform.position.x - player.transform.position.x;
        data[3] = ball.transform.position.y - player.transform.position.y;

        foreach (var p in game.Players)
        {
            if(p != player)
            {
                data[4] = p.transform.position.x - player.transform.position.x;
                data[5] = p.transform.position.y - player.transform.position.y;
            }
        }

        if (player.Team == Team.Away)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] *= -1;
        }

        return data;
    }
}
