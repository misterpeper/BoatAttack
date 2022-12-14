#if UNITY_SWITCH

// Adapted from demo example:
// https://developer.nintendo.com/group/development/g1kr9vj6/tech-info/sdsg_knowledge_base/unity/-/knowledge_base/sdsg-kb-for-unity/286312752

using UnityEngine;
using System;
using System.Text;

using nn;
using nn.fs;

/// This static class handles all initialization and error handling for reading and writing save data.
/// Although this sample only saves and loads string data, the same concept can be used to save other data types
/// when combined with various serialization techniques.
public class NXSave : MonoBehaviour
{
    private static NXSave _instance;
    public static NXSave Intance => _instance;

    // Stores the name of the save data mount point. This is like the drive letter.
    // You can change these names if you want.
    private static readonly string _mountName = "MountName";
    private static readonly string _fileName = "SaveFileName";

    // Don't allow player to write to disk too often.
    // The lotcheck guideline 0011 states an average of 32 saves per minute on average but I limit it to once every 30 seconds to be extra safe.
    // Feel free to change this number to whatever works best for your game.
    private const float minimumDelayBetweenSaves = 15f;
    // Remembers the last Time.time that data was written to disk so we don't save too often.
    private static float lastSaveTime = 0f;

    #region Start

    void Awake()
    {
        Debug.LogError("!!! AWAKE (NXSAVE) !!!");
        Debug.LogError($"!!! INSTANCE != NULL : {_instance != null} !!!");
        Debug.LogError($"!!! INSTANCE != this : {_instance != this} !!!");

        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }

