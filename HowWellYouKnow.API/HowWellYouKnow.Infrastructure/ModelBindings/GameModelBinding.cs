using HowWellYouKnow.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Infrastructure.ModelBindings
{
    public class GameModelBinding : IEntityTypeConfiguration<Game>
    {
        public void Configure(EntityTypeBuilder<Game> builder)
        {
            builder
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasOne(x => x.CreatedBy)
                .WithMany(x => x.CreatedGames)
                .HasForeignKey(x => x.CreatedByUserId);

            builder.HasMany(x => x.Questions)
                    .WithOne(x => x.Game)
                    .HasForeignKey(x => x.GameId);

            builder.HasOne(x => x.GameState)
                    .WithOne(x => x.Game)
                    .HasForeignKey<Game>(x => x.GameStateId);

            builder.HasMany(x => x.JoinedUsers)
                 .WithMany(x => x.JoinedGames);

            builder.HasOne(x => x.LastQuestion)
                    .WithOne(x => x.LastQuestionGame)
                    .HasForeignKey<Game>(x => x.LastQuestionId);
        }
    }
}
