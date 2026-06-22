using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class MainMenuPlayer : MonoBehaviour
{
    public int playerID;

    private void Start()
    {
        Debug.Log(name);
        if (GetComponent<PlayerInput>().devices.Count > 0)
        {
            Destroy(gameObject);
        }
    }

    public void StartGame(InputAction.CallbackContext context)
    {
        MainMenuManager.Instance.PlayerSelectionManager.StartGame();
    }

}
