using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Unity.SharpZipLib.Utils;
using UnityEngine;
using UnityEngine.UI;

public class IntervalControl : MonoBehaviour
{
    public DataLog Source;

    private bool[] SaveControl;


    // Start is called before the first frame update
    void Start()
    {
        SaveControl = new bool[] {
            false,
            false,
            false,
            false,
            false
        };
    }

    private int FindMin(long[] inArray, int[] excluding)
    {
        int counter = 0;
        long minValue = long.MaxValue;
        int minSlot = 0;
        foreach (long v in inArray)
        {
            if (v < minValue)
            {
                bool notPresent = true;
                foreach (int e in excluding)
                {
                    if (v.Equals(inArray[e]))
                    {
                        //Skip! this value is already marked for deletion!
                        notPresent = false;
                    }
                }
                if (notPresent)
                {
                    minValue = v;
                    minSlot = counter;
                }
            }
            counter++;
        }
        return minSlot;
    }

    public void ManualSave()
    {
        StartCoroutine(DynamicSave("Manual"));
        //StartCoroutine(DynamicSave("Hourly"));
    }

    //add manual mode

    private List<string> BackupQueue = new List<string>();

    private bool BackupInProgress;

    IEnumerator DynamicSave(string FolderSort)
    {
        yield return new WaitForSeconds(0f);
        /*Artifact from original PalworldBUM script
        //UpdateText.text = "Shutting down server for backup";
        foreach (Process p in Process.GetProcessesByName("PalServer-Win64-Test-Cmd"))
        {
            p.CloseMainWindow();
        }
        yield return new WaitForSeconds(10f);
        yield return new WaitForSeconds(1f);*/
        string zipFile = Source.ServerBackups.text + "\\" + FolderSort + "\\Backup_" + DateTime.Today.Month + "_" + DateTime.Today.Day + "_" + DateTime.Today.Year + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + ".zip";
        if (!Directory.Exists(Source.ServerBackups.text + "\\" + FolderSort))
        {
            Directory.CreateDirectory(Source.ServerBackups.text + "\\" + FolderSort);
        }
        if (!File.Exists(zipFile))
        {
            ZipUtility.CompressFolderToZip(zipFile, null, Source.ServerDirectory.text + "\\Pal\\Saved\\SaveGames\\0");
            //UpdateText.text = "Zip created!";
            yield return new WaitForSeconds(1f);
        }
        else
        {
            //This implies saving in the same second. How is that possible?
        }

        if ((!FolderSort.Equals("Manual")) && (Source.SavesEnabled[5].isOn))
        {
            string[] backups = Directory.GetFiles(Source.ServerBackups.text + "\\" + FolderSort);
            int slot = -1;

            //Use limit values, possibly from switch case using foldersort string
            switch (FolderSort)
            {
                case ("Hourly"):
                    slot = 0;
                    break;
                case ("Daily"):
                    slot = 1;
                    break;
                case ("Weekly"):
                    slot = 2;
                    break;
                case ("Monthly"):
                    slot = 3;
                    break;
                case ("Yearly"):
                    slot = 4;
                    gameObject.transform.GetChild(0).GetChild(slot).gameObject.GetComponent<Text>().text = FolderSort + " backup complete";
                    break;
            }
            if ((slot > -1) && (slot < 4))
            {
                gameObject.transform.GetChild(0).GetChild(slot).gameObject.GetComponent<Text>().text = FolderSort + " backup complete";
                if (Source.SaveLimits[slot].text.Length != 0)//Checks if save limit is empty.
                {
                    ProcedurallyClear(backups, int.Parse(Source.SaveLimits[slot].text));
                }
            }
        }

        /*
        //UnityEngine.Debug.Log("Zip management complete");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif*/
        BackupInProgress = false;
        yield return null;
    }

    private void ProcedurallyClear(string[] backupFiles, int capacity)
    {
        long[] times = new long[backupFiles.Length];
        int counter = 0;
        foreach (string z in backupFiles)
        {
            times[counter] = File.GetCreationTime(z).ToFileTime();
            counter++;
        }
        List<int> toRemove = new List<int>();
        for (int i = capacity; i < backupFiles.Length; i++)
        {
            toRemove.Add(FindMin(times, toRemove.ToArray()));
        }
        foreach (int r in toRemove)
        {
            File.Delete(backupFiles[r]);
        }
    }

    public void ResetTracker()
    {
        LastMinute = -1;
    }

    private int LastMinute = -1;
    private bool BackupsEnabled = false;
    public GameObject BackupButton;

