using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    [SerializeField]
    private InputManager inputManager;

    private void Start()
    {
        inputManager.OnMainMenuInput += BackToMainMenu;
    }

    private void OnDestroy()
    {
        inputManager.OnMainMenuInput -= BackToMainMenu;
    }

    private void BackToMainMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
    }
}
