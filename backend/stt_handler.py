import whisper
import io
import wave
import numpy as np
import asyncio

class STTHandler:
    def __init__(self):
        print("Loading Whisper model (base)...")
        self.model = whisper.load_model("base")
        print("Whisper model loaded.")

    async def transcribe(self, audio_bytes: bytes) -> str:
        loop = asyncio.get_event_loop()
        return await loop.run_in_executor(None, self._transcribe_sync, audio_bytes)

    def _transcribe_sync(self, audio_bytes: bytes) -> str:
        # Parse WAV bytes purely in python to avoid needing ffmpeg installed on Windows
        with wave.open(io.BytesIO(audio_bytes), 'rb') as wf:
            n_frames = wf.getnframes()
            audio_data = wf.readframes(n_frames)
            
            # Unity sends 16-bit PCM WAV, so dtype=np.int16
            audio_np = np.frombuffer(audio_data, dtype=np.int16)
            
            # Whisper expects float32 between -1 and 1
            audio_float32 = audio_np.astype(np.float32) / 32768.0

        # Pass the numpy array directly to whisper
        result = self.model.transcribe(audio_float32, fp16=False)
        return result["text"].strip()
