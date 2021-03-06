﻿// <copyright file="TransactSqlTemplate.cs" company="T4 Toolbox Team">
//  Copyright © T4 Toolbox Team. All Rights Reserved.
// </copyright>

namespace T4Toolbox
{
    using System;
    using System.IO;

    /// <summary>
    /// Serves as a base class for templates that produce Transact-SQL.
    /// </summary>
    public abstract class TransactSqlTemplate : Template
    {
        /// <summary>
        /// Contains Transact-SQL reserved keywords defined in SQL Server documentation
        /// at http://msdn.microsoft.com/en-us/library/ms189822.aspx.
        /// </summary>
        private static readonly string[] reservedKeywords =
        {
            "ADD", "ALL", "ALTER", "AND", "ANY", "AS", "ASC", "AUTHORIZATION", "BACKUP",
            "BEGIN", "BETWEEN", "BREAK", "BROWSE", "BULK", "BY", "CASCADE", "CASE",
            "CHECK", "CHECKPOINT", "CLOSE", "CLUSTERED", "COALESCE", "COLLATE", "COLUMN",
            "COMMIT", "COMPUTE", "CONSTRAINT", "CONTAINS", "CONTAINSTABLE", "CONTINUE",
            "CONVERT", "CREATE", "CROSS", "CURRENT", "CURRENT_DATE", "CURRENT_TIME",
            "CURRENT_TIMESTAMP", "CURRENT_USER", "CURSOR", "DATABASE", "DBCC", "DEALLOCATE",
            "DECLARE", "DEFAULT", "DELETE", "DENY", "DESC", "DISK", "DISTINCT", "DISTRIBUTED",
            "DOUBLE", "DROP", "DUMP", "ELSE", "END", "ERRLVL", "ESCAPE", "EXCEPT", "EXEC",
            "EXECUTE", "EXISTS", "EXIT", "EXTERNAL", "FETCH", "FILE", "FILLFACTOR", "FOR",
            "FOREIGN", "FREETEXT", "FREETEXTTABLE", "FROM", "FULL", "FUNCTION", "GOTO",
            "GRANT", "GROUP", "HAVING", "HOLDLOCK", "IDENTITY", "IDENTITY_INSERT", "IDENTITYCOL",
            "IF", "IN", "INDEX", "INNER", "INSERT", "INTERSECT", "INTO", "IS", "JOIN",
            "KEY", "KILL", "LEFT", "LIKE", "LINENO", "LOAD", "MERGE", "NATIONAL", "NOCHECK",
            "NONCLUSTERED", "NOT", "NULL", "NULLIF", "OF", "OFF", "OFFSETS", "ON", "OPEN",
            "OPENDATASOURCE", "OPENQUERY", "OPENROWSET", "OPENXML", "OPTION", "OR",
            "ORDER", "OUTER", "OVER", "PERCENT", "PIVOT", "PLAN", "PRECISION", "PRIMARY",
            "PRINT", "PROC", "PROCEDURE", "PUBLIC", "RAISERROR", "READ", "READTEXT",
            "RECONFIGURE", "REFERENCES", "REPLICATION", "RESTORE", "RESTRICT", "RETURN",
            "REVERT", "REVOKE", "RIGHT", "ROLLBACK", "ROWCOUNT", "ROWGUIDCOL", "RULE",
            "SAVE", "SCHEMA", "SECURITYAUDIT", "SELECT", "SESSION_USER", "SET", "SETUSER",
            "SHUTDOWN", "SOME", "STATISTICS", "SYSTEM_USER", "TABLE", "TABLESAMPLE",
            "TEXTSIZE", "THEN", "TO", "TOP", "TRAN", "TRANSACTION", "TRIGGER", "TRUNCATE",
            "TSEQUAL", "UNION", "UNIQUE", "UNPIVOT", "UPDATE", "UPDATETEXT", "USE",
            "USER", "VALUES", "VARYING", "VIEW", "WAITFOR", "WHEN", "WHERE", "WHILE",
            "WITH", "WRITETEXT"
        };

        /// <summary>
        /// Returns a valid Transact-SQL identifier for the specified object name.
        /// </summary>
        /// <param name="name">
        /// Original object name.
        /// </param>
        /// <returns>
        /// Original <paramref name="name"/> if it is a valid Transact-SQL identifier.
        /// Otherwise, returns the <paramref name="name"/> converted to [delimited] form.
        /// </returns>
        /// <remarks>
        /// This method checks if the specified object name is a valid identifier according
        /// to rules described at http://msdn.microsoft.com/en-us/library/ms175874.aspx.
        /// </remarks>
        public static string Identifier(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            name = name.Trim();

            // Determine if the identifier is valid as is
            bool valid = true;

            // 1. First character must be a letter or _
            char c = name[0];
            valid &= Char.IsLetter(c) || c == '_';

            // 3. Subsequent characters can be letters, digits, @, $, # or _
            for (int i = 1; i < name.Length; i++)
            {
                c = name[i];
                valid &= Char.IsLetterOrDigit(c) || c == '@' || c == '$' || c == '#' || c == '_';
            }

            // 3. Identifier must not be a reserved keyword
            valid &= Array.BinarySearch(reservedKeywords, name.ToUpperInvariant()) < 0;

            // 4. Embedded spaces or special characters are not allowed (covered by code for rule 3)
            // 5. Supplementary characters are not allowed (covered by code for rule 3)

            // If the identifier is not valid, enclose it with delimiters to make it valid
            if (!valid)
            {
                name = "[" + name + "]";
            }

            return name;
        }

        /// <summary>
        /// Generates the "autogenerated" file header.
        /// </summary>
        /// <returns>
        /// Generated content.
        /// </returns>
        public override string TransformText()
        {
            this.WriteLine("-- <autogenerated>");
            this.WriteLine("--   This file was generated by T4 code generator {0}.", Path.GetFileName(TransformationContext.Host.TemplateFile));
            this.WriteLine("--   Any changes made to this file manually will be lost next time the file is regenerated.");
            this.WriteLine("-- </autogenerated>");
            this.WriteLine(string.Empty);
            return this.GenerationEnvironment.ToString();
        }
    }
}