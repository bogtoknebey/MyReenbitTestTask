using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;

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
                if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
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

        // Function to validate email (you can use a more comprehensive email validation)
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Function to send an email notification
        private void SendEmailNotification(string email, string filePath)
        {
            using (var client = new SmtpClient(_configuration["Smtp:Host"]))
            {
                client.Port = int.Parse(_configuration["Smtp:Port"]);
                client.Credentials = new NetworkCredential(_configuration["Smtp:Username"], _configuration["Smtp:Password"]);
                client.EnableSsl = true;

                var message = new MailMessage
                {
                    From = new MailAddress(_configuration["Smtp:Username"]),
                    Subject = "File Upload Notification",
                    Body = "Your file has been successfully uploaded.",
                    IsBodyHtml = false
                };

                message.To.Add(email);

                // Attach the file to the email
                message.Attachments.Add(new Attachment(filePath));

                client.Send(message);
            }
        }
    }
}
