using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AffirmOrderFormsService.ViewModels.Response
{
    public class FormsListResponse
    {
        public Guid CorrelationGuid { get; set; }
        public List<FormInstanceVm> FormInstances { get; set; }
    }
}
