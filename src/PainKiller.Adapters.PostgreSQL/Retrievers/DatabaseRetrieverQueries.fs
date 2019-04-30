module PainKiller.Adapters.PostgreSQL.Retrievers.DatabaseRetrieverQueries

module TableQueries =
    let getAllTablesQuery = """
        SELECT table_name, table_schema
        FROM information_schema.tables
        WHERE table_schema NOT IN ('pg_catalog', 'information_schema')
        AND table_schema NOT LIKE 'pg_toast%'
        AND table_type = 'BASE TABLE';
    """

    let getColumns = sprintf """
        SELECT column_name, ordinal_position, column_default, is_nullable, udt_name::regtype::text as data_type, character_maximum_length 
        FROM information_schema.columns
        WHERE table_schema = '%s' AND table_name = '%s'
    """

    let getTableConstraints = sprintf """
        SELECT c.conname                              AS constraint_name,
        c.contype                                     AS constraint_type,
        sch.nspname                                   AS table_schema,
        tbl.relname                                   AS table_name,
        ARRAY_AGG(col.attname ORDER BY u.attposition) AS columns,
        pg_get_constraintdef(c.oid)                   AS definition
        FROM pg_constraint c
        JOIN LATERAL UNNEST(c.conkey) WITH ORDINALITY AS u(attnum, attposition) ON TRUE
        JOIN pg_class tbl ON tbl.oid = c.conrelid
        JOIN pg_namespace sch ON sch.oid = tbl.relnamespace
        JOIN pg_attribute col ON (col.attrelid = tbl.oid AND col.attnum = u.attnum)
        WHERE contype <> 'f' AND sch.nspname = '%s' AND tbl.relname = '%s'
        GROUP BY constraint_name, constraint_type, table_schema, table_name, definition
        ORDER BY table_schema, table_name;
    """

    let getForeignKeys = sprintf """
        SELECT * FROM (
            SELECT
                c.conname AS constraint_name,
                (SELECT n.nspname FROM pg_namespace AS n WHERE n.oid=c.connamespace) AS constraint_schema,

                tf.from_schema AS from_schema,
                tf.from_table AS from_table,
                (
                    SELECT ARRAY_AGG(QUOTE_IDENT(a.attname) ORDER BY t.seq)
                    FROM
                        (
                            SELECT
                                ROW_NUMBER() OVER (ROWS UNBOUNDED PRECEDING) AS seq,
                                attnum
                            FROM
                                UNNEST(c.conkey) AS t(attnum)
                        ) AS t
                        INNER JOIN pg_attribute AS a ON a.attrelid=c.conrelid AND a.attnum=t.attnum
                ) AS from_cols,

                tt.name AS to_table,
                tt.schema as to_table_schema,
                (
                    SELECT ARRAY_AGG(QUOTE_IDENT(a.attname) ORDER BY t.seq)
                    FROM
                        (
                            SELECT
                                ROW_NUMBER() OVER (ROWS UNBOUNDED PRECEDING) AS seq,
                                attnum
                            FROM
                                UNNEST(c.confkey) AS t(attnum)
                        ) AS t
                        INNER JOIN pg_attribute AS a ON a.attrelid=c.confrelid AND a.attnum=t.attnum
                ) AS to_cols,

                CASE confupdtype WHEN 'r' THEN 'restrict' WHEN 'c' THEN 'cascade' WHEN 'n' THEN 'set null' WHEN 'd' THEN 'set default' WHEN 'a' THEN 'no action' ELSE NULL END AS on_update,
                CASE confdeltype WHEN 'r' THEN 'restrict' WHEN 'c' THEN 'cascade' WHEN 'n' THEN 'set null' WHEN 'd' THEN 'set default' WHEN 'a' THEN 'no action' ELSE NULL END AS on_delete,
                CASE confmatchtype::text WHEN 'f' THEN 'full' WHEN 'p' THEN 'partial' WHEN 'u' THEN 'simple' WHEN 's' THEN 'simple' ELSE NULL END AS match_type,  -- In earlier postgres docs, simple was 'u'nspecified, but current versions use 's'imple.  text cast is required.

                pg_catalog.pg_get_constraintdef(c.oid, true) as condef
            FROM
                pg_catalog.pg_constraint AS c
                INNER JOIN (
                    SELECT pg_class.oid,
                           QUOTE_IDENT(pg_namespace.nspname) as from_schema,
                           QUOTE_IDENT(pg_class.relname) AS from_table
                    FROM pg_class INNER JOIN pg_namespace ON pg_class.relnamespace=pg_namespace.oid
                ) AS tf ON tf.oid=c.conrelid
                INNER JOIN (
                    SELECT pg_class.oid,
                           pg_namespace.nspname as schema,
                           QUOTE_IDENT(pg_class.relname) AS name
                    FROM pg_class INNER JOIN pg_namespace ON pg_class.relnamespace=pg_namespace.oid
                ) AS tt ON tt.oid=c.confrelid
            WHERE c.contype = 'f' ORDER BY 1 ) as T
        WHERE t.from_schema = '%s' AND t.from_table = '%s'
    """

