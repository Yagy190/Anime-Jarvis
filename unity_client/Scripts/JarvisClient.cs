using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using NativeWebSocket; // Requires downloading NativeWebSocket package for Unity

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

    async void Start()
    {
        websocket = new WebSocket(serverUrl);

        websocket.OnOpen += () =>
        {
            Debug.Log("Connected to Jarvis AI Backend!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            // We alternate between receiving JSON metadata and raw audio bytes
            if (!expectingAudio)
            {
                string json = Encoding.UTF8.GetString(bytes);
                AIResponse response = JsonUtility.FromJson<AIResponse>(json);
                Debug.Log("AI says: " + response.text);
                
                // Trigger facial expression
                if (avatarAnimator != null)
                {
                    avatarAnimator.SetEmotion(response.emotion);
                }
                
                expectingAudio = true;
            }
            else
            {
                // We received the raw audio bytes (mp3/wav)
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
    }

    public async void SendAudioToAI(byte[] audioBytes)
    {
        if (websocket.State == WebSocketState.Open)
        {
            await websocket.Send(audioBytes);
        }
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }
}
