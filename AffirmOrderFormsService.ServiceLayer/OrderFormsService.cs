using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public async Task<FormsListResponse> RenderForms(RenderFormsVm model)
        {
            try
            {
                var trans = GetTrans(model.CallerOrgCode, model.UserName, model.OrderId);
                if (trans == null)
                {
                    throw new ADMServerException("Couldn't find Trans");
                }

                var transString = trans.GetXml()?.OuterXml;
                //Create request for formservice

                //Make request to formservice

                var formServiceResponse = await RenderFormsWebServiceCall(transString, model);
                //if status code isn't good throw exception

                var formServiceResponseString = await formServiceResponse.Content.ReadAsStringAsync();

                var jObj = JObject.Parse(formServiceResponseString);
                var admString = jObj["TxPayload"].ToString();

                //TODO: is this Step Necessary?
                var formsMsg = new AdmMessage(){Body = admString};
                var formsTrans = formsMsg.GetBodyAsDataEntity() as IAdmTrans;

                //loopthrough formInstances of forms trans. Add each Form to trans
                formsTrans?.FormInstances.ToList().ForEach(formInstance =>
                {
                    var fi = trans.FormInstances.AddNew("FormInstance_Attachment");
                    fi.FormName = formInstance.FormName;
                    fi.ProviderFormNumber = formInstance.ProviderFormNumber;
                    fi.FormInstanceTrackingID = formInstance.FormInstanceTrackingID;
                    fi.DocumentControlNumber = formInstance.DocumentControlNumber;
                    fi.FormInstanceStatus = formInstance.FormInstanceStatus;
                    fi.FormOptional = formInstance.FormOptional;
                    fi.UserCode = formInstance.UserCode;
                    fi.Sequence = formInstance.Sequence;
                    fi.InfoSourceTC = formInstance.InfoSourceTC;
                    fi.Attachments[0].AttachmentData.Data =
                        formInstance.Attachments[0].AttachmentData.Data;

                });
                
                //Update Trans in DB

                //trans.Store.Update(trans);
                //trans.Store.Refresh(trans);
                //Return new Appropriate response Message
                var retObj = new FormsListResponse()
                {
                    CorrelationGuid = model.CorrelationGuid,
                    FormInstances = MapFormInstances(trans.FormInstances.ToList())
                };
                return retObj;

            }
            catch (Exception e)
            {
                _logManager.WriteEntry($"{e.Message} | CorrelationGUID: {model.CorrelationGuid}", LogLevel.Error, 4025);

                throw;
            }
        }

        public async Task<SingleFormResponse> GetSingleForm(SingleFormRequest model)
        {
            try
            {
                var trans = GetTrans(model.CallerOrgCode, model.UserName, model.OrderId);
                if (trans == null)
                {
                    //TODO: Log Something
                    throw new ADMServerException("Couldn't find Trans");
                }

                var formInstance = trans.FormInstances.ToList()
                    .FirstOrDefault(x => x.FormInstanceID == model.FormInstanceId.ToString());
                if (formInstance == null)
                {
                    //TODO: Log Something
                    throw new ADMServerException($"Couldn't find FormInstance: {model.FormInstanceId}");
                }

                await Task.Delay(1);
                var response = new SingleFormResponse()
                {
                    CorrelationGuid = model.CorrelationGuid,
                    FormInstance = new FormInstanceVm()
                    {
                        DocumentControlNumber = formInstance.DocumentControlNumber,
                        FormInstanceID = formInstance.FormInstanceID,
                        FormName = formInstance.FormName,
                        FormOptional = formInstance.FormOptional == "100030001" ? "Required" : "Optional",
                        //InfoSourceTC = formInstance.InfoSourceTC.ToString(),
                        PdfBinaryString = model.IncludePdfString
                            ? Convert.ToBase64String(formInstance.Attachments[0].AttachmentData.Data)
                            : string.Empty,
                        ProviderFormNumber = formInstance.ProviderFormNumber,
                        Sequence = formInstance.Sequence.ToString(),
                        FormSource = GetFormSource(formInstance.InfoSourceTC.ToString())
                    }
                };
                return response;
            }
            catch (Exception e)
            {
                _logManager.WriteEntry($"{e.Message} | CorrelationGUID: {model.CorrelationGuid}", LogLevel.Error, 4025);
                throw;
            }
        
        }

        public async Task<FormsListResponse> GetForms(FormsListRequest model)
        {
            try
            {
                var trans = GetTrans(model.CallerOrgCode, model.UserName, model.OrderId);
                if (trans == null)
                {
                    //TODO: Log Something
                    throw new ADMServerException("Couldn't find Trans");
                }

                var formsList = trans.FormInstances.ToList();
                await Task.Delay(1);
                var retObj = new FormsListResponse()
                {
                    CorrelationGuid = model.CorrelationGuid,
                    FormInstances =  MapFormInstances(formsList,model.IncludePdfString)
                };

                return retObj;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private List<FormInstanceVm> MapFormInstances(List<IFormInstance> formsList, bool includePdfString = false)
        {
            var returnList = new List<FormInstanceVm>();
            foreach (var formInstance in formsList)
            {
                var newForm = new FormInstanceVm()
                {
                    DocumentControlNumber = formInstance.DocumentControlNumber,
                    FormInstanceID = formInstance.FormInstanceID,
                    FormName = formInstance.FormName,
                    FormOptional = formInstance.FormOptional == "100030001" ? "Required" : "Optional",
                    PdfBinaryString = includePdfString
                        ? Convert.ToBase64String(formInstance.Attachments[0].AttachmentData.Data)
                        : string.Empty,
                    ProviderFormNumber = formInstance.ProviderFormNumber,
                    Sequence = formInstance.Sequence.ToString(),
                    FormSource = GetFormSource(formInstance.InfoSourceTC.ToString())
                };
                returnList.Add(newForm);
            }

            return returnList;
        }

        private string GetFormSource(string infoSource)
        {
            string ret = string.Empty;
            switch (infoSource)
            {
                case "1000300010":
                    ret = "Distributor";
                    break;
                case "1000300020":
                    ret = "Vendor";
                    break;
                case "1000300030":
                    ret = "Carrier";
                    break;
            }

            return ret;

        }
    }
}
