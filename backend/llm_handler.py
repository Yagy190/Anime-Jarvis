import os
from dotenv import load_dotenv
from langchain_google_genai import ChatGoogleGenerativeAI
from langchain_core.messages import HumanMessage, SystemMessage

load_dotenv()

class LLMHandler:
    def __init__(self):
        # We are using Gemini for lightning-fast responses!
        api_key = os.getenv("GOOGLE_API_KEY")
        if not api_key:
            raise ValueError("GOOGLE_API_KEY not found in .env file!")

        self.llm = ChatGoogleGenerativeAI(
            model="gemini-1.5-flash",
            google_api_key=api_key,
            temperature=0.7
        )
        print("LLM: Using Gemini API (gemini-1.5-flash)")
            
        self.system_prompt = SystemMessage(
            content="You are Jarvis, but you have the personality of a cute, energetic 20-year-old Japanese anime girl. "
                    "You speak English, but you occasionally use cute Japanese filler words like 'Eto...', 'Ano...', or end sentences with '!'. "
                    "Keep your responses relatively short as they will be spoken out loud."
        )
        self.history = [self.system_prompt]

    async def generate_response(self, user_input: str) -> str:
        self.history.append(HumanMessage(content=user_input))
        
        # Keep history short for performance (last 10 messages)
        if len(self.history) > 11:
            self.history = [self.system_prompt] + self.history[-10:]
            
        response = await self.llm.ainvoke(self.history)
        self.history.append(response)
        
        return response.content
