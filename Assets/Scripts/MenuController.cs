using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject firstSelectedButton;

    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(null); // clear previous
        EventSystem.current.SetSelectedGameObject(firstSelectedButton); // select default
    }
}
