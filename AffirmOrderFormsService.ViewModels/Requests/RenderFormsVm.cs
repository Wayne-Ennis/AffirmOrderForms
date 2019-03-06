using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AffirmOrderFormsService.ViewModels.Requests
{
    public class RenderFormsVm : IBaseRequest
    {
       // [Required(ErrorMessage = "{0} is a Required Parameter")]
        public string CallerOrgCode { get; set; }

     //  [Required(ErrorMessage = "{0} is a Required Parameter")]
        public Guid CorrelationGuid { get; set; }

      // [Required(ErrorMessage = "{0} is a Required Parameter")]
        public string OrderId { get; set; }

//        [Required(ErrorMessage = "{0} is a Required Parameter")]
        public string UserName { get; set; }
    }
}
