using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayAudioBytes(byte[] audioBytes)
    {
        // Save the raw MP3 bytes to a temporary file
        string tempPath = Path.Combine(Application.persistentDataPath, "temp_reply.mp3");
        File.WriteAllBytes(tempPath, audioBytes);
        
        // Use Unity's built-in web request to natively decode the MP3 file!
        StartCoroutine(LoadAndPlayAudio("file://" + tempPath));
    }

    private IEnumerator LoadAndPlayAudio(string url)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Audio play error: " + www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }
}
