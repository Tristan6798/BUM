using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DataLog : MonoBehaviour
{
    private string TrueDataPath = "";
    public InputField ServerProfile;
    public InputField ServerDirectory;
    public InputField ServerBackups;
    public Toggle[] SavesEnabled;
    public Dropdown[] SaveIntervals;
    public InputField[] SaveLimits;
    public GameObject StartPoint;
    public int[] QuickStartAccess = new int[5];

    public GameObject[] ErrorCodes;
    public GameObject ProfileDisplay;



    private void SaveProfile()
    {
        if (File.Exists(TrueDataPath + "/" + ServerProfile.text + "/Settings.txt"))
        {
            File.Delete(TrueDataPath + "/" + ServerProfile.text + "/Settings.txt");
        }
        StreamWriter sw = new StreamWriter(TrueDataPath+"/"+ServerProfile.text+"/Settings.txt");
        sw.WriteLine(ServerDirectory.text);
        sw.WriteLine(ServerBackups.text);
        foreach (Toggle b in SavesEnabled)
        {
            sw.WriteLine(b.isOn.ToString());
        }
        foreach (Dropdown d in SaveIntervals)
        {
            sw.WriteLine(d.value.ToString());
        }
        foreach (InputField i in SaveLimits)
        {
            sw.WriteLine(i.text.ToString());
        }

        sw.WriteLine(StartPoint.transform.GetChild(0).gameObject.GetComponent<Dropdown>().value);
        sw.WriteLine(StartPoint.transform.GetChild(1).gameObject.GetComponent<Dropdown>().value);
        sw.WriteLine(StartPoint.transform.GetChild(2).gameObject.GetComponent<InputField>().text);
        sw.WriteLine(StartPoint.transform.GetChild(3).gameObject.GetComponent<Dropdown>().value);
        sw.WriteLine(StartPoint.transform.GetChild(4).gameObject.GetComponent<Dropdown>().value);
        sw.Close();
    }

    private void LoadProfile()
    {
        Debug.Log("Loading from: "+ TrueDataPath + "/" + ServerProfile.text + "/Settings.txt");
        StreamReader sr = new StreamReader(TrueDataPath + "/" + ServerProfile.text + "/Settings.txt");
        try
        {
            ServerDirectory.text = sr.ReadLine();
            ServerBackups.text = sr.ReadLine();
            for (int i = 0; i < SavesEnabled.Length; i++)
            {
                SavesEnabled[i].isOn = bool.Parse(sr.ReadLine());
            }
            for (int i = 0; i < SaveIntervals.Length; i++)
            {
                SaveIntervals[i].value = int.Parse(sr.ReadLine());
            }
            for (int i = 0; i < SaveLimits.Length; i++)
            {
                SaveLimits[i].text = sr.ReadLine();
            }
            StartPoint.transform.GetChild(0).gameObject.GetComponent<Dropdown>().value = int.Parse(sr.ReadLine());
            StartPoint.transform.GetChild(1).gameObject.GetComponent<Dropdown>().value = int.Parse(sr.ReadLine());
            StartPoint.transform.GetChild(2).gameObject.GetComponent<InputField>().text = sr.ReadLine();
            StartPoint.transform.GetChild(3).gameObject.GetComponent<Dropdown>().value = int.Parse(sr.ReadLine());
            StartPoint.transform.GetChild(4).gameObject.GetComponent<Dropdown>().value = int.Parse(sr.ReadLine());
        }
        catch
        {

        }
        sr.Close();
        //Debug.Log("Creation time: " +File.GetCreationTime(ServerDirectory.text + "\\PalServer.exe").ToFileTime());
    }


    //Should be called when accessing home, displaying save starting if data is entered correctly.
    public void UpdateProfile()
    {
        foreach (GameObject g in ErrorCodes)
        {
            g.SetActive(false);
        }

        if (ServerProfile.text.Equals(""))
        {
            string proString = "";
            //no profile set! Either first time startup or left blank. Check folder and load!
            foreach (string pro in Directory.GetDirectories(TrueDataPath))
            {
                //Read primary folder, load
                for (int i = pro.Length - 1; i > -1; i--)
                {
                    if ((!pro[i].Equals('/')) && (!pro[i].Equals('\\')))
                    {
                        proString = pro[i] + proString;
                    }
                    else
                    {
                        break;
                    }
                }
                ServerProfile.text = proString;
                break;
            }
            if (proString.Equals(""))
            {
                //If there is no folder, display that server profile needs to be set!
                ErrorCodes[0].SetActive(true);
            }
            else
            {
                if (File.Exists(TrueDataPath + "/" + ServerProfile.text + "/Settings.txt"))
                {
                    LoadProfile();
                    UpdateProfile();
                    //load relevant profile
                }
                else
                {
                    //No settings detected. Setup must have been incomplete. Try function again to see what's missing.
                    UpdateProfile();
                }
            }
        }
        else
        {
            if (!Directory.Exists(TrueDataPath + "/" + ServerProfile.text))
            {
                CreateProfile(); // should have been confirmed in calibration, user might not have.
            }
            bool dirCheckSuc = true;
            //checking directory is valid
            if (ServerDirectory.text.Equals(""))
            {
                //Directory not provided!
                ErrorCodes[1].SetActive(true);
                dirCheckSuc = false;
            }
            else
            {
                if (Directory.Exists(ServerDirectory.text))
                {
                    try
                    {
                        bool exeFound = false;

                        //checking all files in directory. Should be retrofitted later to take server profile type and search for relevant executable instead.
                        foreach (string storedFiles in Directory.GetFiles(ServerDirectory.text))
                        {
                            if (storedFiles.EndsWith("PalServer.exe"))
                            {
                                //Success! Directory confirmed
                                exeFound = true;
                                break;
                            }
                        }
                        if (!exeFound)
                        {
                            Debug.Log("Error loc 1");
                            dirCheckSuc = false;
                            ErrorCodes[2].SetActive(true);
                            //failed to find executable. Display message to make sure this is the correct directory.
                        }
                    }
                    catch
                    {
                        Debug.Log("Error loc 2");
                        dirCheckSuc = false;
                        ErrorCodes[2].SetActive(true);
                        //treat as else, something went wrong!
                    }
                }
                else
                {
                    Debug.Log("Error loc 3");
                    dirCheckSuc = false;
                    ErrorCodes[2].SetActive(true);
                    //Directory is invalid. Display message for checking directory and that this directory doesn't exist!
                }
            }
            //simple check for 
            if (!ServerBackups.text.Equals(""))
            {
                if (!Directory.Exists(ServerBackups.text))
                {
                    Directory.CreateDirectory(ServerBackups.text);
                }
            }
            else
            {
                if (dirCheckSuc)
                {
                    ErrorCodes[3].SetActive(true);
                    dirCheckSuc = false;
                }
            }
            //Beyond this point, profile should be saved and used if everything checks out!
            SaveProfile();

            //enable display control
            ProfileDisplay.SetActive(dirCheckSuc);
            if (dirCheckSuc)
            {
                ProfileDisplay.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Server profile in use: "+ServerProfile;
                QuickStartAccess[0] = StartPoint.transform.GetChild(0).gameObject.GetComponent<Dropdown>().value;
                QuickStartAccess[1] = int.Parse(StartPoint.transform.GetChild(1).gameObject.GetComponent<Dropdown>().options[StartPoint.transform.GetChild(1).gameObject.GetComponent<Dropdown>().value].text);
                QuickStartAccess[2] = int.Parse(StartPoint.transform.GetChild(2).gameObject.GetComponent<InputField>().text);
                QuickStartAccess[3] = int.Parse(StartPoint.transform.GetChild(3).gameObject.GetComponent<Dropdown>().options[StartPoint.transform.GetChild(3).gameObject.GetComponent<Dropdown>().value].text);
                QuickStartAccess[4] = int.Parse(StartPoint.transform.GetChild(4).gameObject.GetComponent<Dropdown>().options[StartPoint.transform.GetChild(4).gameObject.GetComponent<Dropdown>().value].text);
                IntervalDisplay.ResetTracker();
            }
        }
    }

    public IntervalControl IntervalDisplay;

    public void CreateProfile()
    {
        Directory.CreateDirectory(TrueDataPath+"/"+ServerProfile.text);
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (char c in Application.persistentDataPath)
        {
            TrueDataPath += c;
            if (TrueDataPath.EndsWith("AppData"))
            {
                TrueDataPath += "/Local/PalBUM";
                break;
            }
        }
        if (!Directory.Exists(TrueDataPath))
        {
            Directory.CreateDirectory(TrueDataPath);
        }
        Debug.Log("Persistent data oath is: "+TrueDataPath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
