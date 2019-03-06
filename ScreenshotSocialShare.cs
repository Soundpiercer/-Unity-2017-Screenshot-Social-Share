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
    private bool isProcessing = false;
    private bool isFocus = false;

    /// <summary>
    /// Call this method from other classes.
    /// </summary>
    public void Share()
    {
        if (!isProcessing && !Application.isEditor)
        {
            StartCoroutine(ShareScreenshot("Share your world!", "Hello, World!"));
        }
    }

    /// <summary>
    /// The Process should be Asynchronous so we handle this method by IEnumerator.
    /// </summary>
    private IEnumerator ShareScreenshot(string ShareEventName, string ShareMessageText)
    {
        isProcessing = true;

        // wait for graphics to render
        yield return new WaitForEndOfFrame();

        // Screen Capture and Wait
        ScreenCapture.CaptureScreenshot("screenshot.png", 2); // Unity 2017 or upper
        // Application.CaptureScreenshot("screenshot.png", 2); // Unity 5
        string destination = Path.Combine(Application.persistentDataPath, "screenshot.png");

        yield return new WaitForSecondsRealtime(0.3f);

        // The Sharing Process varies by each Android Device's OS Level. 
        var apiInfo = new AndroidJavaClass("android.os.Build$VERSION");
        var apiLevel = apiInfo.GetStatic<int>("SDK_INT");
		
        if (apiLevel > 25) // Android 7.1 Nougat, 8 Oreo, 9 Pie
        {
            yield return StartCoroutine(AndroidOreoShareEnumerator(filePath, string.Empty, string.Empty));
        }
        else // ~ Android 7.0 Nougat
        {
			yield return StartCoroutine(AndroidNougatShareEnumerator(filePath, string.Empty, string.Empty));            
        }

        // won't proceed until the app restores its focus
        yield return new WaitUntil(() => isFocus);

        // End
        isProcessing = false;
        yield break;
    }
	
	/// <summary>
	/// Sharing Process (~ API Level 24)
    /// </summary>
	/// <param name="_path">Image path.</param>
	/// <param name="_popupMessage">Message that comes with 'Choose the App to Share' Popup </param>
	/// <param name="_shareMessage">Message that prints when the contents are shared to the app</param>
	private IEnumerator AndroidNougatShareEnumerator(string _path, string _popupMessage, string _shareMessage)
    {
        // Set Events (Unity)
        AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

        // Set Events (Android)
        AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
        AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");

        intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));

        // Set Text Message
        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), _shareMessage);

        // Set Image
        AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
        AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "file://" + _path);

        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
        intentObject.Call<AndroidJavaObject>("setType", "image/png");

        // START!
        AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, _popupMessage);
        currentActivity.Call("startActivity", chooser);

        yield return new WaitForSecondsRealtime(1.0f);
    }

	/// <summary>
	/// Sharing Process (API Level 25 ~)
    /// </summary>
	/// <param name="_path">Image path.</param>
	/// <param name="_popupMessage">Message that comes with 'Choose the App to Share' Popup </param>
	/// <param name="_shareMessage">Message that prints when the contents are shared to the app</param>
    private IEnumerator AndroidOreoShareEnumerator(string _path, string _popupMessage, string _shareMessage)
    {
        // Set Events (Unity)
        AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

        // Set Events (Android)
        AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
        AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");

        intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));

        // Set Text Message
        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), _shareMessage);

        // Set Image
        AndroidJavaClass uriClass = new AndroidJavaClass("android.support.v4.content.FileProvider");
        AndroidJavaClass fileClass = new AndroidJavaClass("java.io.File");

        AndroidJavaObject fileObject = new AndroidJavaObject("java.io.File", _path);
        AndroidJavaObject stringObject = new AndroidJavaObject("java.lang.String", "com.Colortronics.SliteDev.share.fileprovider");

        AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("getUriForFile", currentActivity, stringObject, fileObject);

        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
        intentObject.Call<AndroidJavaObject>("setType", "image/png");

        // START!
        AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, _popupMessage);
        currentActivity.Call("startActivity", chooser);

        yield return new WaitForSecondsRealtime(1.0f);
    }

    private void OnApplicationFocus(bool _focus)
    {
        isFocus = _focus;
    }
}
