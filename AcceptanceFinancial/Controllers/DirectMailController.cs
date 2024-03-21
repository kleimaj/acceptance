using Microsoft.AspNetCore.Mvc;
using System;
using AcceptanceFinancial.Models;
using AcceptanceFinancial.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AcceptanceFinancial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DirectMailController : ControllerBase
    {
        private readonly IMartenService _martenService;
        public DirectMailController(IMartenService martenService)
        {
            _martenService = martenService;
        }

        [HttpGet]
        public async Task<DirectMail> GetDirectMail(string? promoCode)
        {
            if (promoCode == null)
            {
                return null; //allow blank code
            }
            var directMail = await _martenService.GetDirectMail(promoCode);
            if (directMail != null)
            {
                return directMail;
            }
            else
            {
                return null;
            }

        }
        [HttpPost]
        public async Task<bool> Post([FromBody] DirectMail directMail)
        {
            return await _martenService.CreateDirectMail(directMail);
        }
    }
}