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
        public void Should_Create_Question_When_Data_Is_Valid()
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

        [Fact]
        public void Should_Validate_Name_Correctly()
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

            var result = Question.Create(variants, "Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name Test name ", false, game);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Should_Validate_Variant_Count_Correctly()
        {
            var variants = new List<QuestionVariantDto>
            {
                new QuestionVariantDto
                {

                },
            };

            var user = new User();

            var game = Game.Create("Test game", user);

            var result = Question.Create(variants, "Test name", false, game);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