    //if a -1 is passed in interval, this value is not interval dependent. (probably minute or day)
    //if a -1 is passed in max, the maximum is inconsistent. Likely days.
    //might need carry in for dependent remainder
    public int[] Crunch(int current, int start, int interval, int max)
    {
        //After the first three, rough indicator is fourth, remaining is 5th
        int[] values = new int[5];
        values[0] = current;
        values[1] = start;
        values[2] = interval;
        if (values[2] != -1)
        {
            //values[3] = start % interval; Originall stored as offset, now stored as rough indicator
            values[3] = (current - (start % interval)) % interval;
            values[4] = (interval - values[3]);
        }
        else
        {
            if (max == -1)
            {
                max = GetMax(DateTime.Now.Month);
                values[2] = max;
            }

            if (start > current)
            {
                values[4] = start - current;
            }
            else
            {
                values[4] = max + (start - current);
            }
            values[3] = -1;
        }
        return values;
    }

    public int GetMax(int month)
    {
        UnityEngine.Debug.Log("Month registry says: " + month);
        switch (month - 1)
        {
            case (0):
                return 31;
            case (1):
                if ((DateTime.Now.Year % 4) == 0)
                {
                    return 29;
                }
                else
                {
                    return 28;
                }
            case (2):
                return 31;
            case (3):
                return 30;
            case (4):
                return 31;
            case (5):
                return 30;
            case (6):
                return 31;
            case (7):
                return 31;
            case (8):
                return 30;
            case (9):
                return 31;
            case (10):
                return 30;
            case (11):
                return 31;
        }
        return -1;
    }

    public void ToggleBackups()
    {
        BackupsEnabled = !BackupsEnabled;
        if (BackupsEnabled)
        {
            BackupButton.transform.GetChild(0).GetComponent<Text>().text = "Stop backup intervals";
        }
        else
        {
            BackupButton.transform.GetChild(0).GetComponent<Text>().text = "Start backup intervals";
        }
        ResetTracker();
    }

