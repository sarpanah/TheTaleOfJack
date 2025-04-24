using UnityEngine;
using UnityEngine.SceneManagement;
public class TEMP_Restart_Game : MonoBehaviour
{
    public void Restart(){
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
