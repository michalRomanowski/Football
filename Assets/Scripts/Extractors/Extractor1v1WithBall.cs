using UnityEngine;

public class Extractor1v1WithBall : IExtractor{
    private GameBehaviourScript game = GameObject.FindObjectOfType(typeof(GameBehaviourScript)) as GameBehaviourScript;

    public float[] Extract(PlayerBehaviourScript player)
    {
        float[] data = new float[4];

        data[0] = player.transform.position.x;
        data[1] = player.transform.position.y;
        
        foreach (var p in game.Players)
        {
            if (p != player)
            {
                data[2] = p.transform.position.x - player.transform.position.x;
                data[3] = p.transform.position.y - player.transform.position.y;
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
