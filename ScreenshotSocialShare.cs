using UnityEngine;
using System.IO;
using System.Collections;

// ----------------------
// Author : Soundpiercer
// soundpiercer@gmail.com
// ----------------------

/// <summary>
/// A Class that captures the screen and invoke Social Share Event in Android Environment.
/// It won't work on Unity Editor. 
/// </summary>
public class ScreenshotSocialShare : MonoBehaviour
{
    private bool _isProcessing = false;
    private bool _isFocus = false;

    /// <summary>
    /// Call this method from other classes.
    /// Screenshot Only : Share();
    /// Screenshot with messages : Share("Example Popup Message", "Example Share Message");
    /// </summary>
    /// <param name="popupMessage">Message that comes with 'Choose the App to Share' Popup </param>
    /// <param name="shareMessage">Message that prints when the contents are shared to the app</param>
    public void Share(string popupMessage = "", string shareMessage = "")
    {
        if (!_isProcessing && !Application.isEditor)
        {
            StartCoroutine(ShareScreenshot(popupMessage, shareMessage));
        }
    }

    /// <summary>
    /// The Process should be Asynchronous so we handle this method by IEnumerator.
    /// </summary>
    private IEnumerator ShareScreenshot(string popupMessage, string shareMessage)
    {
        _isProcessing = true;

        // wait for graphics to render
        yield return new WaitForEndOfFrame();

        // Screen Capture and Wait
        ScreenCapture.CaptureScreenshot("screenshot.png", 2); // Unity 2017 or upper
        // Application.CaptureScreenshot("screenshot.png", 2); // Unity 5
        string filePath = Path.Combine(Application.persistentDataPath, "screenshot.png");

        yield return new WaitForSecondsRealtime(0.3f);

        // The Sharing Process varies by each Android Device's OS Level. 
        var apiInfo = new AndroidJavaClass("android.os.Build$VERSION");
        var apiLevel = apiInfo.GetStatic<int>("SDK_INT");

        if (apiLevel > 25) // Android 7.1 Nougat ~ 
        {
            yield return StartCoroutine(AndroidOreoShareEnumerator(filePath, popupMessage, shareMessage));
        }
        else // ~ Android 7.0 Nougat
        {
            yield return StartCoroutine(AndroidNougatShareEnumerator(filePath, popupMessage, shareMessage));
        }

        // won't proceed until the app restores its focus
        yield return new WaitUntil(() => _isFocus);

        // End
        _isProcessing = false;
        yield break;
    }

    /// <summary>
    /// Sharing Process (~ API Level 24)
    /// </summary>
    /// <param name="path">Image path.</param>
    private IEnumerator AndroidNougatShareEnumerator(string path, string popupMessage, string shareMessage)
    {
        // Set Events (Unity)
        AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

        // Set Events (Android)
        AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
        AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");

        intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));

        // Set Text Message
        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), shareMessage);

        // Set Image
        AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
        AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "file://" + path);

        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
        intentObject.Call<AndroidJavaObject>("setType", "image/png");

        // START!
        AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, popupMessage);
        currentActivity.Call("startActivity", chooser);

        yield return new WaitForSecondsRealtime(1.0f);
    }

    /// <summary>
    /// Sharing Process (API Level 25 ~)
    /// </summary>
    /// <param name="path">Image path.</param>
    private IEnumerator AndroidOreoShareEnumerator(string path, string popupMessage, string shareMessage)
    {
        // Set Events (Unity)
        AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

        // Set Events (Android)
        AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
        AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");

        intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));

        // Set Text Message
        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), shareMessage);

        // Set Image
        AndroidJavaClass uriClass = new AndroidJavaClass("android.support.v4.content.FileProvider");
        AndroidJavaClass fileClass = new AndroidJavaClass("java.io.File");

        AndroidJavaObject fileObject = new AndroidJavaObject("java.io.File", path);
        AndroidJavaObject stringObject = new AndroidJavaObject("java.lang.String", "com.Colortronics.SliteDev.share.fileprovider");

        AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("getUriForFile", currentActivity, stringObject, fileObject);

        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
        intentObject.Call<AndroidJavaObject>("setType", "image/png");

        // START!
        AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, popupMessage);
        currentActivity.Call("startActivity", chooser);

        yield return new WaitForSecondsRealtime(1.0f);
    }

    private void OnApplicationFocus(bool focus)
    {
        _isFocus = focus;
    }
}
