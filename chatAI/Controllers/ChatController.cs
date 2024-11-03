using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using chatAI.Models;
using Mscc.GenerativeAI;

namespace chatAI.Controllers
{
   

    [ApiController]
    [Route("api/chatbot")]
    public class ChatController : ControllerBase
    {
        private readonly BotService _botService;
        private readonly FileStorageService _fileStorageService;
        private string Userid;
       
       


        public ChatController(IWebHostEnvironment env)
        {
            _botService = new BotService();
            _fileStorageService = new FileStorageService(env);
            Userid = "user123";


        }

        // Handle text-only message
        [HttpPost("message")]
        public async Task<IActionResult> GetTextResponse([FromBody] UserMessage userMessage)
        {
            var botResponse = await _botService.GetBotReplyAsync(userMessage.Message, Userid);
            return Ok(new { reply = botResponse });
        }

        // Handle message with image
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage([FromForm]  IFormFile image, [FromForm] string message)
        {
            if (image == null || image.Length == 0)
                return BadRequest("No image provided.");

            var fileUrl = await _fileStorageService.UploadFileToStorage(image);
            var botResponse = await _botService.GetImageReplyAsync(message, fileUrl, Userid); // Call GetImageReplyAsync with image

            return Ok(new { reply = botResponse, imageUrl = fileUrl });
        }
    }

    public class UserMessage
    {
        public string UserId { get; set; }
        public string Message { get; set; }
    }
    public class FileStorageService
    {
        private readonly string _storagePath;

        public FileStorageService(IWebHostEnvironment env)
        {
            _storagePath = Path.Combine(env.WebRootPath, "uploads");
        }

        public async Task<string> UploadFileToStorage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file provided for upload.");
            }

            // Ensure the directory exists
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }

            // Generate a unique filename
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(_storagePath, fileName);

            // Save the file to the wwwroot directory
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the relative URL to access the file
            return filePath;
        }
    }
}
