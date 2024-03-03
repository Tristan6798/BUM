using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MiniBrowser : MonoBehaviour
{
    public Dropdown DirectoryList;
    private List<int> ValidDirectory;
    private List<int> InvalidDirectory;
    private List<string> BlacklistDirectories;

    public bool ExeDetect;
    //Should be remade to get options list from function for both RefreshList and
    //JumpUp to accomodate for new function StartFrom which will accept a string
    //directory in the inputfield. Make sure it catches for invalid directories.

    public InputField Target;

    public void HardSet()
    {
        try{
            DirectoryList.ClearOptions();
            List<Dropdown.OptionData> drives = new List<Dropdown.OptionData>();
            drives.Add(new Dropdown.OptionData(Target.text));
            drives.Add(new Dropdown.OptionData(Target.text));
            DirectoryList.AddOptions(drives);
            DirectoryList.value = 1;
            RefreshList();
        }
        catch
        {
            Debug.Log("Hard set failed!");
            GenerateDrives();
        }
    }

    public void RefreshList()
    {
        if (DirectoryList.value > 0)
        {
            try
            {
                //get currently selected option change to be 0 in new list
                List<Dropdown.OptionData> drives = new List<Dropdown.OptionData>();
                drives.Add(new Dropdown.OptionData(DirectoryList.options[DirectoryList.value].text));
                //generate new list based on current
                int counter = 0;
                List<int> tempInvalidDirectory = new List<int>();
                //List<int> tempValidDirectory = new List<int>();
                counter++;
                foreach (string s in Directory.GetDirectories(drives[0].text))
                {
                    try
                    {
                        foreach (string a in BlacklistDirectories)
                        {
                            if (a.Equals(s))
                            {
                                tempInvalidDirectory.Add(counter);
                                break;
                            }
                        }
                        if (ExeDetect) {
                            foreach (string c in Directory.GetFiles(s))
                            {
                                if (c.EndsWith("PalServer.exe"))
                                {
                                    ValidDirectory.Add(counter);
                                }
                            }
                        }
                    }
                    catch
                    {
                        //bad fetch
                    }
                    drives.Add(new Dropdown.OptionData(s));
                    counter++;
                }
                DirectoryList.ClearOptions();
                DirectoryList.AddOptions(drives);
                ValidDirectory = new List<int>();
                InvalidDirectory = new List<int>();
                foreach (int i in tempInvalidDirectory)
                {
                    InvalidDirectory.Add(i);
                }
                //highlight valid directory as green where executable is detected.
            }
            catch{
                Debug.Log("Adding "+ DirectoryList.options[DirectoryList.value].text);
                InvalidDirectory.Add(DirectoryList.value);
                bool notFound = true;
                foreach (string a in BlacklistDirectories)
                {
                    if (a.Equals(DirectoryList.options[DirectoryList.value].text))
                    {
                        notFound = false;
                    }
                }
                if (notFound)
                {
                    BlacklistDirectories.Add(DirectoryList.options[DirectoryList.value].text);
                }
                DirectoryList.value = 0;
            }
        }
    }

    public void GenerateDrives()
    {
        DirectoryList.ClearOptions();
        List<Dropdown.OptionData> drives = new List<Dropdown.OptionData>();
        drives.Add(new Dropdown.OptionData("Select Drive"));
        foreach (string s in Directory.GetLogicalDrives())
        {
            drives.Add(new Dropdown.OptionData(s));
        }
        DirectoryList.ClearOptions();
        DirectoryList.AddOptions(drives);
        InvalidDirectory = new List<int>();
        ValidDirectory = new List<int>();
        BlacklistDirectories = new List<string>();
    }

    public void JumpUp()
    {
        DirectoryList.Hide();
        //Debug.Log("Hello?");
        if ((!DirectoryList.options[0].text.Equals("Select Drive")) && (DirectoryList.options[0].text.Length > 4))
        {
            bool falsePositive = false;
            bool backslashDetect = false;
            bool secondBackslashDetect = false;
            string newDir = "";
            for (int i = DirectoryList.options[0].text.Length - 1; i > -1; i--)
            {
                if (backslashDetect)
                {
                    if (DirectoryList.options[0].text[i].Equals('\\'))
                    {
                        secondBackslashDetect = true;
                    }
                    newDir = DirectoryList.options[0].text[i] + newDir;
                }
                else if (DirectoryList.options[0].text[i].Equals('\\'))
                {
                        backslashDetect = true;
                    if (DirectoryList.options[0].text.EndsWith('\\'))
                    {
                        if (!falsePositive)
                        {
                            backslashDetect = false;
                        }
                        falsePositive = true;
                    }
                }
            }
            if (!secondBackslashDetect)
            {
                newDir = "";
                Debug.Log("It's probably a drive letter");
                for (int i = 0; i < DirectoryList.options[0].text.Length; i++)
                {
                    if (DirectoryList.options[0].text[i].Equals('\\'))
                    {
                        newDir = newDir + DirectoryList.options[0].text[i];
                        break;
                    }
                    else
                    {
                        newDir = newDir + DirectoryList.options[0].text[i];
                    }
                }
                Debug.Log("NewDir now: "+newDir);
            }
            try
            {
                //get currently selected option change to be 0 in new list
                List<Dropdown.OptionData> drives = new List<Dropdown.OptionData>();
                drives.Add(new Dropdown.OptionData(newDir));
                //generate new list based on current
                int counter = 0;
                List<int> tempInvalidDirectory = new List<int>();
                //List<int> tempValidDirectory = new List<int>();
                counter++;
                foreach (string s in Directory.GetDirectories(drives[0].text))
                {
                    try
                    {
                        foreach (string a in BlacklistDirectories)
                        {
                            if (a.Equals(s))
                            {
                                tempInvalidDirectory.Add(counter);
                                break;
                            }
                        }
                        if (ExeDetect) {
                            foreach (string c in Directory.GetFiles(s))
                            {
                                if (c.EndsWith("PalServer.exe"))
                                {
                                    ValidDirectory.Add(counter);
                                }
                            }
                        }
                    }
                    catch
                    {
                        //bad fetch
                    }
                    drives.Add(new Dropdown.OptionData(s));
                    counter++;
                }
                DirectoryList.ClearOptions();
                DirectoryList.AddOptions(drives);
                InvalidDirectory = new List<int>();
                ValidDirectory = new List<int>();
                foreach (int i in tempInvalidDirectory)
                {
                    InvalidDirectory.Add(i);
                }
                //highlight valid directory as green where executable is detected.
            }
            catch
            {
                UnityEngine.Debug.Log("Oh noes");
            }
        }
        else
        {
            GenerateDrives();
        }
        //check if there is only one \\. if there is, reload logical drives.

        //chop off from the last \\ forward
    }

    private int BaseSize;
    // Start is called before the first frame update
    void Start()
    {
        ValidDirectory = new List<int>();
        InvalidDirectory = new List<int>();
        BaseSize = DirectoryList.gameObject.transform.childCount;
        HardSet();
    }

    // Update is called once per frame
    void Update()
    {
        DirectoryList.Show();
        DestroyImmediate(GameObject.Find("Blocker"));
        if (DirectoryList.gameObject.transform.childCount > BaseSize)
        {
            foreach (int i in ValidDirectory)
            {
                Debug.Log("i = " + i);
                if (i < DirectoryList.gameObject.transform.GetChild(BaseSize).GetChild(0).GetChild(0).childCount)
                {
                    DirectoryList.gameObject.transform.GetChild(BaseSize).GetChild(0).GetChild(0).GetChild(i + 2).GetChild(2).gameObject.GetComponent<Text>().color = new Color(0f,0.75f,0f);
                }
            }
            foreach (int i in InvalidDirectory)
            {
                Debug.Log("i = " + i);
                if (i < DirectoryList.gameObject.transform.GetChild(BaseSize).GetChild(0).GetChild(0).childCount)
                {
                    DirectoryList.gameObject.transform.GetChild(BaseSize).GetChild(0).GetChild(0).GetChild(i + 1).gameObject.GetComponent<Toggle>().interactable = false;
                }
            }
        }
        if (ExeDetect) {
            if (!DirectoryList.options[DirectoryList.value].text.Equals("Select Drive")) {
                try
                {
                    foreach (string c in Directory.GetFiles(DirectoryList.options[DirectoryList.value].text))
                    {
                        if (c.EndsWith("PalServer.exe"))
                        {
                            DirectoryList.gameObject.transform.gameObject.GetComponent<Image>().color = Color.green;
                        }
                    }
                }
                catch { }
            }
        }
    }
}