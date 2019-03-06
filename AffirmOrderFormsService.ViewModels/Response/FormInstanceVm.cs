using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AffirmOrderFormsService.ViewModels.Response
{
    public class FormInstanceVm
    {
        public string FormInstanceID { get; set; }
        public string FormName { get; set; }
        public string ProviderFormNumber { get; set; }
        public string DocumentControlNumber { get; set; }
        public string FormOptional { get; set; }
        //public string InfoSourceTC { get; set; }
        public string FormSource { get; set; }
        public string Sequence { get; set; }
        public string PdfBinaryString { get; set; }



    }
}
