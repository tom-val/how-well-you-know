using FluentAssertions;
using HowWellYouKnow.Domain.Dtos;
using HowWellYouKnow.Domain.Models;
using System.Collections.Generic;
using Xunit;

namespace HowWellYouKnow.Tests
{
    public class QuestionUnitTests
    {
        [Fact]
        public void Should_Validate_Creation_Correctly()
        {
            var variants = new List<QuestionVariantDto>
            {
                new QuestionVariantDto
                {

                },
                new QuestionVariantDto
                {

                }
            };

            var user = new User();

            var game = Game.Create("Test game", user);

            var result = Question.Create(variants, "Test name", false, game);

            result.IsSuccess.Should().BeTrue();
        }
    }
}
