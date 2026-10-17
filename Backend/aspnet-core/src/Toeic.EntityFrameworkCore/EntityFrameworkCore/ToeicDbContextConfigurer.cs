using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Toeic.EntityFrameworkCore;

public static class ToeicDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<ToeicDbContext> builder, string connectionString)
    {
        builder.UseSqlServer(connectionString);
    }

    public static void Configure(DbContextOptionsBuilder<ToeicDbContext> builder, DbConnection connection)
    {
        builder.UseSqlServer(connection);
    }
}
