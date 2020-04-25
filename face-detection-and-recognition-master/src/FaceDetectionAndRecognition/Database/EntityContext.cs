using FaceDetectionAndRecognition.Model;
using SQLite.CodeFirst;
using System.Data.Entity;
using System.Data.SQLite;
using DataBase = System.Data.Entity.Database;

namespace FaceDetectionAndRecognition.Database
{
    public class EntityContext : DbContext
    {
        public EntityContext() : base(new SQLiteConnection()
        {
            ConnectionString = new SQLiteConnectionStringBuilder() { DataSource = "FaceDetectionAndRecognition.db", ForeignKeys = true }.ConnectionString
        }, true)
        {

        }
        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<User>();
            builder.ToTable(nameof(User));
            builder.HasKey(p => p.Id);
            var model = modelBuilder.Build(Database.Connection);
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<EntityContext>(modelBuilder);
            DataBase.SetInitializer(sqliteConnectionInitializer);
            //sqliteConnectionInitializer.InitializeDatabase(this);
            //IDatabaseCreator sqliteDatabaseCreator = new SqliteDatabaseCreator();
            //sqliteDatabaseCreator.Create(Database, model);
        }



        public static EntityContext CreateInstance()
        {
            var context = new EntityContext();
            return context;
        }


    }
}
