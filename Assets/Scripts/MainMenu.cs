using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaInMenu : MonoBehaviour
{

    public void QuitGame ()
    {
        Debug.Log("Game Ended.");
        Application.Quit();
    }

}
