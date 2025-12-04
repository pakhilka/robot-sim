using UnityEngine;
using UnityEngine.SceneManagement; // добавили, чтобы грузить сцены

public class MainMenuController : MonoBehaviour
{
    // Метод для кнопки "Start"
    public void OnStartSimulation()
    {
        // Имя должно совпадать с именем сцены, которую ты создал: Level01
        SceneManager.LoadScene("Level01");
    }
}
