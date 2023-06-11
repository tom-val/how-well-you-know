using HowWellYouKnow.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Infrastructure.ModelBindings
{
    public class QustionModelBinding : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasMany(x => x.Variants)
                .WithOne(x => x.Question)
                .HasForeignKey(x => x.QuestionId);

            builder.HasOne(x => x.Game)
                    .WithMany(x => x.Questions)
                    .HasForeignKey(x => x.GameId);
        }
    }
}
