using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;


public class GameplayScript : MonoBehaviour
{

    public GameObject firework;
    public GameObject trail;
    public GameObject Score;
    public GameObject HighScore;
    public GameObject TimeLeft;
    public GameObject GameOverScoreDisplay;
    public GameObject NewHighScore;
    public GameObject LineRenderer;
    public GameObject Arrow;
    public GameObject helptext;
    public GameObject[] GameOverButtons;
    public GameObject[] GamePlayStats;
    public GameObject[] DifficultyButtons;
    public GameObject[] StartSceneItems;
    public GameObject[] AddShapeSceneItems;
    public float minTime = 3;
    public float extraTime = 10;
    public float matchMargin = 0.5f;//if gesture is accurate enough - player gets a point. could be adjusted to scale difficulty
    private float timeLeft = 0;
    private SavedGestures Data = SavedGestures.GetInstance();
    private List<Vector3> gesture = new List<Vector3>();
    private Vector3 prevMousePosition;
    private bool isDrawing = false;
    private Vector2[] currentFigure = null;
    private int score = 0;
    private bool isGameOn = false;
    private bool isGameModeNewFigure = false;
    private bool isMouseInDrawingArea = false;
    private bool newHighScore = false;
    
    void Start()
    {
        foreach (var item in GamePlayStats)
            item.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            isMouseInDrawingArea = MouseInDrawingArea(Input.mousePosition);
        timeLeft -= Time.deltaTime;
        if (isGameOn && timeLeft <= 0.0f)
            SetGameModeGameOver();
        if (Input.GetMouseButton(0) && Input.mousePosition != prevMousePosition && isMouseInDrawingArea && (isGameModeNewFigure||isGameOn))
        {
            if (!isDrawing)
            {
                gesture = new List<Vector3>();
                isDrawing = true;
            }
            RecordGesturePoint(Input.mousePosition);
        }
        if (isGameOn)
        {
            ShowTime();
            if (isDrawing && !Input.GetMouseButton(0))
            {
                isDrawing = false;
                if (trail.GetComponent<ParticleSystem>().isPlaying)
                    trail.GetComponent<ParticleSystem>().Stop();
                float match = RecognitionScript.Match(currentFigure, gesture.ToArray());
                if (match >= matchMargin)
                {
                    score++;
                    Score.GetComponent<TextMesh>().text = score.ToString();
                    CheckHighScore(score);
                    NextRound();
                }
            }
        }
        if (isGameModeNewFigure && isDrawing && !Input.GetMouseButton(0))
        {
            isDrawing = false;
            if (trail.GetComponent<ParticleSystem>().isPlaying)
                trail.GetComponent<ParticleSystem>().Stop();
            if (gesture.Count > 0)
            {
                currentFigure = RecognitionScript.ProcessNewTemplate(gesture.ToArray());
                DrawCurrentFigure();
            }
        }
    }
    public void Restart()
    {
        isGameOn = false;
        SetGameModeStartScreen();
    }
    public void RecordGesture()
    {
        if (gesture.Count < 2)
            return;
        Data.Add(RecognitionScript.ProcessNewTemplate(gesture.ToArray()));
    } //check if last entered gesture is unique and write to file if so
    public void StartGame()
    {
        score = 0;
        extraTime = 10;
        Score.GetComponent<TextMesh>().text = score.ToString();
        HighScore.GetComponent<TextMesh>().text = Data.highScore.ToString();
        SetGameModeGamePlay();
        isGameOn = true;
        NextRound();
    }
    public void SetGameModeDifficultySettings()
    {
        foreach (var item in StartSceneItems)
            item.SetActive(false);
        foreach (var item in DifficultyButtons)
            item.SetActive(true);
    }
    public void SetGameModeGamePlay()
    {
        foreach (var item in StartSceneItems)
            item.SetActive(false);
        foreach (var item in GamePlayStats)
            item.SetActive(true);
    }
    public void SetGameModeNewShape()
    {
        foreach (var item in StartSceneItems)
            item.SetActive(false);
        foreach (var item in AddShapeSceneItems)
            item.SetActive(true);
        isGameModeNewFigure = true;
        ClearDrawArea();
    }
    public void SetGameModeStartScreen()
    {
        ClearDrawArea();
        isGameModeNewFigure = false;
        isGameOn = false;
        foreach (var item in GamePlayStats)
            item.SetActive(false);
        foreach (var item in AddShapeSceneItems)
            item.SetActive(false);
        foreach (var item in DifficultyButtons)
            item.SetActive(false);
        foreach (var item in GameOverButtons)
            item.SetActive(false);
        foreach (var item in StartSceneItems)
            item.SetActive(true);
        GameOverScoreDisplay.SetActive(false);
        NewHighScore.SetActive(false);
        helptext.SetActive(false);
    }
    public void SetGameModeGameOver()
    {
        isGameOn = false;
        ClearDrawArea();
        foreach (var item in GamePlayStats)
            item.SetActive(false);
        foreach (var item in GameOverButtons)
            item.SetActive(true);
        GameOverScoreDisplay.SetActive(true);
        GameOverScoreDisplay.GetComponent<TextMesh>().text = score.ToString();
        NewHighScore.SetActive(true);
        if (newHighScore)
        {
            NewHighScore.GetComponent<TextMesh>().text = "Новый рекорд!";
            Data.highScore = score;
            Data.Save();
            newHighScore = false;
            for (int i = -3; i < 4; i++)
            {
                Instantiate(firework, new Vector3((float)i, 2f, 0), GetComponent<Transform>().rotation);
            }
        }
        else
        {
            NewHighScore.GetComponent<TextMesh>().text = "Очков набрано:";
        }
    }
    public void SetDifficulty(float difficulty)
    {
        matchMargin = difficulty;
        SetGameModeStartScreen();
    }
    public void ShowHelp()
    {
        foreach (var item in StartSceneItems)
            item.SetActive(false);
        GamePlayStats[1].SetActive(true);
        helptext.SetActive(true);
    }
    public void Quit()
    {
        Application.Quit();
    }
    private void ShowTime()
    {
        TimeLeft.GetComponent<TextMesh>().text = Mathf.RoundToInt(timeLeft).ToString() + ":" + Mathf.RoundToInt((timeLeft % 1)*1000) + "ms";
    }
    private void NextRound()
    {
        currentFigure = Data.GetRandomGesture();
        DrawCurrentFigure();
        extraTime -= 0.3f;
        timeLeft = minTime + extraTime;
    }
    private void DrawCurrentFigure()
    {
        LineRenderer.SetActive(true);
        Arrow.SetActive(true);
        for (int index = 0; index < 64; index++)
        {
            LineRenderer.GetComponent<LineRenderer>().SetPosition(index, new Vector3(currentFigure[index].x * 0.8f, currentFigure[index].y * 0.8f, 1));
        }
        Arrow.GetComponent<LineRenderer>().SetPosition(0, new Vector3(currentFigure[0].x * 0.8f, currentFigure[0].y * 0.8f, 0.9f));
        Vector2 arrowPoint = currentFigure[0];
        int arrowIndex = 1;
        while (RecognitionScript.distance(currentFigure[0], arrowPoint) < 0.4f)
        {
            arrowPoint = currentFigure[arrowIndex++];
        }
        Arrow.GetComponent<LineRenderer>().SetPosition(1, new Vector3(arrowPoint.x * 0.8f, arrowPoint.y * 0.8f, 0.9f));
    }
    private void ClearDrawArea()
    {
        LineRenderer.SetActive(false);
        Arrow.SetActive(false);
    }
    private void CheckHighScore(int currentScore)
    {
        if (Data.highScore < score)
        {
            Data.highScore = score;
            newHighScore = true;
            HighScore.GetComponent<TextMesh>().text = score.ToString();
        }
    }
    private bool MouseInDrawingArea(Vector3 mousePosition)
    {
        Vector2 temp = new Vector2();
        temp.x = (mousePosition.x / Screen.width) * 13.32f;
        temp.x -= 6.66f;
        temp.y = (mousePosition.y / Screen.height) * 10f;
        temp.y -= 5f;
        if (temp.x < -5f || temp.x > 5f || temp.y < -5f || temp.y > 5f)
            return false; ;
        return true;
    }
    private void RecordGesturePoint(Vector3 mousePosition)
    {
        Vector2 adjustedMousePosition = adjustMousePosition(mousePosition);
        gesture.Add(adjustedMousePosition);
        prevMousePosition = mousePosition;
        trail.transform.position = new Vector3(adjustedMousePosition.x, adjustedMousePosition.y, 1);
        if (!trail.GetComponent<ParticleSystem>().isPlaying)
            trail.GetComponent<ParticleSystem>().Play();
        
    }
    private Vector2 adjustMousePosition(Vector3 mousePosition)
    {
        Vector2 result = new Vector2();
        result.x = (mousePosition.x / Screen.width) * 13.32f;
        result.x -= 6.66f;
        result.y = (mousePosition.y / Screen.height) * 10f;
        result.y -= 5f;
        if (result.x < -5f)
            result.x = -5f;
        if (result.x > 5f)
            result.x = 5f;
        if (result.y < -5f)
            result.y = -5f;
        if (result.y > 5f)
            result.y = 5f;
        return result;
    }
}
