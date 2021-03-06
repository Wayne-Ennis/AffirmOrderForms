﻿using System.Threading.Tasks;
using System.Xml;
using ADM.Messaging;
using AffirmOrderFormsService.ViewModels.Requests;
using AffirmOrderFormsService.ViewModels.Response;

namespace AffirmOrderFormsService.ServiceLayer
{
    public interface IOrderFormsService
    {
       // Task<ProposalServiceResponse> CreateProposal(CreateOrderProposalVm model);
     //   IFMSCaller GetAuthenticateCaller();
       // IFMSCaller GetAuthenticateCaller(string callerOrgCode, string userName);
        Task<FormsListResponse> RenderForms(RenderFormsVm model);
        Task<SingleFormResponse> GetSingleForm(SingleFormRequest model);
        Task<FormsListResponse> GetForms(FormsListRequest model);
    }

}