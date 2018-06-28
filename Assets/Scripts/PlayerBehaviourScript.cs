using UnityEngine;
using NeuralNet;
using System.IO;

public enum Controller
{
    Human,
    Bot
}

public class PlayerBehaviourScript : MonoBehaviour {

    public Team Team;
    public Team OpponentTeam
    {
        get
        {
            var opponentTeam = Team == Team.Away ? Team.Home : Team.Away;
            return opponentTeam;
        }
    }

    public Controller Controller;

    public Vector3 InitialPosition;
    public Vector3 DefendingCornerPosition;
    public Vector3 AttackingCornerPosition;
    public Vector3 WithBallGoalKeeperFreekickPosition;
    public Vector3 WithoutBallGoalKeeperFreekickPosition;

    public Vector3 PivotPoint { get { return GetComponent<SpriteRenderer>().bounds.center; } }
    public Vector3 Movement { get; private set; }
        
    public INeuralNet MovingNeuralNet { get; private set; }
    public INeuralNet MovingWithBallNeuralNet { get; private set; }
    public INeuralNet KickingBallNeuralNet { get; private set; }
    
    private const string MOVING_NEURAL_NET = "MovingNeuralNet";
    private const string MOVING_WITH_BALL_NEURAL_NET = "MovingWithBallNeuralNet";
    private const string KICKING_BALL_NEURAL_NET = "KickingBallNeuralNet";

    private const float MOVEMENT_SPEED = 1.0f;
    private const float DYNAMIC_SPEED = 0.4f;
    private const float DIRECTED_SPEED = 1.0f;
    private const float ROTATION_SPEED = 300.0f;
    private const float CONTROLLING_BALL_MOVEMENT_MULTIPLIER = 0.7f;
    private const float KICKING_BALL_MOVEMENT_MULTIPLIER = 0.6f;
    private const float MAX_SPEED = 2.0f;
    private const float BRAKING_MULTIPLIER = 0.9f;
    private const float ACCELERATION = 1.0f;
    private const float KICING_BALL_OUTPUT_MULTIPLIER = 3.0f;

    private BallBehaviourScript ball;

    private float _rotationDegrees { get { return transform.eulerAngles.z; } }
    private float _rotation { get { return transform.eulerAngles.z * Mathf.PI / 180; } }

    private IExtractor ExtractorWithBall;
    private IExtractor ExtractorWithoutBall;

    private static RectTransform _fieldRectTransform = null;
    private static RectTransform fieldRectTransform
    {
        get
        {
            if (_fieldRectTransform == null)
                _fieldRectTransform = GameObject.Find("Field").GetComponent<RectTransform>();
            return _fieldRectTransform;
        }
    }

    private static Camera _mainCamera = null;
    private static Camera mainCamera
    {
        get
        {
            if (_mainCamera == null)
                _mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

            return _mainCamera;
        }
    }

    public PlayerBehaviourScript()
    {
        MovingNeuralNet = NeuralNetsProvider.GetRandomMultiLayerNeuralNet(6, 2, 8, 1);
        MovingWithBallNeuralNet = NeuralNetsProvider.GetRandomMultiLayerNeuralNet(4, 2, 8, 1);
        KickingBallNeuralNet = NeuralNetsProvider.GetRandomMultiLayerNeuralNet(4, 3, 8, 1);
    }

    // Use this for initialization
    void Start () {
        ball = (BallBehaviourScript)GameObject.Find("Ball").GetComponent("BallBehaviourScript");

        if (Team == Team.Home)
        {
            GetComponent<SpriteRenderer>().color = Color.red;

            if(Args.GameMode == GameMode.Play)
                Controller = Controller.Human;
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.yellow;

            if (Args.GameMode == GameMode.Play)
            {
                Controller = Controller.Bot;
                LoadFromDisc(Application.dataPath + "/StreamingAssets/Teams/OpponentTeam");
            }
        }

        switch(Args.GameMode)
        {
            case GameMode.Play:
            case GameMode.Simple1v1Play:
            case GameMode.Simple1v1WithBall:
            case GameMode.Simple1v1WithoutBall:
                {
                    ExtractorWithBall = new Extractor1v1WithBall();
                    ExtractorWithoutBall = new Extractor1v1WithoutBall();
                }
                break;
        }
    }
	
