import asyncio
import json
import logging
from dotenv import load_dotenv
from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from llm_handler import LLMHandler
from stt_handler import STTHandler
from tts_handler import TTSHandler

load_dotenv()

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI()

llm = LLMHandler()
stt = STTHandler()
tts = TTSHandler()

@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    logger.info("Client connected")
    
    try:
        while True:
            # Receive message from client (Unity)
            message = await websocket.receive()
            
            user_text = ""
            
            if "text" in message:
                # User sent a text message
                user_text = message["text"]
                logger.info(f"User typed: {user_text}")
                
            elif "bytes" in message:
                # User sent voice audio
                data = message["bytes"]
                logger.info("Received audio, transcribing...")
                user_text = await stt.transcribe(data)
                logger.info(f"User said: {user_text}")
            
            if not user_text.strip():
                continue
                
            # 2. LLM response
            logger.info("Generating AI response...")
            ai_text = await llm.generate_response(user_text)
            logger.info(f"AI: {ai_text}")
            
            # Generate a simple emotion based on text (very basic for now)
            emotion = "neutral"
            if "!" in ai_text:
                emotion = "happy"
            elif "?" in ai_text:
                emotion = "thinking"
                
            # 3. Text to Speech
            logger.info("Synthesizing speech...")
            audio_bytes = await tts.synthesize(ai_text)
            
            # 4. Send back to Unity
            # We send JSON with metadata, then the audio bytes in a separate frame, or base64 encoded.
            # For efficiency in binary WebSockets, we'll send a JSON header followed by binary audio.
            response_data = {
                "text": ai_text,
                "emotion": emotion,
                "audio_length": len(audio_bytes)
            }
            
            await websocket.send_text(json.dumps(response_data))
            await websocket.send_bytes(audio_bytes)
            logger.info("Response sent to client")
            
    except WebSocketDisconnect:
        logger.info("Client disconnected")
    except Exception as e:
        logger.error(f"Error: {e}")

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=8000)
