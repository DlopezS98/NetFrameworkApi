using Domain.Endpoint.Entities;
using Infrastructure.Endpoint.Builders;
using Infrastructure.Endpoint.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Infrastructure.Endpoint.Services
{
    public class EntitiesService : IEntitiesService
    {
        private Dictionary<Type, SqlEntitySettings> entities = new Dictionary<Type, SqlEntitySettings>();

        public SqlEntitySettings GetSettings<TEntity>() where TEntity : BaseEntity
        {
            if (!entities.ContainsKey(typeof(TEntity))) throw new ArgumentOutOfRangeException(nameof(TEntity), "Entidad no encontrada");

            return entities[typeof(TEntity)];
        }

        private void BuildEntities()
        {
            SqlEntitySettings toDoSettings = GetToDoSettings();
            entities.Add(typeof(ToDo), toDoSettings);
        }

        private SqlEntitySettings GetToDoSettings()
        {
            var columns = new List<SqlColumnSettings>()
            {
                new SqlColumnSettings() { Name = "Id", DomainName = "Id", IsPrimaryKey = true, SqlDbType = SqlDbType.UniqueIdentifier },
                new SqlColumnSettings() { Name = "Title", DomainName = "Title", SqlDbType = SqlDbType.NVarChar },
                new SqlColumnSettings() { Name = "Description", DomainName = "Description", SqlDbType = SqlDbType.NVarChar },
                new SqlColumnSettings() { Name = "Done", DomainName = "Done", SqlDbType = SqlDbType.Bit },
                new SqlColumnSettings() { Name = "StartedAt", DomainName = "StartedAt", SqlDbType = SqlDbType.DateTime, IsNullable = true },
                new SqlColumnSettings() { Name = "CreatedAt", DomainName = "CreatedAt", SqlDbType = SqlDbType.DateTime, IsNullable = false },
                new SqlColumnSettings() { Name = "UpdatedAt", DomainName = "UpdatedAt", SqlDbType = SqlDbType.DateTime, IsNullable = true }
            };

            return new SqlEntitySettings()
            {
                TableName = "ToDos",
                Columns = columns
            };
        }
    }
}
