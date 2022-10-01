using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;

public class MetaController : MonoBehaviour
{
    private Board gameBoard;
    private Spawner spawner;
    private Shape activeShape;
    private Ghost ghost;

    private SoundManager soundManager;
    private ScoreManager scoreManager;

    private float dropInterval = 1f;
    private float dropIntervalModded;

    private float timeToDrop;
    private float timeToNextKeyLeftRight;
    private float timeToNextKeyDown;
    private float timeToNextKeyRotate;

    private float timeToNextSwipeLeftRight;
    private float timeToNextSwipeDown;

    [Range(0.02f, 1)]
    public float keyRepeatRateLeftRight = 0.1f;

    [Range(0.02f, 1)]
    public float keyRepeatRateDown = 0.05f;

    [Range(0.02f, 1)]
    public float keyRepeatRateRotate = 0.05f;

    [Range(0.02f, 1)]
    public float swipeRepeatRateLeftRight = 0.25f;

    [Range(0.02f, 1)]
    public float swipeRepeatRateDown = 0.05f;

    public GameObject pausePanel;
    public GameObject gameOverPanel;

    public GameObject roundedBlock;

    private bool gameOver = false;
    private bool rotateClockwise = true;

    public bool isPaused = false;
    public bool isPausedByGame = false;

    private enum Direction { none, left, right, up, down }

    private Direction swipeDirection = Direction.none;
    private Direction swipeEndDirection = Direction.none;

    private void OnEnable()
    {
        TouchController.SwipeEvent += SwipeHandler;
        TouchController.SwipeEndEvent += SwipeEndHandler;
    }

    private void OnDisable()
    {
        TouchController.SwipeEvent -= SwipeHandler;
        TouchController.SwipeEndEvent -= SwipeEndHandler;
    }

    private void Start()
    {
        isPausedByGame = true;
        Time.timeScale = isPaused || isPausedByGame ? 0 : 1;

        gameBoard = GameObject.FindObjectOfType<Board>();
        spawner = GameObject.FindObjectOfType<Spawner>();

        ghost = GameObject.FindObjectOfType<Ghost>();

        soundManager = GameObject.FindObjectOfType<SoundManager>();
        scoreManager = GameObject.FindObjectOfType<ScoreManager>();

        timeToNextKeyLeftRight = Time.time + keyRepeatRateLeftRight;
        timeToNextKeyRotate = Time.time + keyRepeatRateRotate;
        timeToNextKeyDown = Time.time + keyRepeatRateDown;

        if (!gameBoard)
            Debug.Log("No game board");

        if (!soundManager)
            Debug.Log("No sound manager");

        if (!scoreManager)
            Debug.Log("No score manager");


        if (!spawner)
            Debug.Log("No spawner");

        if (gameOverPanel)
            gameOverPanel.SetActive(false);

        if (pausePanel)
            pausePanel.SetActive(false);

        dropIntervalModded = dropInterval;

        {
            var dummyCube = Instantiate(roundedBlock, spawner.transform);
            dummyCube.transform.position = new Vector3(-4.5f, -12, 1);

            gameBoard.grid[0, 0] = dummyCube.transform;
        }

        {
            var dummyCube = Instantiate(roundedBlock, spawner.transform);
            dummyCube.transform.position = new Vector3( 4.5f, -12, 1);

            gameBoard.grid[9, 0] = dummyCube.transform;
        }
    }

    private void MoveRight()
    {
        activeShape.MoveRight();
        timeToNextKeyLeftRight = Time.time + keyRepeatRateLeftRight;
        timeToNextSwipeLeftRight = Time.time + swipeRepeatRateLeftRight;

        if (!gameBoard.IsValidPosition(activeShape))
            activeShape.MoveLeft();
    }

    private void MoveLeft()
    {
        activeShape.MoveLeft();
        timeToNextKeyLeftRight = Time.time + keyRepeatRateLeftRight;
        timeToNextSwipeLeftRight = Time.time + swipeRepeatRateLeftRight;

        if (!gameBoard.IsValidPosition(activeShape))
            activeShape.MoveRight();
    }

    private void Rotate()
    {
        activeShape.RotateClockwise(rotateClockwise);
        timeToNextKeyRotate = Time.time + keyRepeatRateRotate;

        if (!gameBoard.IsValidPosition(activeShape))
            activeShape.RotateClockwise(!rotateClockwise);
    }

    private void MoveDown()
    {
        timeToDrop = Time.time + dropIntervalModded;
        timeToNextKeyDown = Time.time + keyRepeatRateDown;
        timeToNextSwipeDown = Time.time + swipeRepeatRateDown;

        if (activeShape)
        {
            activeShape.MoveDown();

            if (!gameBoard.IsValidPosition(activeShape))
            {
                if (gameBoard.IsOverLimit(activeShape))
                {
                    GameOver();
                }
                else
                {
                    LandShape();
                }
            }
        }
    }

