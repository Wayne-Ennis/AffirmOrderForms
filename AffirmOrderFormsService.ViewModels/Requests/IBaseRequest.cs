using System;

namespace AffirmOrderFormsService.ViewModels.Requests
{
    public interface IBaseRequest
    {
        Guid? CorrelationGuid { get; set; }
    }
}