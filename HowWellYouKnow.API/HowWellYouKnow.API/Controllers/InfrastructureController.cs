using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace HowWellYouKnow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InfrastructureController : ControllerBase
    {
        [HttpGet("database")]
        public ActionResult Download()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "questions.db");
            return PhysicalFile(path, MimeTypes.GetMimeType(path), Path.GetFileName(path));
        }
    }
}