    private void PlayerInput()
    {
        if (!gameBoard || !spawner)
            return;

        if ((Input.GetButton("MoveRight") && Time.time > timeToNextKeyLeftRight) || Input.GetButtonDown("MoveRight"))
        {
            MoveRight();
        }
        else if ((Input.GetButton("MoveLeft") && Time.time > timeToNextKeyLeftRight) || Input.GetButtonDown("MoveLeft"))
        {
            MoveLeft();
        }
        else if (Input.GetButtonDown("Rotate") && Time.time > timeToNextKeyRotate)
        {
            Rotate();
        }
        else if ((Input.GetButton("MoveDown") && Time.time > timeToNextKeyDown) || Time.time > timeToDrop)
        {
            MoveDown();
        }
        else if ((swipeDirection == Direction.right && Time.time > timeToNextSwipeLeftRight) ||
                  swipeEndDirection == Direction.right)
        {
            MoveRight();
            swipeDirection = Direction.none;
            swipeEndDirection = Direction.none;
        }
        else if ((swipeDirection == Direction.left && Time.time > timeToNextSwipeLeftRight) ||
                  swipeEndDirection == Direction.left)
        {
            MoveLeft();
            swipeDirection = Direction.none;
            swipeEndDirection = Direction.none;
        }
        else if (swipeEndDirection == Direction.up)
        {
            Rotate();
            swipeEndDirection = Direction.none;
        }
        else if (swipeDirection == Direction.down && Time.time > timeToNextSwipeDown)
        {
            MoveDown();
            swipeDirection = Direction.none;
        }
        else if (Input.GetButtonDown("Pause"))
        {
            TogglePause();
        }
    }

    private void GameOver()
    {
        activeShape.MoveUp();

        scoreManager.SaveGame();

        StartCoroutine(GameOverRoutine());

        gameOver = true;
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(0.3f);

        if (gameOverPanel)
            gameOverPanel.SetActive(true);
    }

    private void LandShape()
    {
        timeToNextKeyLeftRight = Time.time;
        timeToNextKeyRotate = Time.time;
        timeToNextKeyDown = Time.time;

        activeShape.MoveUp();
        gameBoard.StoreShapeInGrid(activeShape);

        if (ghost)
            ghost.Reset();

        gameBoard.StartCoroutine(gameBoard.ClearAllRows());

        if (gameBoard.completedRows > 0)
        {
            scoreManager.ScoreLines(gameBoard.completedRows);

            if (scoreManager.didLevelUp)
                dropIntervalModded = dropInterval - Mathf.Clamp((((float)scoreManager.level - 1) * 0.1f), 0.05f, 1f);
        }

        if(gameBoard.IsClear())
        {
            scoreManager.gold += 1000;
            scoreManager.SaveGame();
            Exit();
        }    


        isPausedByGame = true;
        Time.timeScale = isPaused || isPausedByGame ? 0 : 1;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip && soundManager.fxEnabled)
        {
            AudioSource.PlayClipAtPoint(
                clip, Camera.main.transform.position,
                soundManager.fxVolume);
        }
    }

    private void Update()
    {
        if (!gameBoard || !spawner || !activeShape || gameOver || !soundManager || !scoreManager)
            return;

        PlayerInput();
    }

    private void LateUpdate()
    {
        if (ghost && activeShape)
            ghost.DrawGhost(activeShape, gameBoard);
    }

    private void SwipeHandler(Vector2 swipeMovement)
    {
        swipeDirection = GetDirection(swipeMovement);
    }

    private void SwipeEndHandler(Vector2 swipeMovement)
    {
        swipeEndDirection = GetDirection(swipeMovement);
    }

    private Direction GetDirection(Vector2 swipeMovement)
    {
        Direction swipeDirection = Direction.none;

        if (Mathf.Abs(swipeMovement.x) > Mathf.Abs(swipeMovement.y))
        {
            swipeDirection = swipeMovement.x >= 0 ? Direction.right : Direction.left;
        }
        else
        {
            swipeDirection = swipeMovement.y >= 0 ? Direction.up : Direction.down;
        }

        return swipeDirection;
    }


    public void Exit()
    {
        scoreManager.SaveGame();

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void ToggleRotationDirection()
    {
        rotateClockwise = !rotateClockwise;
    }

    public void TogglePause()
    {
        if (gameOver)
            return;

        scoreManager.SaveGame();

        isPaused = !isPaused;

        if (pausePanel)
        {
            pausePanel.SetActive(isPaused);

            if (soundManager)
            {
                soundManager.musicSource.volume =
                    isPaused ? soundManager.musicVolume * 0.25f : soundManager.musicVolume;
            }

            Time.timeScale = isPaused || isPausedByGame ? 0 : 1;
        }
    }

    public void SpawnShape(Shape shape)
    {
        if (isPaused)
            return;

        if (!isPausedByGame)
            return;

        //if (scoreManager.metaBricks < 1)
         //   return;

        scoreManager.metaBricks--;
        scoreManager.SaveGame();

        activeShape = Instantiate<Shape>(shape, spawner.transform);
        spawner.SpawnShape(activeShape);

        isPausedByGame = false;
        Time.timeScale = isPaused || isPausedByGame ? 0 : 1;
    }

    public void SpawnI()
    {
        foreach (var shape in spawner.allShapes)
            if (shape.name == "ShapeI")
            {
                SpawnShape(shape);
                return;
            }
    }

    public void SpawnJ()
    {
        foreach (var shape in spawner.allShapes)
            if (shape.name == "ShapeJ")
            {
                SpawnShape(shape);
                return;
            }
    }

    public void SpawnL()
    {
        foreach (var shape in spawner.allShapes)
            if (shape.name == "ShapeL")
            {
                SpawnShape(shape);
                return;
            }
    }

    public void SpawnO()
    {
        foreach (var shape in spawner.allShapes)
            if (shape.name == "ShapeO")
            {
                SpawnShape(shape);
                return;
            }
    }

    public void SpawnS()
    {
        foreach (var shape in spawner.allShapes)
            if (shape.name == "ShapeS")
            {
                SpawnShape(shape);
                return;
            }
    }

    public void SpawnT()
    {
        foreach (var shape in spawner.allShapes)
            if (shape.name == "ShapeT")
            {
                SpawnShape(shape);
                return;
            }
    }

    public void SpawnZ()
    {
        foreach (var shape in spawner.allShapes)
            if (shape.name == "ShapeZ")
            {
                SpawnShape(shape);
                return;
            }
    }
}
