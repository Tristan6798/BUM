using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class HyperlinkLauncher : MonoBehaviour
{
    public string url = "https://www.patreon.com/geraldspark";

    public void Launch()
    {
        Process.Start(url);
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
