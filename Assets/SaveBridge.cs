using System;
using System.Collections;
using System.Collections.Generic;
//using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public static class SaveBridge
{
    static bool initialised = false;
    public static ES3Settings settings;
    public static ES3File cache;

    public static int someInt;
    public static float someFloat;
    public static bool someBool;

    #region Start

    
    // Call this once when your game begins to initialise your ES3 script.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Initialise()
    {
        // Don't initialise more than once.
        if (initialised)
        {
            Debug.LogError("Tried to initialise SaveManager when it was already initialised.");
            return;
        }
        Debug.LogError("SaveBridge Initialized");

        // Initialise an ES3Settings, which holds all the settings for reading/writing.
        // IMPORTANT: Always pass this in as an argument whenever reading/writing.
        settings = new ES3Settings();

#if UNITY_SWITCH && !UNITY_EDITOR
        // These must be set this way because the settings below ("Location.File" and "Directory.PersistentDataPath") are not supported on Switch.
        settings.location = ES3.Location.Cache;
        settings.directory = ES3.Directory.DataPath;
        settings.encryptionType = ES3.EncryptionType.None;
#else
        // You probably want to use these settings for Windows/Mac/Linux builds. Not sure about other platforms.
        settings.location = ES3.Location.File;
        settings.directory = ES3.Directory.PersistentDataPath;
#endif

        // Remember that we initialised this class so we don't do it again.
        initialised = true;

        // Load all saved data into relevant classes.
        LoadAllData();
        // ExperienceManager.Initialize();
    }

    #endregion
    
    #region Load

    public static void LoadAllData()
    {
        if (!initialised)
            Initialise();

        // Create a new ES3File (for caching save data in memory).
        // Use the ES3Settings we definied on initialise.
        cache = new ES3File(settings);

        // NOTE: Switch functionality is not supported in Editor (it crashes).
#if UNITY_SWITCH && !UNITY_EDITOR
        
        // Load saved byte array into a new byte array.
        bool successful = false;
        byte[] fileAsByteArray = NXSave.Load(ref successful);

        // If the load failed, return out and don't store it in the cache.
        if (!successful)
        { 
            Debug.LogError("Load failed.");
            return;
        }

        // Put the byte array into the ES3File cache.
        cache = new ES3File(fileAsByteArray, settings);
        
#else
        // If we're not running on Switch, reading from disk is much more simple.
        cache.Sync();
        Debug.Log("Save data synced with cache.");
#endif

        // Retrieve all your values from the save cache.
        // NOTE: You could then push these variables out to whatever classes need them, e.g. a high score integer goes into some HighScore class or something.
        // For this sample I just leave them unused here.

        // someInt = cache.Load<int>("someIntID", 0);
        // someFloat = cache.Load<float>("someFloatID", 0f);
        // someBool = cache.Load<bool>("someBoolID", false);

        Debug.Log("Data loaded from cache.");
        
    }

    #endregion
    
    #region Save

    // Push all your values from memory to the save cache.
    // "ignoreFrequencyRestrictions" defines whether we want to adhere to the lotcheck guideline 0011 requirement which prevents you writing too frequently.
    // It defaults to false for safety but you can override it in special cases (e.g. saving when your app is closed by the user)
    public static void SaveAllData(bool ignoreFrequencyRestrictions = false)
    {
        if (!initialised)
            Initialise();

        // NOTE: Instead of the sample variables here you should be pulling all your data from wherever it lies (e.g. a high score from a HighScore class or something).
        // cache.Save<int>("someIntID", someInt);
        // cache.Save<float>("someFloatID", someFloat);
        // cache.Save<bool>("someBoolID", someBool);

        Debug.LogError("!!! Data saved to Cache. !!!");

        // The save data has now been stored in the ES3File cache, which is only in memory.
        // You can now write that cache to disk.

        // OR, you may want to make the following code an optional step so you can have some quick 'auto-saves' and a slower 'save to disk' at a convenient time, for example.

        // Here's how you would write to disk...

        // NOTE: Switch save functionality is not supported in Editor (it crashes).
#if UNITY_SWITCH && !UNITY_EDITOR
        
        // Convert the cached 'ES3File' to byte array.
        byte[] data = cache.LoadRawBytes();

        // Pass the byte array to NXSave class to write to disk.
        // Also pass in whether we want to ignore the frequency restrictions (you should only do this if absolutely necessary, e.g. when shutting down the app)
        bool saved = NXSave.Save(data, ignoreFrequencyRestrictions);

        Debug.Log("Write cache to disk success = " + saved);
#else
        // If we're not running on Switch, saving to disk is much more simple.
        
        cache.Sync();
        
        Debug.Log("Save data synced with cache.");
#endif
    }


    public static void DeleteAllData()
    {
        cache.Clear();
        SaveAllData(true);
    }



    // You can call this when the application is closed.
    // It makes sure everything is saved and forces it to ignore the frequency restriction.
    // You should also make sure the Switch firmware waits for the save to complete before closing.
    public static void SaveOnQuit()
    {
        Debug.Log("Application quit. Forcing a save...");
        SaveAllData(true);
    }

    #endregion
    
    
    
    #region PlayerPrefs Helper

    // SaveBridge.HasKeyPP
    public static bool HasKeyPP(string key) 
    {
#if UNITY_SWITCH
        return cache.KeyExists(key);
#else
       return PlayerPrefs.HasKey(key);
#endif
    }
    
    // SaveBridge.SetIntPP
    public static void SetIntPP(string key, int value)
    {
#if UNITY_SWITCH
        cache.Save(key,value);
#else
        PlayerPrefs.SetInt(key,value);
#endif
    }

    // SaveBridge.GetIntPP
    public static int GetIntPP(string key, int defaultValue = 0)
    {
#if UNITY_SWITCH
        return cache.Load(key,defaultValue);
#else
        return PlayerPrefs.GetInt(key,defaultValue);
#endif
    }
    
    // SaveBridge.SetFloatPP
    public static void SetFloatPP(string key, float value)
    {
#if UNITY_SWITCH
        cache.Save(key,value);
#else
        PlayerPrefs.SetFloat(key,value);
#endif
    }

    // SaveBridge.GetFloatPP
    public static float GetFloatPP(string key, float defaultValue)
    {
#if UNITY_SWITCH
        return cache.Load(key,defaultValue);
#else
        return PlayerPrefs.GetFloat(key,defaultValue);
#endif
    }
    
    // SaveBridge.SetStringPP
    public static void SetStringPP(string key, string value)
    {
#if UNITY_SWITCH
        cache.Save(key,value);
#else
        PlayerPrefs.SetString(key,value);
#endif
    }

    // SaveBridge.GetStringPP
    public static string GetStringPP(string key, string defaultValue = "")
    {
#if UNITY_SWITCH
        return cache.Load(key,defaultValue);
#else
        return PlayerPrefs.GetString(key,defaultValue);
#endif
    }
    
    #endregion
    
    
    /*
    #region ObscuredPrefs Helper
    
    // SaveBridge.HasKeyOP
    public static bool HasKeyOP(string key)
    {
#if UNITY_SWITCH
        return cache.KeyExists(key);
#else
        return ObscuredPrefs.HasKey(key);
#endif
    }
    
    // SaveBridge.SetIntOP
    public static void SetIntOP(string key, int value)
    {
#if UNITY_SWITCH
        cache.Save(key,value);
#else
        ObscuredPrefs.SetInt(key,value);
#endif
    }
    
    // SaveBridge.GetIntOP
    public static int GetIntOP(string key, int defaultValue = 0)
    {
#if UNITY_SWITCH
        return cache.Load(key,defaultValue);
#else
        return ObscuredPrefs.GetInt(key,defaultValue);
#endif
    }
    
    // SaveBridge.SetFloatOP
    public static void SetFloatOP(string key, float value)
    {
#if UNITY_SWITCH
        cache.Save(key,value);
#else
        ObscuredPrefs.SetFloat(key,value);
#endif
    }

    // SaveBridge.GetFloatOP
    public static float GetFloatOP(string key, float defaultValue = 0)
    {
#if UNITY_SWITCH
        return cache.Load(key,defaultValue);
#else
        return ObscuredPrefs.GetFloat(key,defaultValue);
#endif
    }
    
    // SaveBridge.SetStringOP
    public static void SetStringOP(string key, string value)
    {
#if UNITY_SWITCH
        cache.Save(key,value);
#else
        ObscuredPrefs.SetString(key,value);
#endif
    }

    // SaveBridge.GetStringOP
    public static string GetStringOP(string key, string defaultValue)
    {
#if UNITY_SWITCH
        return cache.Load(key,defaultValue);
#else
        return ObscuredPrefs.GetString(key,defaultValue);
#endif
    }
    
    // SaveBridge.SetBoolOP
    public static void SetBoolOP(string key, bool value)
    {
#if UNITY_SWITCH
        cache.Save(key,value);
#else
        ObscuredPrefs.SetBool(key,value);
#endif
    }

    // SaveBridge.GetBoolOP
    public static bool GetBoolOP(string key, bool defaultValue = false)
    {
#if UNITY_SWITCH
        return cache.Load(key,defaultValue);
#else
        return ObscuredPrefs.GetBool(key,defaultValue);
#endif
    }
    
    // SaveBridge.SetVector3OP
    public static void SetVector3OP(string key, Vector3 value)
    {
#if UNITY_SWITCH
            cache.Save(key,value);
#else
        ObscuredPrefs.SetVector3(key,value);
#endif
    }
    
    // SaveBridge.GetVector3OP
    public static Vector3 GetVector3OP(string key, Vector3 defaultValue = new Vector3())
    {
#if UNITY_SWITCH
        return cache.Load(key,defaultValue);
#else
        return ObscuredPrefs.GetVector3(key,defaultValue);
#endif
    }
    
    // SaveBridge.SetQuaternionOP
    public static void SetQuaternionOP(string key, Quaternion value)
    {
#if UNITY_SWITCH
            cache.Save(key,value);
#else
        ObscuredPrefs.SetQuaternion(key,value);
#endif
    }

    // SaveBridge.GetQuaternionOP
    public static Quaternion GetQuaternionOP(string key, Quaternion defaultValue = new Quaternion())
    {
#if UNITY_SWITCH
            return cache.Load(key,defaultValue);
#else
        return ObscuredPrefs.GetQuaternion(key,defaultValue);
#endif
    }
    
    // SaveBridge.SetColorOP
    public static void SetColorOP(string key, Color value)
    {
#if UNITY_SWITCH
            cache.Save(key,value);
#else
        ObscuredPrefs.SetColor(key,value);
#endif
    }
    
    // SaveBridge.GetColorOP
    public static Color GetColorOP(string key, Color defaultValue = new Color())
    {
#if UNITY_SWITCH 
            return cache.Load(key,defaultValue);
#else
        return ObscuredPrefs.GetColor(key,defaultValue);
#endif
    }
    #endregion
    */
    
    
    #region EasySave3 Helper

    // SaveBridge.ES3KeyExists
    public static bool ES3KeyExists(string key)
    {
#if UNITY_SWITCH && !UNITY_EDITOR
            return cache.KeyExists(key);
#else
        return ES3.KeyExists(key);
#endif
    }
    
    // SaveBridge.ES3Save
    public static void ES3Save<T>(string key, T value)
    {

#if UNITY_SWITCH && !UNITY_EDITOR
        cache.Save(key,value);
#else
        ES3.Save(key,value);
#endif
    }
    
    // SaveBridge.ES3Load
    public static T ES3Load<T>(string key, T value)
    {
#if UNITY_SWITCH && !UNITY_EDITOR
        return cache.Load(key,value);
#else
        return  ES3.Load(key,value);
#endif
    }
    
    public static T ES3Load<T>(string key)
    {
#if UNITY_SWITCH && !UNITY_EDITOR
            return cache.Load<T>(key);
#else
        return ES3.Load<T>(key);
#endif
    }

    #endregion
    
}