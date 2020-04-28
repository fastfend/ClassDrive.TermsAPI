using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ClassDrive.TermsAPI.Controllers
{
    public class User
    {
        public string key;
        public string email;
    }

    [Route("[controller]")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post([FromBody] User user)
        {
            bool IsValid(string emailAddress)
            {
                try
                {
                    MailAddress m = new MailAddress(emailAddress);
                    return true;
                }
                catch (FormatException)
                {
                    return false;
                }
            }

            if (user.key == "dupamaryny09182309810923890157091831937")
            {
                if(IsValid(user.email))
                {
                    try
                    {
                        UserAdder userAdder = new UserAdder();
                        userAdder.AddPermission(user.email);
                        return StatusCode(200);
                    }
                    catch
                    {
                        return StatusCode(500);
                    }
                    
                }
                return StatusCode(500);
            }
            else
            {
                return StatusCode(500);
            }
        }
    }
}
