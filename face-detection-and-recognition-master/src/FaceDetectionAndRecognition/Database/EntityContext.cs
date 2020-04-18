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
            ConnectionString =
            new SQLiteConnectionStringBuilder()
            { DataSource = "FaceDetectionAndRecognition.db", ForeignKeys = true }
            .ConnectionString
        }, true)
        {

        }
        public DbSet<Person> Persons { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<Person>();
            builder.ToTable(nameof(Person));
            builder.HasKey(p => p.Id);
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<EntityContext>(modelBuilder);
            //Database.SetInitializer(sqliteConnectionInitializer);
            DataBase.SetInitializer(sqliteConnectionInitializer);
        }



        public static EntityContext CreateInstance()
        {
            var context = new EntityContext();
            context.Database.CreateIfNotExists();
            return context;
        }


    }
}
