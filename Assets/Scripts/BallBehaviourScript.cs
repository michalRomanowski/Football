using UnityEngine;

public class BallBehaviourScript : MonoBehaviour
{
    public Vector3 PivotPoint { get { return GetComponent<SpriteRenderer>().bounds.center; } }

    private Vector3 movement = new Vector3(0, 0, 0);
    private GameBehaviourScript game;

    public PlayerBehaviourScript controllingPlayer { get; private set; }
    public PlayerBehaviourScript lastControllingPlayer { get; private set; }

    private const float DISTANCE_FROM_CONTROLLNG_PLAYER = 0.000001f;
    private const float DISTANCE_OF_LOST_CONTROL_FROM_CONTROLLNG_PLAYER = 0.2f;
    private const float BALL_KICK_SPEED_MULTIPLIER = 0.6f;
    private const float MAX_BALL_KICK_SPEED = 40.0f;
    private const float FRICTION = 0.6f;

    // Use this for initialization
    void Start()
    {
        game = GameObject.FindObjectOfType(typeof(GameBehaviourScript)) as GameBehaviourScript;
    }

    // Update is called once per frame
    void Update()
    {
        if(controllingPlayer != null && Vector3.Distance(PivotPoint, controllingPlayer.PivotPoint) > DISTANCE_OF_LOST_CONTROL_FROM_CONTROLLNG_PLAYER)
        {
            controllingPlayer = null;
            transform.parent = null;
        }

        transform.position += movement * Time.deltaTime;

        movement -= movement * FRICTION * Time.deltaTime;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        switch(collision.gameObject.name)
        {
            case "GoalHome":
                {
                    Debug.Log("AwayScored!!!");
                    transform.parent = null;
                    ToStartingPosition();
                    game.Goal(Team.Away);
                }
                break;
            case "GoalAway":
                {
                    Debug.Log("HomeScored!!!");
                    ToStartingPosition();
                    game.Goal(Team.Home);
                }
                break;
            case "LowerOut":
            case "UpperOut":
                {
                    movement = new Vector3(movement.x, -movement.y, 0);
                    Debug.Log("Out!!!");
                }
                break;
            case "UpperLeftCorner":
                {
                    game.Corner(CornerType.UpperLeft);
                }
                break;
            case "LowerLeftCorner":
                {
                    game.Corner(CornerType.LowerLeft);
                }
                break;
            case "UpperRightCorner":
                {
                    game.Corner(CornerType.UpperRight);
                }
                break;
            case "LowerRightCorner":
                {
                    game.Corner(CornerType.LowerRight);
                }
                break;
        }
    }

    public void HitByPlayer(CollisionParams collisionParams)
    {
        controllingPlayer = collisionParams.Player;
        lastControllingPlayer = collisionParams.Player;

        if (Args.GameMode != GameMode.Play)
            game.Optimization.BallHitByPlayer();

        transform.position = collisionParams.Position + new Vector3(-Mathf.Sin(collisionParams.Rotation) * DISTANCE_FROM_CONTROLLNG_PLAYER, Mathf.Cos(collisionParams.Rotation) * DISTANCE_FROM_CONTROLLNG_PLAYER);
        movement = new Vector3(0, 0, 0);
        transform.parent = collisionParams.Transform;
    }

    public void Kicked(PlayerBehaviourScript player, Vector3 targetPosition)
    {
        if (controllingPlayer != player)
            return;
        
        controllingPlayer = null;

        transform.parent = null;
        movement = new Vector3(
            (targetPosition.x - transform.position.x) * BALL_KICK_SPEED_MULTIPLIER,
            (targetPosition.y - transform.position.y) * BALL_KICK_SPEED_MULTIPLIER,
            0);

        if (movement.magnitude > MAX_BALL_KICK_SPEED)
        {
            movement.Normalize();
            movement *= MAX_BALL_KICK_SPEED;
        }
    }

    public void ToStartingPosition()
    {
        transform.parent = null;
        controllingPlayer = null;
        lastControllingPlayer = null;

        movement = new Vector3(0, 0, 0);

        transform.position = new Vector3(0, 0);
    }

    public void MoveToCorner(CornerType corner)
    {
        if(corner == CornerType.UpperLeft)
            MoveTo(-3.7f, 2.7f);
        else if (corner == CornerType.LowerLeft)
            MoveTo(-3.7f, -2.7f);
        else if (corner == CornerType.UpperRight)
            MoveTo(3.7f, 2.7f);
        else if (corner == CornerType.LowerRight)
            MoveTo(3.7f, -2.7f);
    }

    public void MoveTo(float x, float y)
    {
        transform.parent = null;
        controllingPlayer = null;
        lastControllingPlayer = null;

        movement = new Vector3(0, 0, 0);

        transform.position = new Vector3(x, y);
    }
}
