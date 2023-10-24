namespace Infrastructure.Endpoint.Builders
{
    public enum SqlWriteOperation
    {
        Create,
        Update,
        Delete
    }

    public enum SqlReadOperation
    {
        Select, // SELECT * FROM TABLENAME;
        SelectById // SELECT * FROM TABLENAME WHERE ID = @ID;
    }
}
