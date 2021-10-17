using HowWellYouKnow.Domain.Dtos;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HowWellYouKnow.API.Exceptions
{
    [Serializable]
    public class DomainValidationException: Exception
    {
        public DomainValidationException(IResult result): base(JsonConvert.SerializeObject(result, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }))
        {

        }
    }
}
