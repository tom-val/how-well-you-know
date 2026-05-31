using HowWellYouKnow.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Infrastructure.ModelBindings
{
    public class QustionVariantModelBinding : IEntityTypeConfiguration<QuestionVariant>
    {
        public void Configure(EntityTypeBuilder<QuestionVariant> builder)
        {
            builder
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(50);
        }
    }
}
