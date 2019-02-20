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
using System.Net;
using System.Net.Http;
using NLog;

namespace AffirmOrderFormsService.ServiceLayer
{
    public class OrderFormsService : IOrderFormsService
    {
        private readonly LogManager _logManager;
      //  private readonly ADMServerKit _kit;
        public OrderFormsService(LogManager logManager)
        {
            _logManager = logManager;
         //   _kit = (ADMServerKit)kit;
           // _kit.Authenticate();
        }

        public IFMSCaller GetAuthenticateCaller()
        {
            IADMServerKit kit = new ADMServerKit();
            string admSubscriptionGUID = "027FD556-1C1C-4D21-9ACC-74A95098FEE2";
            string admUserGUID = "D5F86F09-6A08-4CEC-9943-B7BF730B14A6";
            const string groupUserName = "TestBFDIST1_Agent";

            AuthenticationRequest req = new AuthenticationRequest(groupUserName, "") { UserType = 1 };
            

            if (kit.Authenticate(req, admSubscriptionGUID, admUserGUID) == null)
                throw new ADMServerException("Kit not authenticated.");


            IFMSCaller caller = kit.Instantiate<IDynamicType>(null, "FMSCaller") as IFMSCaller;

            return caller;
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
        /*
        public string GetConfigurationParameter(string key)
        {
            return Kit.Parameters[key];
        }

        private async Task<HttpResponseMessage> GenericFormServiceCall(string requestPath, JToken data)
        {
             using (var client = new HttpClient())
            {
                var requestServiceUrl = _service.GetConfigurationParameter("InternalFormServiceUrl");
                client.BaseAddress = new Uri(requestServiceUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                data["CallerOrgCode"] = _service.GetAuthenticatedOrgCode();
                data["Stage"] = _service.GetAuthenticatedStage();


                // get saved NGEN data as data into form services
                if (requestPath.EndsWith("renderform"))
                {
                    data["TxPayload"] = insertedTrans;
                    data["NgenDataFormat"] = false;
                }

                var requestContent = new StringContent(data.ToString(), Encoding.UTF8, "application/json");

                requestPath = requestPath.Replace("/DistributorMaster_SSO", "");
                requestPath = requestPath.Replace("/DistributorMaster.SSO", "");
                requestPath = requestPath.Replace("/DistributorMaster", "");
                return await client.PostAsync(requestPath, requestContent);
            }
        }*/
        public async Task<RenderFormsResponse> RenderForms(RenderFormsVm request)
        {
            try
            {

                var caller = GetAuthenticateCaller(request.CallerOrgCode, request.UserName);
                if (caller == null)
                {
                   throw new FmsAuthenticationException("Unable to instantiate FMS caller");
                }

                var list = new NameValuePairList
                {
                    { "ApplicationName", "IAOE" },
                    { "OrderID", request.OrderId }
                };

                var response = await Task.Run(() => caller.InvokeEventByDescription("ACORD_AnnuityOrder_Get_Standard", null, list.ToString()));

                if (response == null)
                {
                    throw new ADMServerException("Null Response from FmsCaller");
                }

                var trans = response as IAdmTrans;
                var genericResponseDictionary = response.GetBodyAsString().Split(';');
                if (genericResponseDictionary.Length > 1)
                {

                    var dictionaryResponse = genericResponseDictionary.Where(s => s != "")
                        .ToDictionary(x => x.Split('=')[0], x => x.Split('=')[1]);

                    string resultCode;
                    dictionaryResponse.TryGetValue("ResultCode", out resultCode);
                    if (resultCode == "5")
                    {
                        string message;
                        dictionaryResponse.TryGetValue("ResultInfoDesc", out message);
                        string resultInfoCode;
                        dictionaryResponse.TryGetValue("ResultInfoCode", out resultInfoCode);
                        return new RenderFormsResponse()
                        {
                            Message = message,
                            //@TODO This need to be update base on the response code from Event
                            Status = (resultInfoCode == "2042") ? ResponseStatus.NotFound : ResponseStatus.BadRequest
                        };
                    }
                }

                return new RenderFormsResponse()
                {
                    Response = new ProposalVm() { TxPayload = response.GetBodyAsString() }
                };

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
