using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menu_return : MonoBehaviour
{
    public void menuReturn()
    {
        SceneManager.LoadScene("Assets/rewind/Examples/scenes/menu.unity");
    }
}
