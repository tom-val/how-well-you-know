using HowWellYouKnow.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Infrastructure.ModelBindings
{
    public class GameStateModelBinding : IEntityTypeConfiguration<GameState>
    {
        public void Configure(EntityTypeBuilder<GameState> builder)
        {

            builder.HasMany(x => x.GameScores)
                    .WithOne(x => x.GameState)
                    .HasForeignKey(x => x.GameStateId);

            builder.HasOne(x => x.CurrentQuestion)
                    .WithOne(x => x.GameState)
                    .HasForeignKey<GameState>(x => x.CurrentQuestionId);
        }
    }
}
