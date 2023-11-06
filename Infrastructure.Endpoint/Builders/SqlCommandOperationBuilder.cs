using Domain.Endpoint.Entities;
using Infrastructure.Endpoint.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Infrastructure.Endpoint.Builders
{
    public class SqlCommandOperationBuilder : ISqlCommandOperationBuilder
    {
        private readonly IEntitiesService entitiesService;
        public SqlCommandOperationBuilder(IEntitiesService entitiesService)
        {
            this.entitiesService = entitiesService;
        }

        public IHaveSqlWriteOperation From<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            return new SqlCommandOperationBuilder<TEntity>(entity, entitiesService);
            //throw new System.Exception();
        }

        //public IHaveSqlReadOperation Initialize<TEntity>() where TEntity : BaseEntity
        //{
        //    return new SqlCommandOperationBuilder<TEntity>();
        //    //throw new System.Exception();
        //}
    }

    public class SqlCommandOperationBuilder<TEntity> :
        IHaveSqlWriteOperation,
        IExecuteWriteBuilder
        //IHaveSqlReadOperation,
        //IHavePrimaryKeyValue,
        //IExecuteReadBuilder 
        where TEntity : BaseEntity
    {
        private SqlWriteOperation writeOperation;
        private TEntity entity;
        private readonly IEntitiesService entitiesService;

        public SqlCommandOperationBuilder(TEntity entity, IEntitiesService entitiesService)
        {
            this.entity = entity;
            this.entitiesService = entitiesService;
        }

        public IExecuteWriteBuilder WithOperation(SqlWriteOperation operation)
        {
            writeOperation = operation;
            return this;
        }

        public SqlCommand BuildWritter()
        {
            switch (writeOperation)
            {
                case SqlWriteOperation.Create:
                    return GetInsertCommand();
                case SqlWriteOperation.Delete:
                    return GetDeleteCommand();
                case SqlWriteOperation.Update:
                    return GetUpdateCommand();
                default: throw new ArgumentOutOfRangeException(nameof(writeOperation), "Operacion no valida");
            }
        }


        private SqlCommand GetInsertCommand()
        {
            SqlEntitySettings entitySettings = entitiesService.GetSettings<TEntity>();
            string sqlQuery = GetInsertQuery(entitySettings.NormalizedTableName, entitySettings.Columns);
            List<SqlParameter> parameters = GetSqlParameters(entity, entitySettings.Columns);
            SqlCommand command = new SqlCommand(sqlQuery);
            command.Parameters.AddRange(parameters.ToArray());
            return command;
        }

        private string GetInsertQuery(string tableName, List<SqlColumnSettings> columnSettings)
        {
            // INSERT INTO [Schema].[TableName] (column1, column2) VALUES (@column1, @column2);
            StringBuilder builder = new StringBuilder();
            List<string> columns = columnSettings.Where(column => !column.IsComputedColumn)
                .Select(column => column.Name)
                .ToList();
            List<string> parameters = columnSettings.Where(column => !column.IsComputedColumn)
                .Select(column => column.ParameterName)
                .ToList();
            builder.Append($"INSERT INTO ")
                .Append(tableName)
                .Append($" ({string.Join(",", columns)}) ")
                .Append($"VALUES ({string.Join(",", parameters)});");

            return builder.ToString();
        }


        private List<SqlParameter> GetSqlParameters(TEntity entity, List<SqlColumnSettings> columnSettings)
        {
            Type entityType = entity.GetType();
            List<PropertyInfo> properties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
            List<SqlParameter> parameters = new List<SqlParameter>();

            foreach (PropertyInfo property in properties)
            {
                SqlColumnSettings column = columnSettings.Where(c => c.DomainName == property.Name).FirstOrDefault();
                if (column is null) continue;
                if (column.IsComputedColumn) continue;

                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = column.ParameterName,
                    SqlDbType = column.SqlDbType,
                    Value = GetDefaultValue(entity, property, column),
                    IsNullable = column.IsNullable,
                    Direction = ParameterDirection.Input
                };

                parameters.Add(parameter);
            }

            return parameters;
        }

        private object GetDefaultValue(TEntity entity, PropertyInfo property, SqlColumnSettings column)
        {
            object value = property.GetValue(entity);
            if (value is null)
            {
                return column.IsNullable ? DBNull.Value : Activator.CreateInstance(property.PropertyType);
            }

            return value;
        }

        private SqlCommand GetUpdateCommand()
        {
            SqlEntitySettings entitySettings = entitiesService.GetSettings<TEntity>();
            throw new System.Exception();
        }

        private SqlCommand GetDeleteCommand()
        {
            SqlEntitySettings entitySettings = entitiesService.GetSettings<TEntity>();
            throw new System.Exception();
        }

        //public IHavePrimaryKeyValue WithOperation(SqlReadOperation operation)
        //{
        //    return this;
        //}

        //public IExecuteReadBuilder WithId(Guid id)
        //{
        //    return this;
        //}

        //public SqlCommand BuildReader()
        //{
        //    return new SqlCommand();
        //}
    }
}
