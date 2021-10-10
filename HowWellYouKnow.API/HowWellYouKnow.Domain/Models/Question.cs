using HowWellYouKnow.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace HowWellYouKnow.Domain.Models
{
    public class Question : BaseEntity
    {
        private Question() { }
        public string Name { get; set; }
        public bool MultipleAnswers { get; set; }
        public List<QuestionVariant> Variants { get; set; }
        public List<Guess> Guesses { get; set; }
        public List<UserAnswerResult> AnswerResults { get; set; }
        public List<Answer> Answers { get; set; }
        public Guid GameId { get; set; }
        public Game Game { get; set; }
        public Guid? GameStateId { get; set; }
        public GameState GameState { get; set; }
        public int Order { get; set; }
        public Game LastQuestionGame { get; set; }

        public static Question Create(List<QuestionVariantDto> variants, string name, bool multipleAnswers, Game game)
        {
            if (string.IsNullOrEmpty(name) || name.Length > 200)
            {
                throw new ValidationException("Name length is invalid");
            }

            if (variants.Count < 2)
            {
                throw new ValidationException("Invalid number of varitants");
            }

            return new Question
            {
                Name = name,
                MultipleAnswers = multipleAnswers,
                GameId = game.Id,
                Order = game.Questions.Count + 1,
                Variants = variants.Select(v => new QuestionVariant
                {
                    Name = v.Name,
                    Notation = v.Notation,
                }).ToList()
            };
        }
    }
}