	// Update is called once per frame
	void Update () {
        var input = new PlayerInput();

        if (Controller == Controller.Human)
            input = HumanInput();
        else
            input = BotInput();

        var movementChange = new Vector3();

        bool up = input.Up && !input.Down;
        bool down = input.Down && !input.Up;
        bool left = input.Left && !input.Right;
        bool right = input.Right && !input.Left;

        if (up && left)
            RotateTo(45);
        else if (up && right)
            RotateTo(315);
        else if (up)
            RotateTo(0);
        else if (down && left)
            RotateTo(135);
        else if (down && right)
            RotateTo(225);
        else if (down)
            RotateTo(180);
        else if (left)
            RotateTo(90);
        else if (right)
            RotateTo(270);
        
        // DYNAMIC MOVEMENT
        if(up)
            movementChange += new Vector3(0, DYNAMIC_SPEED, 0);
        if (down)
            movementChange += new Vector3(0, -DYNAMIC_SPEED, 0);
        if (left)
            movementChange += new Vector3(-DYNAMIC_SPEED, 0, 0);
        if (right)
            movementChange += new Vector3(DYNAMIC_SPEED, 0, 0);

        // REDIRECTING EXISTING MOVEMENT
        Movement = new Vector3(
            Movement.magnitude * - Mathf.Sin(_rotation),
            Movement.magnitude * Mathf.Cos(_rotation));

        // DIRECTED MOVEMENT
        if (up && (_rotationDegrees <= 90 || _rotationDegrees > 270))
            movementChange += new Vector3(0, Mathf.Cos(_rotation), 0) * DIRECTED_SPEED;
        else if (down && (_rotationDegrees > 90 && _rotationDegrees <= 270))
            movementChange += new Vector3(0, Mathf.Cos(_rotation), 0) * DIRECTED_SPEED;

        if (left && (_rotationDegrees > 0 && _rotationDegrees <= 180))
            movementChange += new Vector3(-Mathf.Sin(_rotation), 0, 0) * DIRECTED_SPEED;
        else if (right && _rotationDegrees > 180 && _rotationDegrees <= 360)
            movementChange += new Vector3(-Mathf.Sin(_rotation), 0, 0) * DIRECTED_SPEED;

        movementChange *= MOVEMENT_SPEED * Time.deltaTime;

        // BRAKING
        if (movementChange.x == 0 || Mathf.Sign(movementChange.x) != Mathf.Sign(Movement.x))
            Movement = new Vector3(Movement.x * BRAKING_MULTIPLIER, Movement.y);

        if (movementChange.y == 0 || Mathf.Sign(movementChange.y) != Mathf.Sign(Movement.y))
            Movement = new Vector3(Movement.x, Movement.y * BRAKING_MULTIPLIER);

        // CORRECTING SPEED CURVE
        movementChange *= (MAX_SPEED - Movement.magnitude)/MAX_SPEED;

        Movement += movementChange * ACCELERATION;

        if (Movement.magnitude > MAX_SPEED)
        {
            Movement.Normalize();
            Movement *= MAX_SPEED;
        }

        // APPLYING MOVEMENT
        if (ball.controllingPlayer == this)
            transform.position += Movement * Time.deltaTime * CONTROLLING_BALL_MOVEMENT_MULTIPLIER;
        else
            transform.position += Movement * Time.deltaTime;

        // BALL KICK
        if (input.MouseLeftPressed && ball.controllingPlayer == this)
        {
            Movement *= KICKING_BALL_MOVEMENT_MULTIPLIER;
            ball.Kicked(this, input.mousePosition);
        }
    }

