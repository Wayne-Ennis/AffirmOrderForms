using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using ADM;
using ADM.Admin;
using ADM.Kit;
using ADM.Messaging;
using ADM.Acord.TXLife.V2_10;
using AffirmOrderFormsService.ViewModels.Requests;
using AffirmOrderFormsService.ViewModels.Response;
using LogManager = Ipipeline.Logging.LogManager;
using System.Net.Http;
using System.Net.Http.Headers;
using ADM.MacroLanguage;
using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AffirmOrderFormsService.ServiceLayer
{
    public class OrderFormsService : IOrderFormsService
    {
        private readonly LogManager _logManager;
        private readonly ADMServerKit _kit;
        public OrderFormsService(LogManager logManager)
        {
            _logManager = logManager;
         _kit = new ADMServerKit();
           // _kit.Authenticate();
        }

        public IAdmTrans GetTrans(string callerOrgCode, string userName, string orderId)
        {
            try
            {
                var configName = callerOrgCode + "_PROD_OrderSvc";
                var kit = new ADMServerKit(configName, null);
                var authRequest = new AuthenticationRequest
                {
                    UserName = userName,
                    UserType = kit.Parameters.GetString("AuthenticationMode", "") == "Custom"
                        ? OLI_LU_USERTYPE.GROUP
                        : OLI_LU_USERTYPE.USER
                };

                var admSubscriptionG = kit.Parameters.GetString("SubscriptionGUID", null);
                var admUserG = kit.Parameters.GetString("UserGUID", null);
                //	if (kit.Authenticate(null,req) == null)
                //		throw new ADMServerException("Kit not authenticated.");

                kit.Authenticate(authRequest, admSubscriptionG, admUserG);
                kit.InitDefaultApplication();

                //kit.InitApplicationByName("IAOE");
                var trans = new CompiledFormula($"AdmTrans[TransFamily='ADM' and TransIdentifier='{orderId}']").Eval<IAdmTrans>(kit.Database.Store);
                return trans;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public IFMSCaller GetAuthenticateCaller(string callerOrgCode, string userName)
        {



            try
            {
                var configName = callerOrgCode + "_PROD_OrderSvc";
                var kit = new ADMServerKit(configName, null);
                var authRequest = new AuthenticationRequest
                {
                    UserName = userName,
                    UserType = kit.Parameters.GetString("AuthenticationMode", "") == "Custom"
                        ? OLI_LU_USERTYPE.GROUP
                        : OLI_LU_USERTYPE.USER
                };


                var caller = kit.Instantiate<IDynamicType>(null, "FMSCaller") as IFMSCaller;
                var admSubscriptionG = kit.Parameters.GetString("SubscriptionGUID", null);
                var admUserG = kit.Parameters.GetString("UserGUID", null);

                if (admSubscriptionG != null && admUserG != null)

                {
                    caller?.Authenticate(null, authRequest, admSubscriptionG, admUserG);
                }
                else
                {
                    return null;
                }

                return caller;


            }
            catch (FileNotFoundException e)
            {
                _logManager.WriteEntry($"We can't find configuration for callerOrgCode: {callerOrgCode}",
                    LogLevel.Error, 4032);
                _logManager.WriteEntry($"{e.Message} ", LogLevel.Error, 4032);
                throw new FmsAuthenticationException($"Configuration missing for callerOrgCode: {callerOrgCode}");
            }
            catch (Exception e)
            {
                _logManager.WriteEntry(e, 4032);
                throw new FmsAuthenticationException();
            }

        }
        public string GetConfigurationParameter(string key)
        {
            //return Kit.Parameters[key];
            return "3";
        }

        private async Task<HttpResponseMessage> GenericFormServiceCall(string requestPath, IAdmTrans trans, RenderFormsVm model)
        {
            using (var client = new HttpClient())
            {

                var transMsg = new AdmMessage()
                {
                    Body = trans
                };
                var txPayLoad = transMsg.GetBodyAsString();
                //     data["Stage"] = _service.GetAuthenticatedStage();
                JObject request = new JObject{ { "CallerOrgCode", model.CallerOrgCode},
                                               { "CorrelationGUID", model.CorrelationGuid},
                                               { "Stage", "57" },
                                               { "TxPayload", transMsg.GetBodyAsString() },
                                                {"NgenDataFormat", false }
                                             };
                JToken data = JToken.FromObject(request);
                var requestServiceUrl = "http://localhost:53291"; //_service.GetConfigurationParameter("InternalFormServiceUrl");
                client.BaseAddress = new Uri(requestServiceUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var requestContent = new StringContent(data.ToString(), Encoding.UTF8, "application/json");
                return await client.PostAsync(requestPath, requestContent);
            }
        }
        private async Task<HttpResponseMessage> RenderFormsWebServiceCall(string transString,  RenderFormsVm model)
        {
            using (var client = new HttpClient())
            {
                JObject request = new JObject{ { "CallerOrgCode", model.CallerOrgCode},
                    { "CorrelationGUID", model.CorrelationGuid},
                    { "Stage", "57" },
                    { "TxPayload", transString},
                    {"NgenDataFormat", false }
                };
                JToken data = JToken.FromObject(request);
                var requestServiceUrl = "https://FormService-rnd-qa.gobluefrog.com"; //_service.GetConfigurationParameter("InternalFormServiceUrl");
                client.BaseAddress = new Uri(requestServiceUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var requestContent = new StringContent(data.ToString(), Encoding.UTF8, "application/json");
                return await client.PostAsync("https://FormService-rnd-qa.gobluefrog.com/api/forms/renderform", requestContent);
            }
        }

        public async Task<RenderFormsResponse> RenderForms(RenderFormsVm request)
        {
            try
            {
                var trans = GetTrans(request.CallerOrgCode, request.UserName, request.OrderId);
                if (trans == null)
                {
                    throw new ADMServerException("Couldn't find Trans");
                }

                var transString = trans.GetXml()?.OuterXml;
                //Create request for formservice

                //Make request to formservice

                var formServiceResponse = await RenderFormsWebServiceCall(transString, request);
                //if status code isn't good throw exception

                var formServiceResponseString = await formServiceResponse.Content.ReadAsStringAsync();

                var jObj = JObject.Parse(formServiceResponseString);
                var admString = jObj["TxPayload"].ToString();

                return null;

            }
            catch (Exception e)
            {
                _logManager.WriteEntry($"{e.Message} | CorrelationGUID: {request.CorrelationGuid}", LogLevel.Error, 4025);

                throw;
            }
        }


        public IMessage GetRequestMessage(string txPayload)
        {
            try
            {
                var reqMsg = new AdmMessage();
                var txLifeDoc = new XmlDocument();
                txLifeDoc.LoadXml(txPayload);
                reqMsg.Body = txLifeDoc;
                return reqMsg;
            }
            catch (Exception)
            {
                throw new Exception("Invalid payload: TxPayload passed is not a valid xml");
            }
        }
        public IMessage GetRequestMessage(XDocument txPayload)
        {
            try
            {
                var reqMsg = new AdmMessage();
                var txLifeDoc = new XmlDocument();
                //txLifeDoc.LoadXml(txPayload);
                using (var xmlReader = txPayload.CreateReader())
                {
                    txLifeDoc.Load(xmlReader);
                }
                reqMsg.Body = txLifeDoc;
                return reqMsg;
            }
            catch (Exception)
            {
                throw new Exception("Invalid payload: TxPayload passed is not a valid xml");
            }
        }
    }
}