module IndexQueries = 
    let getIndexQuery = """
    SELECT
    n.nspname  as schema
        ,t.relname  as tableName
        ,c.relname  as name
        ,c.relkind as kind
        ,pg_get_indexdef(indexrelid) as definition
    FROM pg_catalog.pg_class c
        JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace
        JOIN pg_catalog.pg_index i ON i.indexrelid = c.oid
        JOIN pg_catalog.pg_class t ON i.indrelid   = t.oid
    WHERE  c.relkind = 'i' AND
      n.nspname not in ('pg_catalog', 'pg_toast') AND
      pg_catalog.pg_table_is_visible(c.oid) AND
      pg_get_indexdef(indexrelid) NOT LIKE '%UNIQUE%btree%'
    ORDER BY
    n.nspname
        ,t.relname
        ,c.relname;
    """

module SequenceQueries = 
    let getSequencesQuery = """
    SELECT sequence_schema, sequence_name, data_type, increment, minimum_value, start_value 
    FROM information_schema.sequences
    """

module SchemaQueries =
    let getSchemaQuery = """
    SELECT * FROM information_schema.schemata
    WHERE schema_name not in ('pg_catalog', 'pg_toast')
    AND schema_name NOT LIKE 'pg_toast_%'
    AND schema_name NOT LIKE 'pg_temp_%';
    """

module UdtQueries =
    let getUserDefinedTypesQuery = """
       SELECT
           user_defined_type_schema as schema ,
           user_defined_type_name as name
       FROM information_schema.user_defined_types t
       """
   
    let getUserColumnsForUserDefinitedType = sprintf """
        SELECT
           attribute_name,
           ordinal_position,
           is_nullable,
           data_type,
           character_maximum_length
        FROM information_schema.attributes
        WHERE udt_schema = '%s' and udt_name = '%s';
        """

module ProcedureRetrieverQueries =
    let getProcedureQueries = """
        SELECT 
            proc.proname as name,
            ns.nspname as schema,
            pg_get_functiondef(proc.oid) as definition,
            prosrc as function_body
        FROM pg_proc proc
        LEFT JOIN pg_namespace ns on ns.oid = proc.pronamespace
            WHERE proc.prokind = 'p'
            AND ns.nspname NOT IN ('pg_catalog', 'information_schema')
            AND probin is null;
        """

module ViewRetrieverQueries = 
    let getViewQuery = """
        SELECT 
                table_schema as schema, 
                table_name as name, 
                view_definition as definition 
        FROM information_schema.views
        WHERE table_schema NOT IN ('pg_catalog', 'information_schema')
        AND table_schema NOT LIKE 'pg_toast%';
        """

module FunctionRetrieverQueries =
    let getFunctionQuery = """
    SELECT 
        proc.proname as name,
        ns.nspname as schema,
        pg_get_functiondef(proc.oid) as definition,
        prosrc as function_body
    FROM pg_proc proc
    LEFT JOIN pg_namespace ns on ns.oid = proc.pronamespace
        WHERE proc.prokind = 'f'
        AND ns.nspname NOT IN ('pg_catalog', 'information_schema')
        AND probin is null;
    """