    private void RotateTo(int targetAngle)
    {
        if (Mathf.Abs(targetAngle - _rotationDegrees) < ROTATION_SPEED * Time.deltaTime)
            transform.eulerAngles = new Vector3(0, 0, targetAngle);

        int oppositeAngle = (targetAngle + 180) % 360;

        int reverse = -1;
        
        if(targetAngle > oppositeAngle)
        {
            int tmp = targetAngle;
            targetAngle = oppositeAngle;
            oppositeAngle = tmp;

            reverse = 1;
        }

        if(_rotationDegrees > targetAngle && _rotationDegrees <= oppositeAngle)
            transform.Rotate(Vector3.forward * ROTATION_SPEED * Time.deltaTime * reverse);
        else transform.Rotate(Vector3.back * ROTATION_SPEED * Time.deltaTime * reverse);
    }

    private PlayerInput HumanInput()
    {
        PlayerInput input = new PlayerInput();

        input.Up = Input.GetKey(KeyCode.UpArrow);
        input.Down = Input.GetKey(KeyCode.DownArrow);
        input.Left = Input.GetKey(KeyCode.LeftArrow);
        input.Right = Input.GetKey(KeyCode.RightArrow);
        input.MouseLeftPressed = Input.GetMouseButtonDown(0);

        if (input.MouseLeftPressed)
        {
            Vector2 mousePositionOnfield;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                fieldRectTransform,
                Input.mousePosition,
                mainCamera,
                out mousePositionOnfield
                );

            input.mousePosition = mousePositionOnfield;
        }

