using UnityEngine;
public class NXScript : MonoBehaviour
{
    private static NXScript _instance;
    public static NXScript Intance => _instance;
    private static nn.account.Uid userId;
#pragma warning disable 0414
    private nn.fs.FileHandle fileHandle = new nn.fs.FileHandle();
#pragma warning restore 0414
    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
    }
    public static void Initialize()
    {
        Debug.LogError("!!! Start Initialize NXScript !!!");
        nn.account.Account.Initialize();
        nn.account.UserHandle userHandle = new nn.account.UserHandle();
        if (!nn.account.Account.TryOpenPreselectedUser(ref userHandle))
        {
            nn.Nn.Abort("Failed to open preselected user.");
        }
        nn.Result result = nn.account.Account.GetUserId(ref userId, userHandle);
        result.abortUnlessSuccess();
        //result = nn.fs.SaveData.Mount(mountName, userId);
        //result.abortUnlessSuccess();
        //nn.hid.Npad.Initialize();
        //nn.hid.Npad.SetSupportedStyleSet(nn.hid.NpadStyle.Handheld | nn.hid.NpadStyle.JoyDual);
        //nn.hid.Npad.SetSupportedIdType(npadIds);
        //npadState = new nn.hid.NpadState();
    }
    public static nn.account.Uid GetUserID()
    {
        Debug.LogError($"!!! USER ID : {userId} !!!");
        return userId;
    }
}