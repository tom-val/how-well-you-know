using HowWellYouKnow.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Infrastructure.ModelBindings
{
    public class UserAnswerResultModelBinding : IEntityTypeConfiguration<UserAnswerResult>
    {
        public void Configure(EntityTypeBuilder<UserAnswerResult> builder)
        {
            builder.HasOne(x => x.Question)
                .WithMany(x => x.AnswerResults)
                .HasForeignKey(x => x.QuestionId);

            builder.HasMany(x => x.AnswerQuestionVariants)
                    .WithMany(x => x.AnswerResults);

            builder.HasMany(x => x.GuessQuestionVariants)
                    .WithMany(x => x.GuessResults);

            builder.HasOne(x => x.User)
                    .WithMany(x => x.AnswerResults)
                    .HasForeignKey(x => x.UserId);

            builder.HasOne(x => x.GuessUser)
                    .WithMany(x => x.GuessAnswerResults)
                    .HasForeignKey(x => x.GuessUserId);
        }
    }
}
