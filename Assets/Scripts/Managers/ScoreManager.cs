using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    private int score = 0;
    public int level = 1;
    public int gold = 0;
    public int metaBricks = 0;

    public Text levelText;
    public Text scoreText;
    public Text goldText;
    public Text metaBricksText;

    public bool didLevelUp = false;

    private const int minLines = 1;
    private const int maxLines = 4;

    private int nextLevelScore = 0;

    public ParticlePlayer levelUpFx;

    private void Start() => LoadGame();

    private void UpdateUIText()
    {
        if (levelText)
            levelText.text = level.ToString();

        if (scoreText)
            scoreText.text = score.ToString();

        if (goldText)
            goldText.text = gold.ToString();

        if (metaBricksText)
            metaBricksText.text = metaBricks.ToString();
    }

    public void NewShape()
    {
        score += 5;

        if (score >= nextLevelScore)
            LevelUp();

        UpdateUIText();
    }

    public void ScoreLines(int n)
    {
        didLevelUp = false;

        n = Mathf.Clamp(n, minLines, maxLines);

        float multiplier = 1;

        switch (n)
        {
            case 1:
                multiplier *= 1;
                break;
            case 2:
                multiplier *= 1.5f;
                break;
            case 3:
                multiplier *= 2.5f;
                break;
            case 4:
                multiplier *= 5;
                break;
        }

        score += (int)(40 * multiplier);
        gold += (int)(4 * multiplier);

        if (score >= nextLevelScore)
            LevelUp();

        UpdateUIText(); 
    }

    public void LoadGame()
    {
        gold = 0;
        if (PlayerPrefs.HasKey("gold"))
            gold = PlayerPrefs.GetInt("gold");

        metaBricks = 0;
        if (PlayerPrefs.HasKey("metaBricks"))
            metaBricks = PlayerPrefs.GetInt("metaBricks");

        level = 1;
        nextLevelScore = 100;
        UpdateUIText();
    }

    public void SaveGame()
    {
        PlayerPrefs.SetInt("gold", gold);
        PlayerPrefs.SetInt("metaBricks", metaBricks);
    }

    public void LevelUp()
    {
        level++;
        didLevelUp = true;

        nextLevelScore = (int)Mathf.Pow(nextLevelScore, 1.3f);

        if (levelUpFx)
            levelUpFx.Play();
    }
}
