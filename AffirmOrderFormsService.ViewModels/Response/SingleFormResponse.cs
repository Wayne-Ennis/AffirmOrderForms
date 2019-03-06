using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AffirmOrderFormsService.ViewModels.Response
{
    public class SingleFormResponse
    {
        public Guid CorrelationGuid { get; set; }
        public FormInstanceVm FormInstance { get; set; }
    }
}
