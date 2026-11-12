using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.IO;
using NativeWebSocket;
using UnityEngine.InputSystem;

public class JarvisClient : MonoBehaviour
{
    WebSocket websocket;
    public string serverUrl = "ws://127.0.0.1:8000/ws";
    
    public AudioPlayer audioPlayer;
    public AvatarAnimator avatarAnimator;

    [System.Serializable]
    public class AIResponse
    {
        public string text;
        public string emotion;
        public int audio_length;
    }

    private bool expectingAudio = false;
    private AudioClip recordingClip;
    private bool isRecording = false;
    private string textMessage = "";

    async void Start()
    {
        websocket = new WebSocket(serverUrl);

        websocket.OnOpen += () => Debug.Log("Connected to Jarvis AI Backend!");
        websocket.OnError += (e) => Debug.Log("Error! " + e);
        websocket.OnClose += (e) => Debug.Log("Connection closed!");

        websocket.OnMessage += (bytes) =>
        {
            if (!expectingAudio)
            {
                string json = Encoding.UTF8.GetString(bytes);
                AIResponse response = JsonUtility.FromJson<AIResponse>(json);
                
                // This logs the AI's response in the Unity Console
                Debug.Log("Jarvis: " + response.text);
                
                if (avatarAnimator != null)
                {
                    avatarAnimator.SetEmotion(response.emotion);
                }
                
                expectingAudio = true;
            }
            else
            {
                if (audioPlayer != null)
                {
                    audioPlayer.PlayAudioBytes(bytes);
                }
                expectingAudio = false;
            }
        };

        await websocket.Connect();
    }

    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
        #endif

        // Press and Hold Spacebar to talk (Using New Input System)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                StartRecording();
            }
            else if (Keyboard.current.spaceKey.wasReleasedThisFrame)
            {
                StopRecordingAndSend();
            }
        }
    }

    void StartRecording()
    {
        Debug.Log("🎤 Recording started... (Speak now!)");
        isRecording = true;
        // Stop any previous dangling recording just in case
        Microphone.End(null);
        // Start recording from default microphone, max 30 seconds
        recordingClip = Microphone.Start(null, false, 30, 16000);
    }

    void StopRecordingAndSend()
    {
        if (!isRecording) return;
        
        Debug.Log("⏹️ Recording stopped. Sending to AI Brain...");
        isRecording = false;
        
        int position = Microphone.GetPosition(null);
        Microphone.End(null);

        if (position > 0)
        {
            byte[] wavData = EncodeAsWAV(recordingClip, position);
            SendAudioToAI(wavData);
        }
    }

    public async void SendAudioToAI(byte[] audioBytes)
    {
        if (websocket.State == WebSocketState.Open)
        {
            await websocket.Send(audioBytes);
            Debug.Log("⏳ Audio sent! Waiting for Jarvis to think...");
        }
    }

    public async void SendTextToAI(string text)
    {
        if (websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(text);
            Debug.Log("💬 Text sent! Waiting for Jarvis to think...");
        }
    }

    void OnGUI()
    {
        // Simple UI on screen to type a message
        GUI.Box(new Rect(10, 10, 300, 60), "Chat with Jarvis");
        textMessage = GUI.TextField(new Rect(20, 35, 200, 20), textMessage);
        
        if (GUI.Button(new Rect(230, 35, 70, 20), "Send"))
        {
            if (!string.IsNullOrEmpty(textMessage))
            {
                SendTextToAI(textMessage);
                textMessage = ""; // Clear after sending
            }
        }
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }

    // Helper method to convert Unity AudioClip into a standard .WAV file format in memory
    private byte[] EncodeAsWAV(AudioClip clip, int lengthSamples)
    {
        float[] samples = new float[lengthSamples * clip.channels];
        clip.GetData(samples, 0);

        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(memoryStream))
            {
                writer.Write(Encoding.UTF8.GetBytes("RIFF"));
                writer.Write(36 + samples.Length * 2);
                writer.Write(Encoding.UTF8.GetBytes("WAVE"));
                writer.Write(Encoding.UTF8.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)clip.channels);
                writer.Write(clip.frequency);
                writer.Write(clip.frequency * clip.channels * 2);
                writer.Write((short)(clip.channels * 2));
                writer.Write((short)16);
                writer.Write(Encoding.UTF8.GetBytes("data"));
                writer.Write(samples.Length * 2);

                int sampleInt;
                for (int i = 0; i < samples.Length; i++)
                {
                    sampleInt = (int)(samples[i] * 32767.0f);
                    if (sampleInt > 32767) sampleInt = 32767;
                    else if (sampleInt < -32768) sampleInt = -32768;
                    writer.Write((short)sampleInt);
                }
            }
            return memoryStream.ToArray();
        }
    }
}
