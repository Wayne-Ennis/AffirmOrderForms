using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using ADM.Acord.TXLife.V2_10.Imp2;
using LogManager = Ipipeline.Logging.LogManager;
using AffirmOrderFormsService.ServiceLayer;
using AffirmOrderFormsService.ViewModels.Requests;
using AffirmOrderFormsService.ViewModels.Response;

namespace AffirmOrderFormsService.WebApi.Controllers
{

    public class OrderFormsController : ApiController
    {
        private readonly IOrderFormsService _service;
        private readonly LogManager _logManager;

        public OrderFormsController(IOrderFormsService service, LogManager logManager)
        {
            _service = service;
            _logManager = logManager;
        }

        [HttpGet]
        //[Route("api/orderforms/single")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task<IHttpActionResult> Get([FromUri] SingleFormRequest model, string formInstanceId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                SingleFormResponse response = await _service.GetSingleForm(model);

                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task<IHttpActionResult> Get([FromUri] FormsListRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                FormsListResponse response = await _service.GetForms(model);

                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

    }
}
