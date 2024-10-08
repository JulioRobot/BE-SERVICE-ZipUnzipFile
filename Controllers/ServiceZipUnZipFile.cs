using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SJ_BE_SERVICE_ZipUnzipFile.Models;


namespace SJ_BE_SERVICE_ZipUnzipFile.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServiceZipUnZipFile: ControllerBase
    {
        private readonly ZipUnzipFile zipUnzipFile;

        public ServiceZipUnZipFile(ZipUnzipFile zipUnZip)
        {
            zipUnzipFile = zipUnZip;
        }

        [HttpPost("zipunzip")]
        [EnableCors("AllowAllPost")]
        public async Task<string> PostZipUnZipFile()
        {
            string token = "";
            var result = await zipUnzipFile.ProsesZipUnzipFile();
            if (result == null)
            {
                Response.StatusCode = 401;
            }
            return result;

        }

        [HttpGet("download-zip")]
        public async Task<IActionResult> DownloadZipAsync()
        {
            var result = await zipUnzipFile.DownloadZipAsync();

            if (result == null)
            {
                return NotFound("The requested zip file does not exist.");
            }

            return File(result.FileContens, result.ContentType, result.FileName);
        }
    }

    
}
