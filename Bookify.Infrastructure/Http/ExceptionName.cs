namespace Bookify.Infrastructure.Http
{
    /// <summary>
    /// Exceptions
    /// </summary>
    public static class ExceptionName
    {
        public const string DbUpdateException = "Microsoft.EntityFrameworkCore.DbUpdateException";
        public const string HttpResponseException = "Bookify.Infrastructure.Http.HttpResponseException";
        public const string SqlException = "System.Data.SqlClient.SqlException";
        public const string ForeignKeyViolation = "System.Data.SqlClient.SqlException.ForeignKeyViolation";
        public const string NoneSqlMiddlewareExceptionKey = "MiddlewareException";
        public const string DefaultMiddlewareExceptionKey = "DefaultMiddlewareExceptionKey";
    }
}
