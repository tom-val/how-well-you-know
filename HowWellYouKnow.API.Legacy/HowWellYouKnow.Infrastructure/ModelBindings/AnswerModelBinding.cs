using HowWellYouKnow.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Infrastructure.ModelBindings
{
    public class AnswerModelBinding : IEntityTypeConfiguration<Answer>
    {
        public void Configure(EntityTypeBuilder<Answer> builder)
        {
            builder.HasOne(x => x.Question)
                .WithMany(x => x.Answers)
                .HasForeignKey(x => x.QuestionId);

            builder.HasMany(x => x.QuestionVariants)
                    .WithMany(x => x.Answers);

            builder.HasOne(x => x.User)
                    .WithMany(x => x.Answers)
                    .HasForeignKey(x => x.UserId);
        }
    }
}
