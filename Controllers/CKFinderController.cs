using Microsoft.AspNetCore.Mvc;

namespace CRUDFIX.Controllers
{
    [Route("ckfinder/connector")]
    public class CKFinderController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public CKFinderController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile upload)
        {
            if (upload != null && upload.Length > 0)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "files");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var fileName = Path.GetFileName(upload.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(fileStream);
                }

                var url = Url.Content($"~/files/{fileName}");
                return Json(new { uploaded = true, url });
            }

            return Json(new { uploaded = false, error = new { message = "Upload failed" } });
        }
    }

}
