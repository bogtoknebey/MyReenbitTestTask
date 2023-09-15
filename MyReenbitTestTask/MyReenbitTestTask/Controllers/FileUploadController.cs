using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.IO;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;

namespace MyReenbitTestTask.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public FileUploadController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetMessage()
        {
            return Ok("This is a GET request message.");
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile()
        {
            //return Ok("File uploaded successfully");
            try
            {
                // Get the uploaded file
                var file = Request.Form.Files[0];

                // Validate the file (e.g., file type, size, etc.)
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Invalid file");
                }

                // Get the email address from the request
                var email = Request.Form["email"].ToString();

                // Validate the email (you can add more validation here)
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest("Invalid email");
                }

                // Save the uploaded file to a temporary location (you can customize this)
                var filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(file.FileName));
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Send an email notification
                SendEmailNotification(email, filePath);

                // Clean up the temporary file
                System.IO.File.Delete(filePath);

                // You can return a success response
                return Ok(new { message = "File uploaded successfully" });
            }
            catch (Exception ex)
            {
                // Handle exceptions and return an error response
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private void SendEmailNotification(string emailAddress, string filePath)
        {
            string host = _configuration["Smtp:Host"];
            int port = int.Parse(_configuration["Smtp:AlterPort"]);
            string username = _configuration["Smtp:Username"];
            string password = _configuration["Smtp:Password"];
            string subject = "Uploading file.";
            string message = "Your file has been successfully uploaded.";

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Sender Name", username));
            email.To.Add(new MailboxAddress("Receiver Name", emailAddress));

            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $"<b>{message}</b>"
            };

            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                var attachment = new MimePart("application", "octet-stream")
                {
                    Content = new MimeContent(stream),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = Path.GetFileName(filePath) // Use the file's name as the attachment name
                };


                // Add the attachment to the email
                email.Body = new Multipart("mixed")
                {
                    email.Body,
                    attachment
                };

                using (var smtp = new SmtpClient())
                {
                    smtp.Connect(host, port, false);
                    smtp.Authenticate(username, password);
                    smtp.Send(email);
                    smtp.Disconnect(true);
                }
            }
        }
    }
}
