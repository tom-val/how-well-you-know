using HowWellYouKnow.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Infrastructure.ModelBindings
{
    public class GuessModelBinding : IEntityTypeConfiguration<Guess>
    {
        public void Configure(EntityTypeBuilder<Guess> builder)
        {
            builder.HasOne(x => x.Question)
                .WithMany(x => x.Guesses)
                .HasForeignKey(x => x.QuestionId);

            builder.HasMany(x => x.QuestionVariants)
                    .WithMany(x => x.Guesses);

            builder.HasOne(x => x.User)
                    .WithMany(x => x.Guesses)
                    .HasForeignKey(x => x.UserId);

            builder.HasOne(x => x.GuessUser)
                    .WithMany(x => x.GuessFor)
                    .HasForeignKey(x => x.GuessUserId);
        }
    }
}
