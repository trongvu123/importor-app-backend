using importer_app.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using importer_app.requests;

namespace importer_app.Controllers
{
    [Route("api")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly List<Quiz> quizzes;
        public ImportController(List<Quiz> quizzes)
        {
            this.quizzes = quizzes;
        }

        [HttpPost]
        [Route("upload-quiz")]
        public async Task<ActionResult> UploadQuizFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Không có file được tải lên");

            try
            {
                // Lưu file tạm thời
                var filePath = Path.GetTempFileName();

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var quizzes = ImportQuizFromTextFile(filePath);
                this.quizzes.Clear(); 
                this.quizzes.AddRange(ImportQuizFromTextFile(filePath));
                System.IO.File.Delete(filePath);

                return Ok(new
                {
                    quizList = quizzes,
                    total = quizzes.Count,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi xử lý file: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("export-option")]
        public async Task<ActionResult> ExportOption([FromBody] OptionRequest optionRequest)
        {
            if(optionRequest == null)
            {
                return BadRequest(new
                {
                    message = "Quỳnh chưa chọn option đó!"
                });
            }
            int from = 0;
            int to = 0;
            bool isValidFrom = false;
            bool isValidTo = false;
            if ( !string.IsNullOrEmpty(optionRequest.from)  || !string.IsNullOrEmpty(optionRequest.to) )
            {

                isValidFrom = int.TryParse(optionRequest.from, out from);
                isValidTo = int.TryParse(optionRequest.to, out to);
            }
            if(optionRequest.all ==null || optionRequest.all == false)
            {
                if ((!isValidFrom && !string.IsNullOrEmpty(optionRequest.from)) || (!isValidTo && !string.IsNullOrEmpty(optionRequest.to)))
                {
                        return BadRequest(new
                        {
                            message = "Quỳnh phải nhập số!"
                        });
                }
                else
                {
                    if (!string.IsNullOrEmpty(optionRequest.from))
                    {
                        if ((from <= 0 || from > quizzes.Count) )
                        {
                            return BadRequest(new
                            {
                                message = "Quỳnh nhập số câu ngoài phạm vi rùi!"
                            });
                        }

                    }
                    if (!string.IsNullOrEmpty(optionRequest.to))
                    {
                        if ( (to <= 0 || to > quizzes.Count))
                        {
                            return BadRequest(new
                            {
                                message = "Quỳnh nhập số câu ngoài phạm vi rùi!"
                            });
                        }
                    }


                }

            }
            if (!string.IsNullOrEmpty(optionRequest.from) && string.IsNullOrEmpty(optionRequest.to) && (optionRequest.all == null || optionRequest.all == false))
            {
                List<Quiz> quizList =  quizzes.Take(from).ToList();
                return Ok(new
                {
                    quizList = quizList,
                    total = quizList.Count,
                });
            }
            else if (string.IsNullOrEmpty(optionRequest.from) && !string.IsNullOrEmpty(optionRequest.to) && (optionRequest.all == null || optionRequest.all == false))
            {
              
                List<Quiz> quizList = quizzes.Take(to).ToList();
                return Ok(new
                {
                    quizList = quizList,
                    total = quizList.Count,
                });
            }
            else if (!string.IsNullOrEmpty(optionRequest.from) && !string.IsNullOrEmpty(optionRequest.to) && (optionRequest.all == null || optionRequest.all == false))
            {
                if(from >= to)
                {
                    return BadRequest(new
                    {
                        message = "Giá trị cuối phải lớn hơn giá trị đầu"
                    });
                }
                List<Quiz> quizList = quizzes.Skip(from - 1).Take(to - (from - 1)).ToList();
                return Ok(new
                {
                    quizList = quizList,
                    total = quizList.Count,
                });
            }
            else if(string.IsNullOrEmpty(optionRequest.from) && string.IsNullOrEmpty(optionRequest.to) && optionRequest.all != null)
            {
                return Ok(new
                {
                    quizList = quizzes,
                    total = quizzes.Count,
                });
            }
            else
            {
                return BadRequest(new
                {
                    message = "Có lỗi rùi Quỳnh!"
                });
            }
            return Ok();
        }

       private List<Quiz> ImportQuizFromTextFile(string filePath)
            {
                var lines = System.IO.File.ReadAllLines(filePath);
                var questions = new List<Quiz>();

                Quiz currentQuestion = null;
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (char.IsDigit(line[0]) && line.Contains("."))
                    {
                        if (currentQuestion != null)
                        {
                            questions.Add(currentQuestion);
                        }

                        currentQuestion = new Quiz
                        {
                            QuestionText = line.Substring(line.IndexOf('.') + 1).Trim(),
                            Options = new List<string>(),
                            CorrectAnswer = -1 // Để xác định sau
                        };
                    }
                    else if (line.StartsWith("-"))
                    {
                        var correctOption = line[1]; 
                        currentQuestion?.Options.Add(line.Substring(3).Trim());
                        currentQuestion.CorrectAnswer = correctOption - 'A'+1;
                    }
                    else if (line.Length > 1 && char.IsLetter(line[0]) && line[1] == '.')
                    {
                        currentQuestion?.Options.Add(line.Substring(2).Trim());
                    }
                }

                if (currentQuestion != null)
                {
                    questions.Add(currentQuestion);
                }

                return questions;
            }

    }
}
