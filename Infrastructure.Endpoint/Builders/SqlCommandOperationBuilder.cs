using Domain.Endpoint.Entities;
using Infrastructure.Endpoint.Interfaces;
using System;
using System.Data.SqlClient;

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
                default: throw new ArgumentOutOfRangeException(nameof(writeOperation), "Operacion no valida");
            }
        }


        private SqlCommand GetInsertCommand()
        {
            SqlEntitySettings entitySettings = entitiesService.GetSettings<TEntity>();
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
