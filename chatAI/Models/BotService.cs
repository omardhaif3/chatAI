using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Mscc.GenerativeAI;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace chatAI.Models
{
    public class BotService
    {
        static string _apiKey = "AIzaSyDIAJTCSNMRu2pFbYugGRn3wELXxBgj4c0";
        static IGenerativeAI genAi = new GoogleAI(_apiKey);
        static GenerativeModel model = genAi.GenerativeModel(Model.Gemini15ProLatest);

        // Dictionary to hold chat sessions for each user
        private static ConcurrentDictionary<string, ChatSession> userSessions = new ConcurrentDictionary<string, ChatSession>();

        public BotService()
        {
            // Initialization if needed
        }

        // Generates response for a text-only message
        public async Task<string> GetBotReplyAsync(string userMessage, string userId)
        {
            // Retrieve or create a new chat session for the user
            var chatSession = userSessions.GetOrAdd(userId, id => model.StartChat());

            var response = await chatSession.SendMessage(userMessage);
            return response.Text;
        }

        // Generates response for a message with an image
        public async Task<string> GetImageReplyAsync(string userMessage, string fileUrl, string userId)
        {
            var request = new GenerateContentRequest(userMessage);
            request.AddMedia(fileUrl); // Add image to the request

            // Retrieve or create a new chat session for the user
            var chatSession = userSessions.GetOrAdd(userId, id => model.StartChat());

            var response = await model.GenerateContent(request);
            return response.Text;
        }
    }
}
