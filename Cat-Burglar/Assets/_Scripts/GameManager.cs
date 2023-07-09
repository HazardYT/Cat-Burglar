using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject Player;
    public Canvas menu;
    public Canvas gameCanv;
    public TMP_Text currentTimeForMenu;
    public Image winloseScreen;
    public TMP_Text WinOrLoseText;
    public TMP_Text TimeLeftAfter;
    public TMP_Text amountCollected;
    public AudioSource audioSource;
    public AudioClip Hiss;
    public AudioClip Purr;
    public float startTime;
    public float curTime;
    public float countdownSpeed;
    string displayTime;
    int mins;
    int secs;
    bool menuon = false;
    PlayerStats playerStats;
    [SerializeField] TMP_Text timerdisplay;

    void Start(){
        Time.timeScale = 1;
        playerStats = FindObjectOfType<PlayerStats>();
        switch(Menu.difficulty){
            case 0:
                startTime = 600;
                break;
            case 1:
                startTime = 300;
                break;
            case 2:
                startTime = 150;
                break;
        }
        StartCoroutine(StartingText());
        DontDestroyOnLoad(this.gameObject);
        curTime = startTime;
    }
    IEnumerator StartingText(){
        playerStats.Hud.text = "Im all out of Catnip!, i need more or im going to die!!";
        yield return new WaitForSeconds(2f);
        playerStats.Hud.text = "";
    }
    void Update(){
        if (curTime <= 0) { Lose(); }
        curTime -= countdownSpeed * Time.deltaTime;

        mins = Mathf.RoundToInt(Mathf.Floor(curTime / 60));
        secs = Mathf.RoundToInt(curTime%60);

        if(secs == 60){secs = 59;}

        displayTime = mins.ToString("00:") + secs.ToString("00");

        timerdisplay.text = "Time Left: " + displayTime;
    }
    public void MenuToggle(){
        menuon = !menuon;
        menu.enabled = menuon;
        if (menuon){
            currentTimeForMenu.text = $"Current Time: {displayTime}";
            gameCanv.enabled = false;
            Player.GetComponent<CatController>().enabled = false;
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else {
            gameCanv.enabled = true;
            Player.GetComponent<CatController>().enabled = true;
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
    }
    public void BackToMenu(){
        SceneManager.LoadScene("Menu");
        SceneManager.UnloadSceneAsync("Game");
        Destroy(this.gameObject);
    }
    public void Retry(){
        SceneManager.UnloadSceneAsync("Game");
        SceneManager.LoadScene("Game");
        Destroy(this.gameObject);
    }
    public void Lose(){
        audioSource.clip = Hiss;
        audioSource.Play();
        Player.GetComponent<CatController>().enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        winloseScreen.gameObject.SetActive(true);
        amountCollected.text = $"You Collected: {Player.GetComponent<PlayerStats>().ItemsGrabbed} Catnip!";
        WinOrLoseText.text = "You suck lol";
        menu.enabled = false;
        gameCanv.enabled = false;
        Time.timeScale = 0;
        TimeLeftAfter.text = $"Your Time: {displayTime}";
    }
    public void Win(){
        audioSource.clip = Purr;
        audioSource.Play();
        Player.GetComponent<CatController>().enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        winloseScreen.gameObject.SetActive(true);
        amountCollected.text = $"You Collected: {Player.GetComponent<PlayerStats>().ItemsGrabbed} Catnip!";
        WinOrLoseText.text = "You Did It!";
        menu.enabled = false;
        gameCanv.enabled = false;
        Time.timeScale = 0;
        TimeLeftAfter.text = $"Your Time: {displayTime}";
    }
    public void PlayPurr(){
        audioSource.Stop();
        audioSource.clip = Purr;
        audioSource.Play();
    }
}
