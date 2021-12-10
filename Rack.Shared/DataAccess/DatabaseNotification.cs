using System;

namespace Rack.Shared.DataAccess
{
    /// <summary>
    /// Уведомление о произошедших изменениях в БД.
    /// </summary>
    public sealed class DatabaseNotification
    {
        public DatabaseNotification(string statement, string user, string schema, string table, object[] ids)
        {
            Statement = statement;
            User = user;
            Schema = schema;
            Table = table;
            Ids = ids;

            if (Statement.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
                SqlStatement = SqlStatement.Insert;
            else if (Statement.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase))
                SqlStatement = SqlStatement.Update;
            else if (Statement.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase))
                SqlStatement = SqlStatement.Delete;
        }

        /// <summary>
        /// Тип произведенной операции.
        /// </summary>
        public string Statement { get; }

        /// <summary>
        /// Имя пользователя, совершившего операцию.
        /// </summary>
        public string User { get; }

        /// <summary>
        /// Название схемы, где произошли изменения.
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Название таблицы, в которой произошли изменения.
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// Первичный ключ или составной первичный ключ в текстовом представлении.
        /// </summary>
        public object[] Ids { get; }

        public SqlStatement SqlStatement { get; }

        /* Заметка: так как в БД в качестве первичного ключа может иметь тип отличный от int, 
         * то в качестве типа первичного ключа в уведомлении используется строка, которую затем, 
         * определив сущность, мы можем преобразовать в нужный нам тип. */
    }
}