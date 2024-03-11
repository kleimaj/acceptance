using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AcceptanceFinancial.Services;

namespace AcceptanceFinancial.Controllers; 
public class UploadController : Controller {
    readonly IBufferedFileUploadService _bufferedFileUploadService;

        public UploadController(IBufferedFileUploadService bufferedFileUploadService)
        {
            _bufferedFileUploadService = bufferedFileUploadService;
        }
        
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(IFormFile file)
        {
            try
            {
                if (await _bufferedFileUploadService.UploadFile(file))
                {
                    ViewBag.Message = "File Upload Successful";
                }
                else
                {
                    ViewBag.Message = "File Upload Failed";
                }
            }
            catch (Exception ex)
            {
                //Log ex
                ViewBag.Message = "File Upload Failed";
            }
            return View();
        }
    }