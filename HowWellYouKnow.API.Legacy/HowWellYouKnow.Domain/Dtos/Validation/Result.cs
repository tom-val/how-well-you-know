using HowWellYouKnow.Domain.Dtos.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HowWellYouKnow.Domain.Dtos
{
    public class Result<T> : IResult
    {
        public bool IsSuccess { get; set; }

        public List<ValidationError> Errors { get; set; }

        private T value;

        [JsonIgnore]
        public T Value
        {
            get
            {
                if (IsSuccess)
                {
                    return value;
                }
                throw new ValidationException("Cannot use invalid result.");
            }
            set => this.value = value;
        }
    }
}
