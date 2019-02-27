using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AffirmOrderFormsService.ViewModels.Requests
{
    public class SingleFormRequest
    {
        public Guid CorrelationGuid { get; set;}
        public string CallerOrgCode { get; set; }
        public string UserName { get; set; }
        public Guid FormInstanceId { get; set; }
        public bool IncludePdfString { get; set; }
        public string OrderId { get; set; }

    }
}
