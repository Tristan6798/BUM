using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleAction : MonoBehaviour
{
    public GameObject ToggleTarget;
    public InputField Reciever;
    public Dropdown Sender;
    private bool ToggleVar = false;

    public GameObject[] InvertTargets;
    public void Send()
    {
        Reciever.text = Sender.options[Sender.value].text;
        Toggle();
    }

    public void Toggle()
    {
        ToggleVar = !ToggleVar;
        ToggleTarget.SetActive(ToggleVar);
        gameObject.GetComponent<Button>().interactable = !ToggleVar;
        if (ToggleTarget.GetComponent<MiniBrowser>()!=null)
        {
            ToggleTarget.GetComponent<MiniBrowser>().HardSet();
        }
    }

    public void FlatClose()
    {
        ToggleTarget.SetActive(false);
    }

    public void SimpleInvert()
    {
        foreach (GameObject g in InvertTargets)
        {
            g.SetActive(g.activeSelf);
        }
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
