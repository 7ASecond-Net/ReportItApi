using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ReportIt.Controllers
{
    public class ReportsController : ApiController
    {
      
        // POST api/values
        public bool Post([FromBody]string value)
        {
            try
            {
                string[] parts = value.Split(',');

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }       
    }
}
