using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
        
    }


    public void LoadGameScene()
    {
        AudioManager.Instance.PlaySound(SoundType.ButtonClick);
        SceneManager.LoadScene("GameScene");
    }

}
