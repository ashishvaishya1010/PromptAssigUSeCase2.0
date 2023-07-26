using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Use_Case_2.Controllers
{
    public class HomeController : Controller
    {
        //const string API_KEY = "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";
        private readonly ILogger<HomeController> _logger;
        static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(500) // Set a timeout of 300 seconds (5 minutes)
        };

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Get(string prompt1, string prompt2, string prompt3, string filename1, string filename2, string filename3)
        {
            var prompts = new string[] { prompt1, prompt2, prompt3 };
            var filenames = new string[] { filename1 , filename2 , filename3  };
            var resultFiles = new List<string>();
            var inputFiles = new List<string>();

            var options = new Dictionary<string, object>
            {
                { "model", "gpt-3.5-turbo" },
                { "max_tokens", 3500 },
                { "temperature", 0.2 }
            };

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", API_KEY);

            try
            {
                for (int i = 0; i < prompts.Length; i++)
                {
                    string input = prompts[i];

                    if (!string.IsNullOrEmpty(input))
                    {
                        options["messages"] = new[]
                        {
                            new
                            {
                                role = "user",
                                content = input
                            }
                        };

                        var json = JsonConvert.SerializeObject(options);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var startTime = DateTime.Now;
                        var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                        response.EnsureSuccessStatusCode();

                        var responseBody = await response.Content.ReadAsStringAsync();
                        dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);
                        string result = jsonResponse.choices[0].message.content;
                        var endTime = DateTime.Now;
                        var responseTime = (endTime - startTime).TotalMilliseconds;

                        // Save the data to a CSV file with the specified filename
                        string csvContent = $"Prompt: {input}\nGenerated Response: {result}\nResponse Time (ms): {responseTime}";
                        string fileName = $"{filenames[i]}.csv"; // Add .csv extension to the provided filename

                        string filePath = Path.Combine("C:\\Users\\prajkta.patil\\source\\repos\\OpenAI_USeCase2.0\\OpenAI_USeCase2.0\\Controllers", fileName);
                        await System.IO.File.WriteAllTextAsync(filePath, csvContent, Encoding.UTF8);
                        resultFiles.Add(fileName);

                        // Store the input files for the third prompt
                        if (i < 2)
                        {
                            inputFiles.Add(filePath);
                        }
                    }
                }

                ViewBag.FileNames = resultFiles;
                return View("Index");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        // Add a new action to download the CSV file for a specific prompt
        public IActionResult Download(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                string filePath = Path.Combine("C:\\Users\\ashish.vaishya\\Downloads\\OpenAI_UseCase2.0-master\\OpenAI_USeCase2.0\\Controllers", fileName);
                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "text/csv", fileName);
                }
            }
            return NotFound();
        }
    }
}