        return input;
    }

    private PlayerInput BotInput()
    {
        if (ball.controllingPlayer == this)
            return BotInputWithBall();
        else
            return BotInputWithoutBall();
    }

    private PlayerInput BotInputWithBall()
    {
        PlayerInput input = new PlayerInput();
        BotInputWithBallMoving(input);
        BotInputWithBallKick(input);

        return input;        
    }

    private void BotInputWithBallMoving(PlayerInput input)
    {
        if (Args.GameMode != GameMode.Play && Args.Immobilize)
            return;

        float[] movingNeuralNetInput = ExtractorWithBall.Extract(this);

        float[] movingNeuralNetOutput = MovingWithBallNeuralNet.Think(movingNeuralNetInput);

        if (Team == Team.Away)
        {
            movingNeuralNetOutput[0] *= -1;
            movingNeuralNetOutput[1] *= -1;
        }

        input.Up = movingNeuralNetOutput[0] > 0.1f;
        input.Down = movingNeuralNetOutput[0] < -0.1f;
        input.Left = movingNeuralNetOutput[1] > 0.1f;
        input.Right = movingNeuralNetOutput[1] < -0.1f;
    }

    private void BotInputWithBallKick(PlayerInput input)
    {
        float[] kickingNeuralNetInput = ExtractorWithBall.Extract(this);

        float[] kickingNeuralNetOutput = KickingBallNeuralNet.Think(kickingNeuralNetInput);

        input.MouseLeftPressed = kickingNeuralNetOutput[0] > 0.0f;

        if (Team == Team.Away)
        {
            kickingNeuralNetOutput[1] *= -1;
            kickingNeuralNetOutput[2] *= -1;
        }

        if (input.MouseLeftPressed)
        {
            input.mousePosition = new Vector2(
                kickingNeuralNetOutput[1] * KICING_BALL_OUTPUT_MULTIPLIER - transform.position.x, 
                kickingNeuralNetOutput[2] * KICING_BALL_OUTPUT_MULTIPLIER - transform.position.y);
        }
    }

    private PlayerInput BotInputWithoutBall()
    {
        if (Args.GameMode != GameMode.Play && Args.Immobilize)
            return new PlayerInput();

        float[] neuralNetInput = ExtractorWithoutBall.Extract(this);

        float[] neuralNetOutput = MovingNeuralNet.Think(neuralNetInput);

        if (Team == Team.Away)
        {
            neuralNetOutput[0] *= -1;
            neuralNetOutput[1] *= -1;
        }

        PlayerInput input = new PlayerInput();

        input.Up = neuralNetOutput[0] > 0.1f;
        input.Down = neuralNetOutput[0] < -0.1f;
        input.Left = neuralNetOutput[1] > 0.1f;
        input.Right = neuralNetOutput[1] < -0.1f;

        input.MouseLeftPressed = false;
        
        return input;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name.Equals("Ball"))
        {
            collision.gameObject.SendMessage("HitByPlayer",
                new CollisionParams()
                {
                    Player = this,
                    Mass = 75.0f,
                    Movement = this.Movement,
                    Position = GetComponent<Renderer>().bounds.center,
                    Rotation = _rotation,
                    Transform = this.transform
                });
        }
    }

    public void HitByBall(CollisionParams collisionParams)
    {

    }

    public void Goal()
    {
        ToStartingPosition();
    }

    public void ToStartingPosition()
    {
        Movement = new Vector3(0, 0, 0);

        transform.position = new Vector3(InitialPosition.x, InitialPosition.y);

        if (Team == Team.Home)
            transform.eulerAngles = new Vector3(0, 0, 270);
        else
            transform.eulerAngles = new Vector3(0, 0, 90);
    }
    
    public void SaveTodisc(string path)
    {
        MovingNeuralNet.Save(Path.Combine(path, MOVING_NEURAL_NET));
        MovingWithBallNeuralNet.Save(Path.Combine(path, MOVING_WITH_BALL_NEURAL_NET));
        KickingBallNeuralNet.Save(Path.Combine(path, KICKING_BALL_NEURAL_NET));
    }

    public void LoadFromDisc(string path)
    {
        MovingNeuralNet.Load(Path.Combine(path, MOVING_NEURAL_NET));
        MovingWithBallNeuralNet.Load(Path.Combine(path, MOVING_WITH_BALL_NEURAL_NET));
        KickingBallNeuralNet.Load(Path.Combine(path, KICKING_BALL_NEURAL_NET));
    }

    public void MoveToAttackingCornerPosition(CornerType corner)
    {
        int angle = 90;
        float x = AttackingCornerPosition.x;
        float y = AttackingCornerPosition.y;
        
        switch(corner)
        {
            case CornerType.LowerLeft:
                {
                    y = -y;
                }
                break;
            case CornerType.UpperRight:
                {
                    angle = 270;
                    x = -x;
                }
                break;
            case CornerType.LowerRight:
                {
                    angle = 270;
                    x = -x;
                    y = -y;
                }
                break;
        }

        MoveTo(x, y, angle);
    }

    public void MoveToDefendingCornerPosition(CornerType corner)
    {
        int angle = 90;
        float x = DefendingCornerPosition.x;
        float y = DefendingCornerPosition.y;

        switch (corner)
        {
            case CornerType.LowerLeft:
                {
                    y = -y;
                }
                break;
            case CornerType.UpperRight:
                {
                    angle = 270;
                    x = -x;
                }
                break;
            case CornerType.LowerRight:
                {
                    angle = 270;
                    x = -x;
                    y = -y;
                }
                break;
        }

        MoveTo(x, y, angle);
    }

    public void MoveToWithBallGoalKeeperFreekickPosition()
    {
        if(Team == Team.Home)
            MoveTo(WithBallGoalKeeperFreekickPosition.x, WithBallGoalKeeperFreekickPosition.y, 270);
        else
            MoveTo(-WithBallGoalKeeperFreekickPosition.x, -WithBallGoalKeeperFreekickPosition.y, 90);
    }

    public void MoveToWithoutBallGoalKeeperFreekickPosition()
    {
        if (Team == Team.Home)
            MoveTo(WithoutBallGoalKeeperFreekickPosition.x, WithoutBallGoalKeeperFreekickPosition.y, 270);
        else
            MoveTo(-WithoutBallGoalKeeperFreekickPosition.x, -WithoutBallGoalKeeperFreekickPosition.y, 90);
    }

    public void MoveTo(float x, float y, int eulerAngle)
    {
        Movement = new Vector3(0, 0, 0);
        transform.position = new Vector3(x, y);
        transform.eulerAngles = new Vector3(0, 0, eulerAngle);
    }
}
