using System;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SJ_BE_SERVICE_ZipUnzipFile.Models
{
    public class ZipUnzipFile
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger _logger;

        private const string ScormFileName = "scorm/Learning-Journey-Scorm-V3.0.4.zip";
        private string Output_Directory = @"D:\ZipOutput";

        public ZipUnzipFile(IWebHostEnvironment env, ILogger<ZipUnzipFile> logger)
        { 
            _environment = env;
            _logger = logger;
        }

        private string GetOutPutDirectory()
        {
            if (OperatingSystem.IsWindows())
            {
                return @"D:\ZipOutput";
            }
            else if (OperatingSystem.IsLinux())
            {
                return "/mnt/ZipOutput";
            }
            else
            {
                return Path.Combine(Directory.GetCurrentDirectory(), "ZipOutput");
            }
        }

        public async Task<string> ProsesZipUnzipFile()
        {
            Output_Directory = GetOutPutDirectory();

            _logger.LogInformation("ProsesZipUnzipFile - PATH DIRECTORY {value} ", Output_Directory);

            string rawPath = Path.Combine(_environment.WebRootPath, ScormFileName);
            // string unZipPath = Path.Combine(_environment.ContentRootPath, $"tmp/standalone/result/");
            string unZipPath = Path.Combine(Output_Directory, $"standalone/result/");
            string finalZip = Path.Combine(Output_Directory, "result.zip");
            Directory.CreateDirectory(Output_Directory);
            DirectoryInfo di_unzipPath = await CopyAndUnzipToPrepareDirectory(unZipPath, rawPath);         
            try
            {
                using (var fs = File.CreateText(Path.Combine(di_unzipPath.FullName, "data-details.json")))
                {
                    var dataScorm = new ScormData();
                    dataScorm.Name = "Test data scorm Name";
                    dataScorm.Id = "1";

                    var scormString = JsonSerializer.Serialize(dataScorm);
                    await fs.WriteAsync(scormString);
                    _logger.LogInformation("ProsesZipUnzipFile - Success Data details.json");
                }

                using (var fs = File.CreateText(Path.Combine(di_unzipPath.FullName, "share-details.json")))
                { 
                    var dataShare = new ShareData();
                    dataShare.Name = "Test data share data";
                    dataShare.ShareId = "1";

                    var dataString = JsonSerializer.Serialize(dataShare);
                    await fs.WriteAsync(dataString);
                    _logger.LogInformation("ProsesZipUnzipFile - Share details.json");
                }
               
                // zip the files
                FileInfo file_finalZip = new FileInfo(finalZip);
                if (file_finalZip.Exists)
                { 
                    file_finalZip.Delete();
                    _logger.LogInformation("ProsesZipUnzipFile - File = {value}", file_finalZip.FullName);
                }

                using (var file_zip = file_finalZip.Create())
                {
                    await Task.Run(() => ZipFile.CreateFromDirectory(di_unzipPath.FullName, file_zip));
                }

                _logger.LogInformation("ProsesZipUnzipFile - success create zip ");

                return "succes";

            }
            catch (Exception ex)
            {
                string error = "Error Create File";
                _logger.LogError("ProsesZipUnzipFile - Error Create File - Detail = {value}", ex.ToString());
                return ex.ToString();
                throw;
            }
        }

       

        private async Task<DirectoryInfo> CopyAndUnzipToPrepareDirectory(string unzipPath, string raw)
        { 
            FileInfo rawFile = new FileInfo(raw);

            var di_unzipPath = new DirectoryInfo(unzipPath);
            if (di_unzipPath.Exists) 
            {
                Console.WriteLine("Delete");
                _logger.LogInformation("CopyAndUnzipToPrepareDirectory - DELETE = {value} ", di_unzipPath.FullName);
                di_unzipPath.Delete(true);
            }
            di_unzipPath.Create();

            string path_raw = Path.Combine(unzipPath, $"rawFile.zip");
            var fi_rawCopy = rawFile.CopyTo(path_raw);

            using (var fs = new FileStream(fi_rawCopy.FullName, FileMode.Open))
            {
                using (ZipArchive seedRaw = new ZipArchive(fs, ZipArchiveMode.Read))
                {
                    seedRaw.ExtractToDirectory(di_unzipPath.FullName, true);
                }
            }

            _logger.LogInformation("CopyAndUnzipToPrepareDirectory - success unzip {value} ", di_unzipPath);

            fi_rawCopy.Delete();
            return di_unzipPath;
        }

        public async Task<ZipFileResult> DownloadZipAsync()
        {
            string finalZip = Path.Combine(Output_Directory, "result.zip");

            try
            {
                if (!File.Exists(finalZip)) 
                {
                    _logger.LogInformation("DownloadZipAsync- File Dont Exist = {value} ", finalZip);
                    return null;
                }

                byte[] fileBytes;
                using (FileStream fs = new FileStream(finalZip, FileMode.Open, FileAccess.Read,FileShare.Read, 4096, true))
                {
                    fileBytes = new byte[fs.Length];
                    await fs.ReadAsync(fileBytes, 0, (int)fs.Length);
                }

                return new ZipFileResult
                {
                    FileContens = fileBytes,
                    FileName = "result.zip",
                    ContentType = "application/zip"
                };

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading zip: {ex.Message}");
                _logger.LogError("DownloadZipAsync - Error downloading zip = {value}", ex.Message);
                return null;
            }
        }
    }

    public class ZipFileResult
    { 
        public byte[] FileContens { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }

    public class ScormData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class ShareData
    {
        public string ShareId { get; set; } = "";
        public string Name { get; set; } = "";
    }
}
