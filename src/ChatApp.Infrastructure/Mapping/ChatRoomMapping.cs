using ChatApp.Domain.Entities.ChatRooms;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Mapping;

public class ChatRoomMapping : IEntityTypeConfiguration<ChatRoom>
{
    public void Configure(EntityTypeBuilder<ChatRoom> builder)
    {
        builder.ToTable("ChatRooms");

        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.Id)
            .HasColumnName("Id");

        builder.Property(cr => cr.Name)
            .HasColumnName("Name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(cr => cr.OwnerId)
            .HasColumnName("OwnerId")
            .IsRequired();

        builder.Property(cr => cr.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(cr => cr.Password)
            .HasColumnName("Password")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(cr => cr.IsPrivate)
            .HasColumnName("IsPrivate")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(cr => cr.Members)
            .HasColumnName("Members")
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasMany(cr => cr.Members)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
    }
}