using UnityEngine;
using System.IO;
using System.Collections;

// ----------------------
// Author : Soundpiercer
// soundpiercer@gmail.com
// ----------------------
// tweaked from youtube tutorials

namespace Soundpiercer.Plugin
{
    /// <summary>
    /// A Class that captures the screen and invoke Social Share Event in Android Environment.
    /// </summary>
    public class ScreenshotSocialShare : MonoBehaviour
    {
        private bool isProcessing = false;
        private bool isFocus = false;

        /// <summary>
        /// Called from Other Classes.
        /// </summary>
        public void Share()
        {
            if (!isProcessing)
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

            // THE REAL SOCIAL SHARE PROCESS !!!
            if (!Application.isEditor)
            {
                AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
                AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
                intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));

                AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
                AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "file://" + destination);

                intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
                intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), ShareEventName);
                intentObject.Call<AndroidJavaObject>("setType", "image/jpeg");

                AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, ShareMessageText);

                currentActivity.Call("startActivity", chooser);

                yield return new WaitForSecondsRealtime(1.0f);
            }

            // won't proceeed until the app restores its focus
            yield return new WaitUntil(() => isFocus);

            // End
            isProcessing = false;
            yield break;
        }

        private void OnApplicationFocus(bool _focus)
        {
            isFocus = _focus;
        }
    }
}
