﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.IO.Compression;
using DotVisualizerLib;
using System.Diagnostics;
using System.Net.Http;

namespace VisualizerWebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Azure Functions Binding Visualizer";

            return View();
        }

        [HttpPost("UploadFiles")]
        public async Task<IActionResult> UploadFile(IFormFile formFile)
        {
            var filePath = Path.GetTempFileName();

            try {
                if (formFile.Length > 0) {
                    using (var stream = new FileStream(filePath, FileMode.Create)) {
                        await formFile.CopyToAsync(stream);
                    }
                }

                string fileContent = ZipDirToRenderedFile(filePath);
            }
            catch (Exception e) {
                return ShowError($"Error processing zipfile: {e.Message}");
            }

            return View("ViewImage");
        }

        private IActionResult ShowError(string message)
        {
            ViewData["Errors"] = message;
            return View("Index");
        }

        private string ZipDirToRenderedFile(string filePath)
        {
            string tempDir = GetTempFilePath();

            ZipFile.ExtractToDirectory(filePath, tempDir);
            string dotFile = CreateDotFile(tempDir);
            string outputFile = CreateOutputFile(dotFile, "svg");

            string fileContent = System.IO.File.ReadAllText(outputFile);

            ViewData["SvgFile"] = outputFile;
            ViewData["ImageContent"] = fileContent;
            ViewData["DotFile"] = dotFile;

            return fileContent;
        }

        [HttpPost("GitHubContent")]
        public async Task<IActionResult> DownloadGitHubRepo(string repoUrl)
        {
            try {
                var uri = new Uri(repoUrl);
            }
            catch (UriFormatException) {
                ViewData["RepoUrlError"] = "has-error";
                ViewData["RepoUrlErrorLabel"] = " - Invalid URL";
                return View("Index");
            }

            try {
                string archiveUrl = $"{repoUrl}/archive/master.zip";

                var client = new HttpClient();
                var content = await client.GetAsync(archiveUrl);
                var zipfile = GetTempFilePath();

                using (var fileStream = new FileStream(zipfile, FileMode.Create, FileAccess.Write, FileShare.None)) {
                    await content.Content.CopyToAsync(fileStream);
                }

                string fileContent = ZipDirToRenderedFile(zipfile);
            }
            catch (Exception e) {
                return ShowError($"Error processing repo: {e.Message}");
            }

            return View("ViewImage");
        }

        public IActionResult DownloadDotFile(string file)
        {
            var bytes = System.IO.File.ReadAllBytes(file);
            return File(bytes, "application/octet-stream", "function-bindings.dot");
        }

        public IActionResult DownloadSvg(string file)
        {
            var bytes = System.IO.File.ReadAllBytes(file);
            return File(bytes, "image/svg+xml", "function-bindings.svg");
        }

        private string CreateOutputFile(string filename, string format)
        {
            string outputFile = Path.Combine(Path.GetTempPath(), $"{filename}.{format}");
            var process = Process.Start("dot", $"-T{format} {filename} -o {outputFile}");
            process.WaitForExit();
            return outputFile;
        }

        private string CreateDotFile(string directory)
        {
            var dotFile = GetTempFilePath();
            Visualizer.DotFileFromFunctionDirectory(directory, dotFile);
            return dotFile;
        }

        private static string GetTempFilePath()
        {
            return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }
    }
}
