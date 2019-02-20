using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AffirmOrderFormsService.ServiceLayer
{
    public class FmsAuthenticationException : Exception
    {
        public FmsAuthenticationException(string message) : base(message)
        {
        }
        public FmsAuthenticationException()
        {
        }

    }
}
