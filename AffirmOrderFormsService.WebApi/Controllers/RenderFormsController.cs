using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using AffirmOrderFormsService.ServiceLayer;
using AffirmOrderFormsService.ViewModels.Requests;
using AffirmOrderFormsService.ViewModels.Response;
using NLog;
using LogManager = Ipipeline.Logging.LogManager;
using System.Web.Http.Description;
using System.Web.Http.Results;
using Swashbuckle.Swagger.Annotations;

namespace AffirmOrderFormsService.WebApi.Controllers
{
      
        public class RenderFormsController : ApiController
        {
            private readonly IOrderFormsService _service;
            private readonly LogManager _logManager;
            public RenderFormsController(IOrderFormsService service, LogManager logManager)
            {
                _service = service;
                _logManager = logManager;
            }


        /// <summary>
        /// Convert Html into a Pdf Document
        /// </summary>
        /// <param name="model">HtmlToPdfRequestVm</param>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="200" >OK</response>
        /// <returns>Pdf File</returns>
        /// 
       
        [HttpPost]
        [Route("api/renderforms")]
        //[ResponseType(typeof(HttpResponseMessage))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task<IHttpActionResult> RenderForms([FromUri] RenderFormsVm model)
            {
                try
                {

                    if (!ModelState.IsValid || model == null)
                    {
                        return BadRequest(ModelState);
                    }

                    var response = await _service.RenderForms(model);

                    if (response == null)
                    {
                        return BadRequest("No Response from Service Layer");
                    }
                    if (response.Status == ResponseStatus.BadRequest)
                    {
                        return BadRequest(response.Message);
                    }
                    if (response.Status == ResponseStatus.NotFound)
                    {
                        return Content(HttpStatusCode.NotFound, response.Message);
                    }
                    return Ok(response.Response);


                }
                catch (FmsAuthenticationException e)
                {
                    _logManager.WriteEntry(e, 4036);
                    return Content(HttpStatusCode.Forbidden, e.Message);
                }
                catch (Exception e)
                {

                    _logManager.WriteEntry(e, 4036);
                    return InternalServerError(e);

                }
            }

        
        }
    }
