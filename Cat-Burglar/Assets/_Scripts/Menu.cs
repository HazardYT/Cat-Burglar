using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public int difficulty = 1;
    public float sensitivity = 2f;
    public static Menu instance;
    public TMP_Text sensText;
    public TMP_Text difficultyText;
    public Slider sensitivitySlider;
    void Start(){
        instance = this;
        sensitivitySlider.onValueChanged.AddListener (delegate {SetSensitivity();});
    }
    public void Play(){
        SceneManager.LoadScene("Game");
    }
    public void SetSensitivity(){
        sensitivity = sensitivitySlider.value;
        sensText.text = sensitivity.ToString("0.0");
    }
    public void ChangeDifficulty(){
        if (difficulty < 2){
            difficulty++;
        }
        else difficulty = 0;
        switch(difficulty){
            case 0:
                difficultyText.text = "Easy";
                break;
            case 1:
                difficultyText.text = "Normal";
                break;
            case 2:
                difficultyText.text = "Hard";
                break;
        }
    }
    public void Quit(){
        Application.Quit();
    }
}
