using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowController : MonoBehaviour
{
    public GameObject[] SisterWindows;
    public Button[] SisterTriggers;
    public GameObject TargetWindow;

    public void SwapWindow()
    {
        foreach (GameObject g in SisterWindows)
        {
            g.SetActive(false);
        }
        foreach (Button b in SisterTriggers)
        {
            b.interactable = true;
        }
        TargetWindow.SetActive(true);

        //this script should only ever be given to buttons connected to the target
        gameObject.GetComponent<Button>().interactable = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
