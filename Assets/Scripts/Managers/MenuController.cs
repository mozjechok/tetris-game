using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;

public class MenuController : MonoBehaviour
{
    private SoundManager soundManager;
    private ScoreManager scoreManager;

    public GameObject shopPanel;

    public bool isShop = false;

    private void Start()
    {
        soundManager = GameObject.FindObjectOfType<SoundManager>();
        scoreManager = GameObject.FindObjectOfType<ScoreManager>();

        if (!soundManager)
            Debug.Log("No sound manager");

        if (!scoreManager)
            Debug.Log("No score manager");

        scoreManager.LoadGame();
        scoreManager.metaBricks++;
        scoreManager.SaveGame();

        if (shopPanel)
            shopPanel.SetActive(false);
    }

    private void PlayerInput()
    {
        //nothing?
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
        if (!soundManager || !scoreManager)
            return;

        PlayerInput();
    }

    public void Play()
    {
        SceneManager.LoadScene("Game");
    }

    public void Meta()
    {
        SceneManager.LoadScene("Meta");
    }

    public void ToggleShop()
    {
        isShop = !isShop;

        if (shopPanel)
        {
            shopPanel.SetActive(isShop);

            if (soundManager)
            {
                soundManager.musicSource.volume =
                    isShop ? soundManager.musicVolume * 0.25f : soundManager.musicVolume;
            }

            Time.timeScale = isShop ? 0 : 1;
        }
    }
}

