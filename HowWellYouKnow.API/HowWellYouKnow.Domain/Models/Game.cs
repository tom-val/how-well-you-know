using HowWellYouKnow.Domain.Dtos;
using HowWellYouKnow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace HowWellYouKnow.Domain.Models
{
    public class Game: BaseEntity
    {
        private List<Question> questions;

        private Game() { }
        public string Name { get; private set; }
        public Guid CreatedByUserId { get; private set; }
        public User CreatedBy { get; private set; }

        public IReadOnlyCollection<Question> Questions
        {
            get => questions.ToImmutableList();
            private set => questions = value.ToList();
        }
        public Guid GameStateId { get; private set; }
        public GameState GameState { get; private set; }
        public List<User> JoinedUsers { get; private set; }
        public Question LastQuestion { get; private set; }
        public Guid? LastQuestionId { get; private set; }

        public static Game Create(string name, User createdByUser)
        {
            if(createdByUser is null)
            {
                throw new ValidationException("User does not exist");
            }

            if (string.IsNullOrEmpty(name) || name.Length > 50)
            {
                throw new ValidationException("Name cannot be shorter than 50 symbols");
            }

           return new Game
            {
                Name = name,
                CreatedByUserId = createdByUser.Id,
                JoinedUsers = new List<User> { createdByUser },
                GameState = new GameState
                {
                    CurrentGameState = CurrentGameState.NotStarted,
                }
            };
        }

        public Question AddQuestion(List<QuestionVariantDto> variants, string name, bool multipleAnswers)
        {
            var question = Question.Create(variants, name, multipleAnswers, this);

            questions.Add(question);
            LastQuestion = question;

            return question;
        }

        public void StartGame()
        {
            if (GameState.CurrentGameState != CurrentGameState.NotStarted)
            {
                throw new ValidationException("You cannot start game that already started");
            }

            if (JoinedUsers.Count < 2)
            {
                throw new ValidationException("You need at least 2 users to start the game");
            }

            GameState.CurrentGameState = CurrentGameState.AnsweringQuestion;
            GameState.CurrentQuestion = Questions.OrderBy(x => x.Order).First();
            GameState.GameScores = JoinedUsers.Select(u => new UserGameScore
            {
                UserId = u.Id,
                CurrentScore = 0
            }).ToList();
        }

        public void NextQuestion()
        {
            if (GameState.CurrentGameState != CurrentGameState.QuestionReview)
            {
                throw new ValidationException("You cannot get next question");
            }


            GameState.CurrentGameState = CurrentGameState.AnsweringQuestion;
            GameState.CurrentQuestion = Questions.OrderBy(x => x.Order).SkipWhile(item => item.Id != GameState.CurrentQuestionId).Skip(1).FirstOrDefault();

            if (GameState.CurrentQuestion == null)
            {
                GameState.CurrentGameState = CurrentGameState.GameReview;
            }
        }

        public void EndGame()
        {
            GameState.CurrentGameState = CurrentGameState.GameReview;
            GameState.CurrentQuestionId = null;
        }
    }
}