    // Update is called once per frame
    void Update()
    {
        if (!BackupInProgress && (BackupQueue.Count != 0))
        {
            BackupInProgress = true;
            StartCoroutine(DynamicSave(BackupQueue[0]));
            BackupQueue.RemoveAt(0);
        }

        //add minute tracker to reduce runtime. Trigger every time the minute changesint.Parse(sr.ReadLine());
        if (LastMinute != DateTime.Now.Minute)
        {
            LastMinute = DateTime.Now.Minute;

            int counter = 0;
            int[] minuteValues = Crunch(DateTime.Now.Minute, Source.QuickStartAccess[4], -1, 60);
            int[] hourValues = Crunch(DateTime.Now.Hour, Source.QuickStartAccess[3], int.Parse(Source.SaveIntervals[0].options[Source.SaveIntervals[0].value].text), 24);
            int[] dayValues = Crunch(DateTime.Now.Day, Source.QuickStartAccess[1], -1, -1);
            int[] weekValues = Crunch((int)DateTime.Now.DayOfWeek, Source.SaveIntervals[1].value, -1, 7);
            int[] monthValues = Crunch(DateTime.Now.Month, (Source.QuickStartAccess[0] + 1), -1, 12);
            foreach (Toggle t in Source.SavesEnabled)
            {
                gameObject.transform.GetChild(0).GetChild(counter).gameObject.SetActive(t.isOn);
                if (t.isOn)
                {

                    switch (counter)
                    {
                        case 0:

                            //BAD! needs adjustment for multi-hour periods! Also needs baseline hour!
                            /*Artifact coding, vastly improved by crunch function
                            int currentMinute = DateTime.Now.Minute;
                            int currentHour = DateTime.Now.Hour;
                            int targetHourOffset = Source.QuickStartAccess[3] % int.Parse(Source.SaveIntervals[0].options[Source.SaveIntervals[0].value].text);
                            if ((currentMinute == Source.QuickStartAccess[4]) && (((currentHour - targetHourOffset) % int.Parse(Source.SaveIntervals[0].options[Source.SaveIntervals[0].value].text)) == 0))
                            {
                                if (BackupsEnabled)
                                {
                                    if (!SaveControl[counter])
                                    {
                                        SaveControl[counter] = true;
                                        gameObject.transform.GetChild(0).GetChild(counter).gameObject.GetComponent<Text>().text = "Hourly backup in progress";
                                        StartCoroutine(DynamicSave("Hourly"));
                                    }
                                }
                                else
                                {
                                    gameObject.transform.GetChild(0).GetChild(counter).gameObject.GetComponent<Text>().text = "Hourly backup triggered, start backup intervals to begin backup saving";
                                }
                            }
                            else
                            {
                                int hoursRemaining = (int.Parse(Source.SaveIntervals[0].options[Source.SaveIntervals[0].value].text) - ((currentHour - targetHourOffset) % int.Parse(Source.SaveIntervals[0].options[Source.SaveIntervals[0].value].text)));
                                SaveControl[counter] = false;
                                int minutesRemaining = 0;
                                if (Source.QuickStartAccess[4] > currentMinute)
                                {
                                    minutesRemaining = Source.QuickStartAccess[4] - currentMinute;
                                    hoursRemaining = hoursRemaining % int.Parse(Source.SaveIntervals[0].options[Source.SaveIntervals[0].value].text);
                                }
                                else
                                {
                                    minutesRemaining = 60 + (Source.QuickStartAccess[4] - currentMinute);
                                    hoursRemaining = (hoursRemaining - 1) % int.Parse(Source.SaveIntervals[0].options[Source.SaveIntervals[0].value].text);
                                }
                                gameObject.transform.GetChild(0).GetChild(counter).gameObject.GetComponent<Text>().text = "Next backup hour in: " + hoursRemaining + " hours, " + minutesRemaining + " minutes";
                            }*/
                            if ((minuteValues[0] == minuteValues[1]) && (hourValues[3] == 0))
                            {
                                if (BackupsEnabled)
                                {
                                    if (!SaveControl[counter])
                                    {
                                        SaveControl[counter] = true;
                                        gameObject.transform.GetChild(0).GetChild(counter).gameObject.GetComponent<Text>().text = "Hourly backup in progress";
                                        if (BackupInProgress)
                                        {
                                            BackupQueue.Add("Hourly");
                                        }
                                        else
                                        {
                                            BackupInProgress = true;
                                            StartCoroutine(DynamicSave("Hourly"));
                                        }

                                    }
                                }
                                else
                                {
                                    gameObject.transform.GetChild(0).GetChild(counter).gameObject.GetComponent<Text>().text = "Hourly backup triggered, start backup intervals to begin backup saving";
                                }
                            }
                            else
                            {
                                SaveControl[counter] = false;
                                if (minuteValues[1] > minuteValues[0])
                                {
                                    hourValues[4] = hourValues[4] % hourValues[2];
                                }
                                else
                                {
                                    hourValues[4] = (hourValues[4] - 1) % hourValues[2];
                                }
                                gameObject.transform.GetChild(0).GetChild(counter).gameObject.GetComponent<Text>().text = "Next backup hour in: " + hourValues[4] + " hours, " + minuteValues[4] + " minutes";
                            }

                            break;
                        case 1:
                            hourValues = Crunch(DateTime.Now.Hour, Source.QuickStartAccess[3], -1, 24);
                            if ((hourValues[0] == hourValues[1]) && (minuteValues[1] == minuteValues[0]))
                            {
                                if (BackupsEnabled)
                                {
                                    if (!SaveControl[counter])
                                    {
                                        SaveControl[counter] = true;
                                        gameObject.transform.GetChild(0).GetChild(counter).gameObject.GetComponent<Text>().text = "Daily backup in progress";
                                        if (BackupInProgress)
                                        {
                                            BackupQueue.Add("Daily");
                                        }
                                        else
                                        {
                                            BackupInProgress = true;
                                            StartCoroutine(DynamicSave("Daily"));
                                        }
                                    }
                                }
                                else
                                {
                                    gameObject.transform.GetChild(0).GetChild(counter).gameObject.GetComponent<Text>().text = "Daily backup triggered, start backup intervals to begin backup saving";
                                }
                            }
                            else
                            {
                                if (hourValues[1] <= hourValues[0])
                                {
                                    if (minuteValues[1] < minuteValues[0])
                                    {
                                        hourValues[4] = (hourValues[4] - 1);
                                    }
                                }
                                hourValues[4] %= 24;
                                SaveControl[counter] = false;
                                gameObject.transform.GetChild(0).GetChild(counter).gameObject.GetComponent<Text>().text = "Next backup day in: " + hourValues[4] + " hours, " + minuteValues[4] + " minutes";
                            }
                            break;
                        case 2:
                            if (weekValues[1] <= weekValues[0])
                            {
                                if (hourValues[1] <= hourValues[0])
                                {
                                    if (minuteValues[1] < minuteValues[0])
                                    {
                                        weekValues[4] = (weekValues[4] - 1);
                                    }
                                }
                            }
                            if ((weekValues[0] == weekValues[1]) && (hourValues[0] == hourValues[1]) && (minuteValues[1] == minuteValues[0]))
                            {
                                if (BackupsEnabled)
                                {
                                    if (!SaveControl[counter])
                                    {
                                        SaveControl[counter] = true;
                                        gameObject.transform.GetChild(0).GetChild(counter).gameObject.GetComponent<Text>().text = "Weekly backup in progress";
                                        if (BackupInProgress)
                                        {
                                            BackupQueue.Add("Weekly");
                                        }
                                        else
                                        {
                                            BackupInProgress = true;
                                            StartCoroutine(DynamicSave("Weekly"));
                                        }

                                    }
                                }
                                else
                                {
                                    gameObject.transform.GetChild(0).GetChild(counter).gameObject.GetComponent<Text>().text = "Weekly backup triggered, start backup intervals to begin backup saving";
                                }
                            }
                            else
                            {
                                SaveControl[counter] = false;
                                weekValues[4] %= 7;
                                gameObject.transform.GetChild(0).GetChild(counter).gameObject.GetComponent<Text>().text = "Next backup week in: " + weekValues[4] + " days, " + hourValues[4] + " hours, " + minuteValues[4] + " minutes";
                            }

                            break;
                        case 3:
                            if (dayValues[1] <= dayValues[0])
                            {
                                if (hourValues[1] <= hourValues[0])
                                {
                                    if (minuteValues[1] < minuteValues[0])
                                    {
                                        dayValues[4] = (dayValues[4] - 1); //Unneccessary?
                                    }
                                }
                            }
                            if ((dayValues[0] == dayValues[1]) && (hourValues[0] == hourValues[1]) && (minuteValues[1] == minuteValues[0]))
                            {
                                if (BackupsEnabled)
                                {
                                    if (!SaveControl[counter])
                                    {
                                        SaveControl[counter] = true;
                                        gameObject.transform.GetChild(0).GetChild(counter).gameObject.GetComponent<Text>().text = "Monthly backup in progress";
                                        if (BackupInProgress)
                                        {
                                            BackupQueue.Add("Monthly");
                                        }
                                        else
                                        {
                                            BackupInProgress = true;
                                            StartCoroutine(DynamicSave("Monthly"));
                                        }

                                    }
                                }
                                else
                                {
                                    gameObject.transform.GetChild(0).GetChild(counter).gameObject.GetComponent<Text>().text = "Monthly backup triggered, start backup intervals to begin backup saving";
                                }
                            }
                            else
                            {
                                SaveControl[counter] = false;
                                dayValues[4] %= dayValues[2];
                                gameObject.transform.GetChild(0).GetChild(counter).gameObject.GetComponent<Text>().text = "Next backup month in: " + dayValues[4] + " days, " + hourValues[4] + " hours, " + minuteValues[4] + " minutes";
                            }

                            break;
                        case 4:
                            if (monthValues[1] <= monthValues[0])
                            {
                                if (dayValues[1] <= dayValues[0])
                                {
                                    if (hourValues[1] <= hourValues[0])
                                    {
                                        if (minuteValues[1] < minuteValues[0])
                                        {
                                            monthValues[4] = (monthValues[4] - 1); //Unneccessary?
                                        }
                                    }
                                }
                            }
                            if ((monthValues[0] == monthValues[1]) && (dayValues[0] == dayValues[1]) && (hourValues[0] == hourValues[1]) && (minuteValues[1] == minuteValues[0]))
                            {
                                if (BackupsEnabled)
                                {
                                    if (!SaveControl[counter])
                                    {
                                        SaveControl[counter] = true;
                                        gameObject.transform.GetChild(0).GetChild(counter).gameObject.GetComponent<Text>().text = "Yearly backup in progress";
                                        if (BackupInProgress)
                                        {
                                            BackupQueue.Add("Yearly");
                                        }
                                        else
                                        {
                                            BackupInProgress = true;
                                            StartCoroutine(DynamicSave("Yearly"));
                                        }

                                    }
                                }
                                else
                                {
                                    gameObject.transform.GetChild(0).GetChild(counter).gameObject.GetComponent<Text>().text = "Yearly backup triggered, start backup intervals to begin backup saving";
                                }
                            }
                            else
                            {
                                SaveControl[counter] = false;
                                monthValues[4] %= 12;
                                gameObject.transform.GetChild(0).GetChild(counter).gameObject.GetComponent<Text>().text = "Next backup year in: " + monthValues[4] + " months, " + dayValues[4] + " days, " + hourValues[4] + " hours, " + minuteValues[4] + " minutes";
                            }

                            break;
                    }
                }
                counter++;
            }
        }
    }
}