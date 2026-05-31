using HowWellYouKnow.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Infrastructure.ModelBindings
{
    public class UserGameScoreModelBinding : IEntityTypeConfiguration<UserGameScore>
    {
        public void Configure(EntityTypeBuilder<UserGameScore> builder)
        {
            builder.HasOne(x => x.User)
                    .WithMany(x => x.GameScores)
                    .HasForeignKey(x => x.UserId);
        }
    }
}
