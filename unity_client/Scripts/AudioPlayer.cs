using UnityEngine;
using System.IO;

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
        // Convert the received bytes into an AudioClip.
        // NOTE: edge-tts sends MP3 by default. Unity's built-in AudioClip doesn't 
        // dynamically load MP3 from bytes at runtime easily without 3rd party libs.
        // A common solution is using NLayer (NuGet package) to decode MP3 to WAV bytes first,
        // or asking the backend to send WAV instead of MP3.
        
        // Assuming backend sends WAV bytes for this simple example:
        float[] floatArray = ConvertWavBytesToFloatArray(audioBytes);
        
        if (floatArray != null && floatArray.Length > 0)
        {
            // 24000 is typical edge-tts sample rate, 1 channel
            AudioClip clip = AudioClip.Create("TTSAudio", floatArray.Length, 1, 24000, false);
            clip.SetData(floatArray, 0);
            
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    private float[] ConvertWavBytesToFloatArray(byte[] wavBytes)
    {
        // Simple WAV header parsing (assumes 16-bit PCM WAV)
        if (wavBytes.Length < 44) return null;

        int audioDataLength = wavBytes.Length - 44;
        float[] floatArray = new float[audioDataLength / 2];

        for (int i = 0; i < floatArray.Length; i++)
        {
            // Convert 16-bit PCM bytes to float (-1.0 to 1.0)
            short bitValue = System.BitConverter.ToInt16(wavBytes, 44 + i * 2);
            floatArray[i] = bitValue / 32768.0f;
        }

        return floatArray;
    }
}
