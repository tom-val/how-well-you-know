using HowWellYouKnow.Domain.Dtos.Validation;
using System.Collections.Generic;

namespace HowWellYouKnow.Domain.Dtos
{
    public interface IResult
    {
        List<ValidationError> Errors { get; set; }
        bool IsSuccess { get; set; }
    }
}