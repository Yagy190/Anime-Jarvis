import edge_tts
import io
import wave
import struct

class TTSHandler:
    def __init__(self):
        # English female voice - works well with English text that has occasional Japanese words
        # Alternative Japanese voice: "ja-JP-NanamiNeural" (use if responses are primarily Japanese)
        self.voice = "en-US-JennyNeural"
        self.sample_rate = 24000
        print(f"TTS: Using edge-tts with voice {self.voice}")

    async def synthesize(self, text: str) -> bytes:
        """
        Synthesizes speech from text using edge-tts.
        Returns the audio as WAV bytes (16-bit PCM) for Unity compatibility.
        """
        communicate = edge_tts.Communicate(text, self.voice)
        
        # edge-tts generates MP3 audio asynchronously
        mp3_data = b""
        async for chunk in communicate.stream():
            if chunk["type"] == "audio":
                mp3_data += chunk["data"]
        
        # Convert MP3 to WAV using pydub for Unity compatibility
        from pydub import AudioSegment
        audio = AudioSegment.from_mp3(io.BytesIO(mp3_data))
        
        # Convert to mono 16-bit PCM WAV at 24000 Hz
        audio = audio.set_frame_rate(self.sample_rate).set_channels(1).set_sample_width(2)
        
        wav_buffer = io.BytesIO()
        audio.export(wav_buffer, format="wav")
        return wav_buffer.getvalue()
