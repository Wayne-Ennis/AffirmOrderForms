using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AffirmOrderFormsService.ViewModels.Response
{
    public interface IProposalResponse
    {

    }
    public class ProposalCreatVm : IProposalResponse
    {
        public string ProposalId { get; set; }
    }

    public class ProposalVm : IProposalResponse
    {
        public string TxPayload { get; set; }
    }

    public class RenderFormsResponse
    {
        public IProposalResponse Response { get; set; }
        public ResponseStatus Status { get; set; }
        public string Message { get; set; }
    }

    public enum ResponseStatus
    {
        BadRequest = 400,
        Error = 500,
        Success = 200,
        NotFound = 404
    }   
}