        Initialize();
    }

    /// Gets the preselected user information and mounts the save data drive.
    public static void Initialize()
    {
        // Be sure to initialise the user account first (opened the user, etc.)
        NXScript.Initialize();

        // I cache the userId in another script. You can get a reference to it here

        nn.account.Uid userId = NXScript.GetUserID();
        nn.Result result;

        // Mount the save data archive as "save" for the selected user account.
        Debug.Log("Mounting save data archive");
        result = nn.fs.SaveData.Mount(_mountName, userId);

        // This error handling is optional.
        // The mount operation will not fail unless the save data is already mounted or the mount name is in use.
        if (nn.fs.FileSystem.ResultTargetLocked.Includes(result))
        {
            // Save data for specified user ID is already mounted. Get account name and display an error.
            nn.account.Nickname nickname = new nn.account.Nickname();
            nn.account.Account.GetNickname(ref nickname, userId);
            Debug.LogErrorFormat("The save data for {0} is already mounted: {1}", nickname.name, result.ToString());
        }
        else if (nn.fs.FileSystem.ResultMountNameAlreadyExists.Includes(result))
        {
            // The specified mount name is already in use.
            Debug.LogErrorFormat("The mount name '{0}' is already in use: {1}", _mountName, result.ToString());
        }

        // Abort if any of the initialization steps failed.
        result.abortUnlessSuccess();
    }

    #endregion

    #region Save/Load

    #region Load

    /// Loads user account save data from the specified filename into the specified string buffer.
    /// If the specified filename is not found, the content of the buffer is unchanged.
    public static byte[] Load(ref bool successful)
    {
        // An nn.Result object is used to get the result of NintendoSDK plug-in operations.
        nn.Result result;

        // Append the filename to the mount name to get the full path to the save data file.
        string filePath = string.Format("{0}:/{1}", _mountName, _fileName);

        // The NintendoSDK plug-in uses a FileHandle object for file operations.
        nn.fs.FileHandle handle = new nn.fs.FileHandle();

        // Attempt to open the file in read-only mode.
        result = nn.fs.File.Open(ref handle, filePath, nn.fs.OpenFileMode.Read);
        if (!result.IsSuccess())
        {
            if (nn.fs.FileSystem.ResultPathNotFound.Includes(result))
            {
                Debug.LogFormat("File not found: {0}", filePath);

                successful = false;
                return null;
            }
            else
            {
                Debug.LogErrorFormat("Unable to open {0}: {1}", filePath, result.ToString());

                successful = false;
                return null;
            }
        }

        // Get the file size.
        long fileSize = 0;
        nn.fs.File.GetSize(ref fileSize, handle);
        // Allocate a buffer that matches the file size.
        byte[] data = new byte[fileSize];
        // Read the save data into the buffer.
        nn.fs.File.Read(handle, 0, data, fileSize);
        // Close the file.
        nn.fs.File.Close(handle);

        successful = true;

        return data;
    }

    #endregion

    #region Save

    /// Saves the specified string to the specified file located in the user account save data.
    /// This method prevents the application from exiting until the write operation is complete.
    public static bool Save(byte[] saveData, bool ignoreFrequencyLimit)
    {
        if (!ignoreFrequencyLimit && !FrequencyIsSafe())
        {
            Debug.Log("Save cancelled due to frequency restriction.");
            return false;
        }

#if UNITY_SWITCH && !UNITY_EDITOR
                // This method prevents the user from quitting the game while saving.
                // This is required for Nintendo Switch Guideline 0080
                // This method must be called on the main thread.
                UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();
#endif

        bool result = _Save(saveData, ignoreFrequencyLimit);

#if UNITY_SWITCH && !UNITY_EDITOR
                // Stop preventing the system from terminating the game while saving.
                UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
#endif

        return result;
    }

    /// Saves the specified string to the specified file located in the user account save data.
    /// Use this version of the Save method if you are calling it from something other than the main thread.
    /// Be sure to call UnityEngine.Switch.Notification.EnterExitRequestHandlingSection from the main thread before invoking this function.
    static bool _Save(byte[] saveData, bool ignoreFrequencyLimit)
    {
        if (!ignoreFrequencyLimit && !FrequencyIsSafe())
        {
            Debug.Log("Save cancelled due to frequency restriction.");
            return false;
        }

        lastSaveTime = Time.time;

        // An nn.Result object is used to get the result of NintendoSDK plug-in operations.
        nn.Result result;

        // Append the filename to the mount name to get the full path to the save data file.
        string filePath = string.Format("{0}:/{1}", _mountName, _fileName);

        // Convert the text to UTF-8-encoded bytes.
        // byte[] data = Encoding.UTF8.GetBytes(saveData);

        // The NintendoSDK plug-in uses a FileHandle object for file operations.
        nn.fs.FileHandle handle = new nn.fs.FileHandle();

        while (true)
        {
            // Attempt to open the file in write mode.
            result = nn.fs.File.Open(ref handle, filePath, nn.fs.OpenFileMode.Write);
            // Check if file was opened successfully.
            if (result.IsSuccess())
            {
                // Exit the loop because the file was successfully opened.
                break;
            }
            else
            {
                if (nn.fs.FileSystem.ResultPathNotFound.Includes(result))
                {
                    // Create a file with the size of the encoded data if no entry exists.
                    result = nn.fs.File.Create(filePath, saveData.LongLength);

                    // Check if the file was successfully created.
                    if (!result.IsSuccess())
                    {
                        Debug.LogErrorFormat("Failed to create {0}: {1}", filePath, result.ToString());

                        return false;
                    }
                }
                else
                {
                    // Generic fallback error handling for debugging purposes.
                    Debug.LogErrorFormat("Failed to open {0}: {1}", filePath, result.ToString());

                    return false;
                }
            }
        }

        // Write the data to the journaling area.
        // Verify that the journaling area is sufficiently large to hold the encoded data.
        // The size of the journaling area is set under PlayerSettings > Publishing Settings > User Account and Save Data > Save Data Journal Size.
        // NOTE: 32 KB of the journaling area is used by the file system.
        //       The file system uses a 16 KB block size, so each file will consume at least 16384 bytes.
        Debug.LogFormat("Writing {0} bytes to save data...", saveData.LongLength);

        // Set the file to the size of the binary data.
        result = nn.fs.File.SetSize(handle, saveData.LongLength);

        // You do not need to handle this error if you are sure there will be enough space.
        if (nn.fs.FileSystem.ResultUsableSpaceNotEnough.Includes(result))
        {
            Debug.LogErrorFormat("Insufficient space to write {0} bytes to {1}", saveData.LongLength, filePath);

            return false;
        }

        // NOTE: Calling File.Write() with WriteOption.Flush incurs two write operations.
        result = nn.fs.File.Write(handle, 0, saveData, saveData.LongLength, nn.fs.WriteOption.Flush);

        // You do not need to handle this error if you are sure there will be enough space.
        if (nn.fs.FileSystem.ResultUsableSpaceNotEnough.Includes(result))
        {
            Debug.LogErrorFormat("Insufficient space to write {0} bytes to {1}", saveData.LongLength, filePath);
        }

        // The file must be closed before committing.
        nn.fs.File.Close(handle);

        // Verify that the write operation was successful before committing.
        if (!result.IsSuccess())
        {
            Debug.LogErrorFormat("Failed to write {0}: {1}", filePath, result.ToString());
            return false;
        }

        // This method moves the data from the journaling area to the main storage area.
        // If you do not call this method, all changes will be lost when the application closes.
        // Only call this when you are sure that all previous operations succeeded.
        nn.fs.FileSystem.Commit(_mountName);

        return true;
    }

    #endregion

    #endregion

    // Returns true if enough time has passed since the last save to meet the minimum frequency requirements
    private static bool FrequencyIsSafe()
    {
        if (Time.time > lastSaveTime + minimumDelayBetweenSaves)
            return true;
        else
        {
            Debug.Log("Not enough time passed since last save: " + (Time.time - lastSaveTime));
            return false;
        }
    }

    private void OnApplicationQuit()
    {
        //ES3SaveDemo.SaveOnQuit();
    }
}
#endif