using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

using Isac.Isql.Collections;
using _con = Isac.Isql.ISqlConnection;

namespace Isac.Isql.QueryCommand
{
    public partial class Query
    {
        private Parser p = new Parser();
        private Fundamentals funds = new Fundamentals();

        //EVENT CLASS DATA
        private SelectStartEventArgs selectStartEvent = new SelectStartEventArgs();
        private SelectingEventArgs selectingEvent = new SelectingEventArgs();
        private SelectEndEventArgs selectEndEvent = new SelectEndEventArgs();

        //EVENTS
        public event EventHandler<SelectStartEventArgs> SelectStart;
        public event EventHandler<SelectingEventArgs> Selecting;
        public event EventHandler<SelectEndEventArgs> SelectEnd;

        //EVENT LOGIC
        protected virtual void OnSelect(SelectStartEventArgs e)
        {
            SelectStart?.Invoke(this, e);
        }
        protected virtual void OnSelecting(SelectingEventArgs e)
        {
            Selecting?.Invoke(this, e);
        }
        protected virtual void OnSelectEnd(SelectEndEventArgs e)
        {
            SelectEnd?.Invoke(this, e);
        }

        //this tells weather selectdistinct is used or not
        private bool method = false;

        //this holds the last used method name
        private string fromWhichMethod = "";

        //this hold the element inserted in the select method
        private string[] select_element_arr = null;

        //this hold the element selected in form of object
        private SelectObject selectObject = new SelectObject();

        //for where subquery-- for each row in the subquery datatable
        private int subquery_row = 0;

        //private connection
        private Connection usedCon = new Connection("", "", "");

        //elements queried are grouped by respective nodes
        internal Dictionary<string, string> Grouped_Elements = new Dictionary<string, string>();
        internal DataTableSet dataTableSet = new DataTableSet();
        internal DataTable MainTable = new DataTable();


        #region Data Retrival GET_DATA
        /// <summary>
        ///	use to get the query result
        /// </summary>
        /// <returns>returns a datatable with the query result without a reference</returns>
        public DataTable GetData()
        {
            if (MainTable != null)
                return MainTable.Copy();
            return new DataTable();
        }

        public DataTable GetData(params string[] columns)
        {
            if (MainTable != null)
            {
                DataTable dt = new DataTable();
                List<Column> col_li = new List<Column>();
                foreach (var col in columns)
                {

                    dt.SetColumn(MainTable.Head.GetColumn(col), MainTable[col].ToCellArray());
                }
                return dt;
            }

            return new DataTable();
        }

        /// <summary>
        ///	use to get the query result
        /// </summary>
        /// <returns>returns a datatable with the query result with a reference for updates</returns>
        public DataTable GetTempData()
        {
            if (MainTable != null)
                return (DataTable)MainTable.Clone();
            return new DataTable();
        }
        #endregion

        #region  Table Collision
        public Query UnionAll(Query q)
        {
            Detail detail1 = this.MainTable.GetDetail(this.MainTable.Head.NameList());
            Detail detail2 = q.MainTable.GetDetail(q.MainTable.Head.NameList());

            string err = (detail1.DataTypes.SequenceEqual(detail2.DataTypes)) ? ("columns length are not equal." + "\nINSTANCE11: " + string.Join(" ", detail1.ColumnNames.Length)
                                                    + "\nINSTANCE2: " + string.Join(" ", detail2.ColumnNames.Length))
                                                    : ("instance data types are not in the same sequence." + "\nINSTANCE1: " + string.Join<Type>(" ", detail1.DataTypes)
                                                                         + "\nINSTANCE2: " + string.Join<Type>(" ", detail2.DataTypes));

            if (detail1.ColumnNames.Length == detail2.ColumnNames.Length && detail1.DataTypes.SequenceEqual(detail2.DataTypes))
                this.MainTable.AddRows(q.MainTable.ToArray<Row>());

            else throw new ISqlArguementException($"Error: can not make an UNION operation between this two instance\n'{err}'");
            return this;
        }

        public Query Union(Query q)
        {
            Detail detail1 = this.MainTable.GetDetail(this.MainTable.Head.NameList());
            Detail detail2 = q.MainTable.GetDetail(q.MainTable.Head.NameList());

            string err = (detail1.DataTypes.SequenceEqual(detail2.DataTypes)) ? ("columns length are not equal." + "\nINSTANCE11: " + string.Join(" ", detail1.ColumnNames.Length)
                                                    + "\nINSTANCE2: " + string.Join(" ", detail2.ColumnNames.Length))
                                                    : ("instance data types are not in the same sequence." + "\nINSTANCE1: " + string.Join<Type>(" ", detail1.DataTypes)
                                                                         + "\nINSTANCE2: " + string.Join<Type>(" ", detail2.DataTypes));

            if (detail1.ColumnNames.Length == detail2.ColumnNames.Length && detail1.DataTypes.SequenceEqual(detail2.DataTypes))
            {
                RowCollection collection1 = this.MainTable.Body;
                RowCollection collection2 = q.MainTable.Body;
                RowCollection collection3 = new RowCollection();
                foreach (Row row in collection2)
                {
                    if (!collection3.HasRow(row))
                        collection3.AddRow(row);
                }

                this.MainTable.Body = collection3;
            }

            else throw new ISqlArguementException($"Error: can not make an UNION operation between this two instance\n'{err}'");
            return this;
        }

        public Query Intersect(Query q)
        {
            Detail detail1 = this.MainTable.GetDetail(this.MainTable.Head.NameList());
            Detail detail2 = q.MainTable.GetDetail(q.MainTable.Head.NameList());

            string err = (detail1.DataTypes.SequenceEqual(detail2.DataTypes)) ? ("columns length are not equal." + "\nINSTANCE11: " + string.Join(" ", detail1.ColumnNames.Length)
                                                    + "\nINSTANCE2: " + string.Join(" ", detail2.ColumnNames.Length))
                                                    : ("instance data types are not in the same sequence." + "\nINSTANCE1: " + string.Join<Type>(" ", detail1.DataTypes)
                                                                         + "\nINSTANCE2: " + string.Join<Type>(" ", detail2.DataTypes));

            if (detail1.ColumnNames.Length == detail2.ColumnNames.Length && detail1.DataTypes.SequenceEqual(detail2.DataTypes))
            {
                //foreach(Row row in q.MainTable)
                RowCollection collection1 = this.MainTable.Body;
                RowCollection collection2 = q.MainTable.Body;
                RowCollection collection3 = new RowCollection();
                foreach (Row row in collection2)
                {
                    if (collection1.HasRow(row))
                        collection3.AddRow(row);
                }

                this.MainTable.Body = collection3;
            }

            else throw new ISqlArguementException($"Error: can not make an INTERSECT operation between this two instance\n'{err}'");

            return this;
        }

        public Query Except(Query q)
        {
            Detail detail1 = this.MainTable.GetDetail(this.MainTable.Head.NameList());
            Detail detail2 = q.MainTable.GetDetail(q.MainTable.Head.NameList());

            string err = (detail1.DataTypes.SequenceEqual(detail2.DataTypes)) ? ("columns length are not equal." + "\nINSTANCE11: " + string.Join(" ", detail1.ColumnNames.Length)
                                                    + "\nINSTANCE2: " + string.Join(" ", detail2.ColumnNames.Length))
                                                    : ("instance data types are not in the same sequence." + "\nINSTANCE1: " + string.Join<Type>(" ", detail1.DataTypes)
                                                                         + "\nINSTANCE2: " + string.Join<Type>(" ", detail2.DataTypes));

            if (detail1.ColumnNames.Length == detail2.ColumnNames.Length && detail1.DataTypes.SequenceEqual(detail2.DataTypes))
            {
                //foreach(Row row in q.MainTable)
                RowCollection collection1 = this.MainTable.Body;
                RowCollection collection2 = q.MainTable.Body;
                RowCollection collection3 = new RowCollection();
                foreach (Row row in collection1)
                {
                    if (!collection2.HasRow(row))
                        collection3.AddRow(row);
                }
                this.MainTable.Body = collection3;
            }

            else throw new ISqlArguementException($"Error: can not make a PRIME operation between this two instance\n'{err}'");

            return this;
        }

        public Query IPrime(Query q)
        {
            Detail detail1 = this.MainTable.GetDetail(this.MainTable.Head.NameList());
            Detail detail2 = q.MainTable.GetDetail(q.MainTable.Head.NameList());

            string err = (detail1.DataTypes.SequenceEqual(detail2.DataTypes)) ? ("columns length are not equal." + "\nINSTANCE11: " + string.Join(" ", detail1.ColumnNames.Length)
                                                    + "\nINSTANCE2: " + string.Join(" ", detail2.ColumnNames.Length))
                                                    : ("instance data types are not in the same sequence." + "\nINSTANCE1: " + string.Join<Type>(" ", detail1.DataTypes)
                                                                         + "\nINSTANCE2: " + string.Join<Type>(" ", detail2.DataTypes));

            if (detail1.ColumnNames.Length == detail2.ColumnNames.Length && detail1.DataTypes.SequenceEqual(detail2.DataTypes))
            {
                //foreach(Row row in q.MainTable)
                RowCollection collection1 = this.MainTable.Body;
                RowCollection collection2 = q.MainTable.Body;
                RowCollection collection3 = new RowCollection();
                foreach (Row row in collection2)
                {
                    if (!collection1.HasRow(row))
                        collection3.AddRow(row);
                }
                this.MainTable.Body = collection3;
            }

            else throw new ISqlArguementException($"Error: can not make a IPRIME operation between this two instance\n'{err}'");

            return this;
        }
        #endregion

        #region select
        //private List<Collections.Column> columns = new List<Column>();
        internal SelectObject ParallelSelectQuery(string[] elementdata, CollectiveSelectBlock collectives = null, bool allow_collective = true, bool never_collective = false)
        {
            SelectObject selectObj = new SelectObject();
            // Using Task for this in future updates
            // This synchronizely solves all query blocks
            int currIndex = 0;
            foreach (var query in elementdata)
            {
                try
                {
                    string data = elementdata[currIndex].Trim();

                    if (data == "") throw new ISqlArguementException($"Error: empty query encountered\nQuery Segment: {currIndex}");

                    string anno = (data != "") ? p.GetType(data[0]) : "";

                    string query_parent = p.GetType(data[0]);
                    object[] obj = p.GetNext(0, data,
                            (p.GetType(data[0]) == "*") ? "all" : p.GetType(data[0]),
                            p.STable);

                    //val, nxt, anno, index

                    //Collective
                    if (obj[2].ToString() == "[" && query_parent == "@" && data.EndsWith("]"))
                    {
                        if (!allow_collective || never_collective)
                            throw new ISqlException("Error: " + ((!never_collective) ? "nested collective select is not allowed" : "collective select is not valid in this block"));

                        CollectiveSelectBlock csb = new CollectiveSelectBlock();
                        csb.Index = currIndex;
                        csb.BlockType = SelectQueryType.COLLECTIVE;
                        csb.CollectiveName = "@" + obj[0];

                        string[] queries = Parser.QuerySeprator(data.Substring(data.IndexOf("[") + 1,
                                                data.LastIndexOf("]") - data.IndexOf("[") - 1));
                        ParallelSelectQuery(queries, csb, false);
                        csb.PassName();

                        selectObj.AddBlocks(csb.GiveBack());
                    }

                    //All with no Alias Done
                    else if (data == "*")
                    {
                        SelectBlock sb = new SelectBlock();
                        sb.Index = currIndex;
                        sb.BlockType = SelectQueryType.ALL;
                        sb.Query.Add("*");
                        sb.done = true;
                        if (collectives == null)
                            selectObj.Add(sb);
                        else
                            collectives.Add(sb);
                        
                    }

                    // All with alias
                    else if (data.StartsWith("@") && data.EndsWith("*") && data.Count(x => x == '.') == 1 && obj[2].ToString() == ".")
                    {
                        SelectBlock sb = new SelectBlock();
                        sb.Index = currIndex;
                        sb.BlockType = SelectQueryType.ALLALIAS;
                        sb.Query.Add(data);
                        sb.done = true;
                        if (collectives == null)
                            selectObj.Add(sb);
                        else
                            collectives.Add(sb);
                    }

                    //Logic
                    else if (data.Contains(">") || data.Contains("<") || data.Contains("<=") || data.Contains(">=") || data.Contains("<>")
                        || data.Contains("!=") || data.Contains("==") || data.Contains("&&") || data.Contains("||") || data.Contains("!"))
                    {
                        string[] aliased_data = Parser.AliasGetter(data);
                        var logicEngine = new Isac.Isql.Logistics.LogicExpressionEngine(aliased_data[0]);
                        SelectBlock selectBlock = logicEngine.GetSelectBlock();
                        selectBlock.Index = currIndex;
                        selectBlock.BlockType = SelectQueryType.LOGIC;
                        selectBlock.QueryAlias = (aliased_data.Length == 2) ? aliased_data[1] : aliased_data[0];
                        selectBlock.done = true;
                        if (collectives == null)
                            selectObj.Add(selectBlock);
                        else
                            collectives.Add(selectBlock);
                    }

                    //Maths
                    else if ((data.Contains("+") || data.Contains("-") || data.Contains("*") || data.Contains("/") || data.Contains("%")
                        || data.Contains("^")) && !data.StartsWith("*"))
                    {
                        string[] aliased_data = Parser.AliasGetter(data);
                        var expEngine = new Isac.Isql.Logistics.ExpressionEngine(aliased_data[0]);
                        SelectBlock selectBlock = expEngine.GetSelectBlock();
                        selectBlock.Index = currIndex;
                        selectBlock.BlockType = SelectQueryType.EXPRESSION;
                        selectBlock.QueryAlias = (aliased_data.Length == 2) ? aliased_data[1] : aliased_data[0];
                        selectBlock.done = true;
                        if (collectives == null)
                            selectObj.Add(selectBlock);
                        else
                            collectives.Add(selectBlock);
                    }

                    //Simple Singles (ie:- number var/keyword string(nvar))			
                    else if (anno == "@" || anno == "name" || anno == "number" || anno == "(" || anno == "value" || anno == "nval" || anno == "[")
                    {
                        string[] aliased_data = Parser.AliasGetter(data);
                        var logicEngine = new Isac.Isql.Logistics.LogicExpressionEngine(aliased_data[0]);
                        SelectBlock selectBlock = logicEngine.GetSelectBlock();
                        selectBlock.Index = currIndex;
                        selectBlock.QueryAlias = (aliased_data.Length == 2) ? aliased_data[1] : aliased_data[0];
                        selectBlock.BlockType = SelectQueryType.SINGLE;
                        selectBlock.done = true;
                        if (collectives == null)
                            selectObj.Add(selectBlock);
                        else
                            collectives.Add(selectBlock);
                    }

                    else throw new ISqlException($"ERROR: unrecognized query '{data}'");
                } catch(Exception ex)
                {
                    throw;
                }
                currIndex++;
            }

            return selectObj;
        }
        
        private void Altemate_Checker(string data)
        {

            if (string.IsNullOrEmpty(data) || string.IsNullOrWhiteSpace(data))
                throw new ISqlException("ERROR: parameter cannot be empty");

            string parent = "";
            string element = data;
            //checking FWM 'FromWhichMethod'

            string acceptable = "@_abcdefghijklmnopqrstuvwxyz0123456789:[ ]().,^+-`><=&|'!*/%$";

            int opculy = element.Count(o => (o == '['));

            int cloculy = element.Count(a => (a == ']'));

            int at = element.Count(a => (a == '@'));

            int opparan = element.Count(b => (b == '('));

            int cloparan = element.Count(d => (d == ')'));

            if (opculy != cloculy || cloparan != opparan)
                throw new ISqlException($"ERROR: problem with syntax.\nSquare brackets or parenthesis are out of order '{opculy}'");

            selectStartEvent.user = usedCon.UserID;
            selectStartEvent.charSet = usedCon.CharSet;
            selectStartEvent.time = DateTime.Now;

            OnSelect(selectStartEvent);

            //triming multiple whitespace to be one each
            element = funds.SpaceRemover(element, acceptable);

            //seprating the element query
            string[] elementdata = Parser.QuerySeprator(element);

            selectObject = ParallelSelectQuery(elementdata);

            //Console.WriteLine(selectObject);
        }

        /// <summary>
        /// number of rows to set for the data queried
        /// </summary>
        /// <param name="limit">number of rows to set</param>
        /// <returns></returns>
        public Query Limit(int limit)
        {
            if (!MainTable.IsEmpty)
            {
                MainTable = MainTable.Limit(limit);
            }
            return this;
        }

        /// <summary>
        /// number of rows to jump(skip) for the data queried
        /// </summary>
        /// <param name="offset">number of data  to jump(skip)</param>
        /// <returns></returns>
        public Query Offset(int offset)
        {
            if (!MainTable.IsEmpty)
            {
                MainTable = MainTable.Offset(offset);
            }
            return this;
        }

        /// <summary>
        /// the list of columns to select
        /// </summary>
        /// <param name="element">select column list with expression(s)</param>
        /// <returns></returns>
        public Query Select(string element)
        {
            if (string.IsNullOrWhiteSpace(element))
                throw new ArgumentException($"'{nameof(element)}' cannot be null or whitespace.", nameof(element));

            fromWhichMethod = "select";

            Grouped_Elements.Clear();

            if (_con.CurrentConnection.ConnectionState != 1)
                throw new ISqlConnectionNotFoundException($"Error: no connection found");

            usedCon = _con.CurrentConnection;
            Altemate_Checker(element);

            selectObject.SelectObjectType = SelectType.NONE;

            method = false;
            return this;
        }

        /// <summary>
        /// the list of columns to select with no duplicates
        /// </summary>
        /// <param name="element">selecte column list with expression(s)</param>
        /// <returns></returns>
        public Query SelectDistinct(string element)
        {
            fromWhichMethod = "select";

            if (_con.CurrentConnection.ConnectionState != 1)
                throw new ISqlConnectionNotFoundException($"Error: no connection found");

            usedCon = _con.CurrentConnection;
            Altemate_Checker(element);
            selectObject.SelectObjectType = SelectType.DISTINCT;
            method = true;
            return this;// new Query(fromWhichMethod, query_connection_index, Grouped_Elements, true, select_element_arr);
        }
        #endregion


        private Query FromTableObjects(string tableNames, JoinTable[] joins = null, Query subQuery = null)
        {
            //for table names case insensitive and removing of too much whitespace
            tableNames = tableNames.ToLower();
            tableNames = funds.SpaceRemover(tableNames);

            //for to decrypt datas read from database
            Encryption encrypt = new Encryption();

            //table names and alias
            List<string[]> table_name_and_alias = new List<string[]>();
            List<string[]> join_table_name_and_alias = new List<string[]>();

            //table column names, data types and number of rows
            List<object[]> table_heads = new List<object[]>();
            List<object[]> join_table_heads = new List<object[]>();

            //database table names .idb or .tb stored here
            List<string> db_tables = new List<string>();
            List<string> join_db_table = new List<string>();

            //sum of all table rows 
            int tables_dimension = 1;

                         //table names broken into names:alias....
            var tables = Parser.QuerySeprator(tableNames.Trim());

            //fixing table names and alias
            #region
            foreach (var tb in tables)
            {
                string table = tb.Trim();
                //check for empty entry
                if (table == "")
                    throw new ISqlException("Error: empty arguements encountered");

                //check if there are multiple alias symbols
                if (tables.Count(x => x == ":") > 1)
                    throw new ISqlException($"Error: multiple alias ':' encountered");

                var name_alias = Parser.AliasGetter(table);

                table_name_and_alias.Add(new string[] { name_alias[0], (name_alias.Length == 2) ? name_alias[1] : "`" });
                db_tables.Add(name_alias[0] + ".idb");
            }

            //fixing table names and alias for joins
            if(joins != null)
            {
                foreach (var join in joins)
                {
                    if (table_name_and_alias.Contains(new string[] { join.Table, join.Alias }))
                        throw new ISqlException("Error: tables with same name and alias can cause ambiguity");

                    join_table_name_and_alias.Add(new string[] { join.Table, join.Alias });
                    join_db_table.Add(join.Table + ".idb");
                }
            }
            #endregion

            //for holidng normal table names so that we will not read from database if the same 
            //table was include twice
            Dictionary<string, object[]> same_tables = new Dictionary<string, object[]>();

            //fatching table headers for normal tables
            #region
            foreach (var base_table_names in db_tables)
            {
                if (same_tables.ContainsKey(base_table_names))
                {
                    table_heads.Add(same_tables[base_table_names]);
                    tables_dimension *= (int)same_tables[base_table_names][2];
                    continue;
                }

                using (FileStream fs = new FileStream(usedCon.Database, FileMode.Open))
                {
                    using (ZipArchive zipArchive = new ZipArchive(fs, ZipArchiveMode.Read))
                    {

                        var base_name_head = zipArchive.GetEntry(base_table_names + ".head");
                        if (base_name_head == null)
                            throw new ISqlTableNotFoundException($"Error: table '{base_table_names.Substring(0, base_table_names.LastIndexOf("."))}' do not exists in the database '{new FileInfo(ISqlConnection.CurrentConnection.Database).Name}'");

                        int tbrow = 0;

                        var hdt = funds.HeadReader(out tbrow, base_name_head, usedCon, encrypt);
                        var det = hdt.GetDetail(hdt.Head.NameList());

                        tables_dimension *= tbrow;

                        var infos = new object[] { string.Join("\t", det.ColumnNames),
                                                                string.Join("\t", Parser.StringTypeConverter(det.DataTypes)),
                                                                tbrow};

                        table_heads.Add(infos);

                        same_tables.Add(base_table_names, infos);
                    }
                }
            }
            #endregion

            //fetching table headers for join tables
            #region
            //for getting joins index
            int join_criteria_index = 0;
            if (join_db_table.Count > 0)
            {
                foreach (var base_table_names in join_db_table)
                {
                    if (same_tables.ContainsKey(base_table_names))
                    {
                        join_table_heads.Add(same_tables[base_table_names]);
                        tables_dimension *= (int)same_tables[base_table_names][2];
                        join_criteria_index++;
                        continue;
                    }

                    using (FileStream fs = new FileStream(usedCon.Database, FileMode.Open))
                    {
                        using (ZipArchive zipArchive = new ZipArchive(fs, ZipArchiveMode.Read))
                        {

                            var base_name_head = zipArchive.GetEntry(base_table_names + ".head");
                            if (base_name_head == null)
                                throw new ISqlTableNotFoundException($"Error: table '{base_table_names.Substring(0, base_table_names.LastIndexOf("."))}' do not exists in the database '{new FileInfo(ISqlConnection.CurrentConnection.Database).Name}'");

                            int tbrow = 0;

                            var hdt = funds.HeadReader(out tbrow, base_name_head, usedCon, encrypt);
                            var det = hdt.GetDetail(hdt.Head.NameList());

                            tables_dimension *= tbrow;

                            var infos = new object[] { string.Join("\t", det.ColumnNames),
                                                                string.Join("\t", Parser.StringTypeConverter(det.DataTypes)),
                                                                tbrow,
                                                                joins[join_criteria_index].On};

                            join_table_heads.Add(infos);

                            same_tables.Add(base_table_names, infos);
                        }
                    }
                    join_criteria_index++;
                }
            }
            #endregion

            //clearing same table for freeing resources
            same_tables.Clear();

            selectingEvent.user = usedCon.UserID;
            selectingEvent.charSet = usedCon.CharSet;
            selectingEvent.time = DateTime.Now;

            OnSelecting(selectingEvent);

            TableGroup tableGroups = new TableGroup(tables_dimension);
            if (joins == null)
                tableGroups = TableGroup.GenerateGroup(table_name_and_alias, table_heads, tables_dimension, selectObject);

            else if(joins != null)
                tableGroups = TableGroup.GenerateGroup(table_name_and_alias, table_heads, join_table_name_and_alias, join_table_heads, tables_dimension, selectObject);

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            
            
            MainTable.Defined(tableGroups.ColumnDefs.GenerateColumnDefination<string, Type>());

            Task.WaitAll(
                Task.Run(async() =>
                {
                    await tableGroups.GroupQuery(usedCon, MainTable, selectObject);
                    while (selectObject.QueryIndex < selectObject.Dimension)
                    {
                        while (selectObject.QueryIndex < tableGroups.Query)
                        {
                            selectObject.MakeQuery(tableGroups.SelectTargets, MainTable, tableGroups);
                        }
                        //Console.WriteLine("Stuck Here");
                    }
                })
            );
            

            if (!tableGroups.IsDone)
                throw new ISqlException($"FATAL ERROR: runtime error");
            
            watch.Stop();
            //Console.WriteLine("Done At: " + watch.Elapsed);
            selectEndEvent.user = usedCon.UserID;
            selectEndEvent.charSet = usedCon.CharSet;
            selectEndEvent.time = DateTime.Now;

            OnSelectEnd(selectEndEvent);
 
            return this;
        }


        /// <summary>
        /// list of table[names] to fetch data from
        /// </summary>
        /// <param name="table_name">name(s) of target table(s)</param>
        /// <returns></returns>
        public Query From(string tables)
        {
            if (usedCon.ConnectionState != 1)
                throw new ISqlConnectionNotFoundException("Error: connection not found");

                string table_name = tables.ToLower();
            if (string.IsNullOrEmpty(table_name) || string.IsNullOrWhiteSpace(table_name))
                throw new Exception($"Error: parameter is empty");
            FromTableObjects(tables);

            if (method)
                MainTable.Distinct();

            return this;
        }



        public Query From(string tables, params JoinTable[] joins)
        {
            if (usedCon.ConnectionState != 1)
                throw new ISqlConnectionNotFoundException("Error: connection not found");

            string table_name = tables.ToLower();
            if (string.IsNullOrEmpty(table_name) || string.IsNullOrWhiteSpace(table_name))
                throw new ISqlException($"Error: tables parameter is empty");

            if (joins == null || joins.Length == 0)
                throw new ISqlException($"Error: no joins found");

            FromTableObjects(tables, joins);

            if (method)
                MainTable.Distinct();

            return this;
        }

        public Query From(string tableName, Query subQuery)
        {
            int current_row = 0, current_column = 0;
            MainTable.Clear();// = new DataTable();
            MainTable.Head = new ColumnDefination();
            dataTableSet = subQuery.dataTableSet;
            while (current_row < subQuery.dataTableSet.MaxSize())
            {
                current_column = 0;
                foreach (string query in select_element_arr)
                {
                    DataGiver(query.Trim(), current_row, current_row, current_column);
                    current_column++;
                }
                current_row++;
            }
            return this;
        }

        public Query From(string tables, JoinTable[] joins, Query subQuery)
        {
            return this;
        }

        public Query FetchRaw()
        {
            int current_row = 0, current_column = 0;
            MainTable.Clear();// = new DataTable();
            MainTable.Head = new ColumnDefination();

            while (current_row < 1)
            {
                current_column = 0;
                foreach (string query in select_element_arr)
                {
                    var _tmp_ = Parser.AliasGetter(query);
                    string[] query_val = (_tmp_.Length == 2) ? _tmp_ : new string[] { _tmp_[0], _tmp_[0] };

                    var logic = new Logistics.LogicExpressionEngine(query_val[0]);
                    logic.Solve();

                    if (!MainTable.Head.HasColumn(query_val[1]))
                        MainTable.DefinedColumn(new Column(query_val[1], logic.RawResult.GetType()));

                    if (MainTable.Size() <= current_row)
                        MainTable.AddRow(new Row(MainTable.Head.Count), true);

                    MainTable.SetCellValue(query_val[1], current_row, logic.RawResult);
                }
                current_row++;
            }

            return this;
        }

        private void DataGiver(string query, int fetch_row, int current_row, int current_column)
        {
            bool can_add = false;
            //this helps to keep alias names intact
            var _tmp_ = Parser.AliasGetter(query);
            string[] query_val = (_tmp_.Length == 2) ? _tmp_ : new string[] { _tmp_[0], _tmp_[0] };

            query_val[0] = query_val[0].Trim();
            query_val[1] = query_val[1].Trim();

            //working for alias VALUE (`)
            if (query_val[1].Trim().StartsWith("`") && query_val[1].Trim().EndsWith("`") && query_val[1].Count(x => x == '`') == 2)
            {
                query_val[1] = new Value(query_val[1]).Data.ToString();//.Substring(1, query_val.Length - 2);
            }

            string que_par = p.GetType(query[0]);
            object[] nxt_obj = Enumerable.Repeat("", 4).ToArray<object>();

            if (!(query_val[0].Trim().StartsWith("*") && query_val[0].Trim().Length == 1))
                nxt_obj = p.GetNext(0, query, que_par, p.STable);

            //val, nxt, anno, index ✔
            #region for isql collective select
            if (nxt_obj[2].ToString() == "[" && que_par == "@")
            {

                string MAIN_PARENT = query.Substring(0, query.IndexOf("[")).Trim();
                string[] q_val = Parser.QuerySeprator(query.Substring(query.IndexOf("[") + 1, query.LastIndexOf("]") - query.IndexOf("[") - 1));

                foreach (string que_data in q_val)
                {
                    var _tmp_1 = Parser.AliasGetter(que_data);
                    string[] datar = (_tmp_1.Length == 2) ? _tmp_1 : new string[] { _tmp_1[0], _tmp_1[0] };

                    //getting alias
                    // = (que_data.Contains(":")) ? que_data.Trim().Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries) : new string[] { que_data, MAIN_PARENT + "." + que_data };

                    //for alias with space ie. :`First Name`
                    if (datar[1].Trim().StartsWith("`") && datar[1].Trim().EndsWith("`") && datar[1].Count(x => x == '`') == 2)
                    {
                        datar[1] = datar[1].Remove(datar[1].Length - 1, 1);
                        datar[1] = datar[1].Remove(0, 1);
                    }

                    var lexp = new Logistics.LogicExpressionEngine(datar[0]);
                    List<object> obj = lexp.GetExpression().ToList<object>();


                    ///dqc = 'data query count'
                    for (int dqc = 0; dqc < obj.Count; dqc++)
                    {
                        if (obj[dqc].GetType() == typeof(string) && !obj[dqc].ToString().EndsWith("`") && !obj[dqc].ToString().StartsWith("`") && lexp.GetVariables().Contains(obj[dqc]) && !obj[dqc].ToString().StartsWith("@"))
                        {
                            obj[dqc] = MAIN_PARENT + "." + obj[dqc];
                        }
                    }

                    //Joing and keeping alias
                    string dataq = string.Join("", obj) + ":" + ((datar[0] == datar[1]) ? String.Join("", obj) : datar[1]);
                    //Console.WriteLine("Collctive=> " + dataq);
                    DataGiver(dataq, fetch_row, current_row, current_column);
                }
            }
            #endregion

            //✔️
            #region for all selected with no alias 
            else if (query_val[0].Trim().Length == 1 && query_val[0].Trim() == "*")
            {
                Details details = (fetch_row == -1) ? dataTableSet.GetDetails(new string[] { query_val[0].Trim() }) : dataTableSet.GetRowDetails(fetch_row, new string[] { query_val[0].Trim() });
                can_add = true;

                foreach (Detail detail in details)
                {
                    string mtable = (detail.Alias == "`") ? detail.Name + "." : detail.Alias + ".";
                    if (!mtable.StartsWith("@"))
                        mtable = "@" + mtable;

                    int dcount = 0;
                    foreach (string name in detail.ColumnNames)
                    {
                        if (!MainTable.Head.HasColumn(mtable + name) && (current_row == 0 || fetch_row == -1))
                        {
                            MainTable.DefinedColumn(new Column(mtable + name, (fetch_row == -1) ? typeof(string) : detail.DataTypes[dcount]));
                        }

                        if (current_row >= MainTable.Size())
                            MainTable.AddRow(new Row(MainTable.Head.Count), true);

                        else if (fetch_row == -1 && MainTable.Size() == 0)
                            MainTable.AddRow(new Row(MainTable.Head.Count), true);

                        if (!MainTable.Head.IsEmpty)
                            MainTable.SetCellValue(mtable + name, (fetch_row == -1) ? 0 : current_row,
                            Parser.DataConverter(MainTable.Head.GetColumn(mtable + name).DataType, (fetch_row == -1) ? "" : detail.RowDatas[dcount]));//fetch_row is dcount

                        dcount++;
                    }
                }
                can_add = false;
            }
            #endregion

            //✔️
            #region for all selected with alias
            else if (query_val[0].StartsWith("@") && query_val[0].EndsWith("*") && query_val[0].Count(x => x == '.') == 1 && nxt_obj[2].ToString() == ".")
            {
                Details details = (fetch_row == -1) ? dataTableSet.GetDetails(new string[] { query_val[0].Trim() }) : dataTableSet.GetRowDetails(fetch_row, new string[] { query_val[0].Trim() });

                can_add = true;
                foreach (Detail detail in details)
                {
                    string mtable = (detail.Alias == "`") ? detail.Name + "." : detail.Alias + ".";
                    if (!mtable.StartsWith("@"))
                        mtable = "@" + mtable;

                    int dcount = 0;
                    foreach (string name in detail.ColumnNames)
                    {
                        if (!MainTable.Head.HasColumn(mtable + name) && (current_row == 0 || fetch_row == -1))
                        {
                            MainTable.DefinedColumn(new Column(mtable + name, (fetch_row == -1) ? typeof(string) : detail.DataTypes[dcount]));
                        }

                        if (current_row >= MainTable.Size())
                            MainTable.AddRow(new Row(MainTable.Head.Count), true);

                        else if (fetch_row == -1 && MainTable.Size() == 0)
                            MainTable.AddRow(new Row(MainTable.Head.Count), true);

                        if (!MainTable.Head.IsEmpty)
                            MainTable.SetCellValue(mtable + name, (fetch_row == -1) ? 0 : current_row, Parser.DataConverter(MainTable.Head.GetColumn(mtable + name).DataType, (fetch_row == -1) ? "" : detail.RowDatas[dcount]));
                        //fetch_row is dcount

                        dcount++;
                    }
                }
                can_add = false;
            }
            #endregion

            //✔️
            #region for logic Operations 
            else if (query_val[0].Contains(">") || query_val[0].Contains("<") || query_val[0].Contains("<=") || query_val[0].Contains(">=") || query_val[0].Contains("<>")
            || query_val[0].Contains("!=") || query_val[0].Contains("==") || query_val[0].Contains("&&") || query_val[0].Contains("||") || query_val[0].Contains("!"))
            {

                var lexp = new Logistics.LogicExpressionEngine(query_val[0]);
                string[] variables = lexp.GetVariables();

                if (fetch_row > -1)
                {
                    object[] data_values = dataTableSet.GetRowValues(variables, fetch_row);
                    lexp.SetVariables(variables, data_values);
                    lexp.Solve();
                }

                dynamic ans = lexp.RawResult;

                if (!MainTable.Head.HasColumn(query_val[1].ToString()))
                {
                    MainTable.DefinedColumn(new Column(query_val[1], ans.GetType()));
                    can_add = true;
                }

                if (current_row >= MainTable.Size())
                    MainTable.AddRow(new Row(MainTable.Head.Count), true);

                else if (fetch_row == -1 && MainTable.Size() == 0)
                    MainTable.AddRow(new Row(MainTable.Head.Count), true);

                MainTable.SetCellValue(query_val[1], current_row,
                Parser.DataConverter(MainTable.Head.GetColumn(query_val[1]).DataType, ans));
            }
            #endregion

            //✔️
            #region for arithmetic Operations 
            else if ((query_val[0].Contains("+") || query_val[0].Contains("-") || query_val[0].Contains("*") || query_val[0].Contains("/") || query_val[0].Contains("%")
            || query_val[0].Contains("^")) && !query_val[0].Trim().StartsWith("*"))
            {
                var exp = new Logistics.ExpressionEngine(query_val[0]);

                if (fetch_row > -1)
                {
                    string[] variables = exp.GetVariables();
                    object[] data_values = dataTableSet.GetRowValues(variables, fetch_row);
                    exp.SetVariables(variables, data_values);
                    exp.Calculate();
                }

                dynamic ans = exp.RawResult;

                if (!MainTable.Head.HasColumn(query_val[1].ToString()))
                {
                    MainTable.DefinedColumn(new Column(query_val[1], ans.GetType()));
                    can_add = true;
                }

                if (current_row >= MainTable.Size())
                    MainTable.AddRow(new Row(MainTable.Head.Count), true);

                else if (fetch_row == -1 && MainTable.Size() == 0)
                    MainTable.AddRow(new Row(MainTable.Head.Count), true);

                if (ans.ToString().StartsWith("'") && ans.ToString().EndsWith("'"))
                    ans = ans.ToString().Substring(1, ans.ToString().LastIndexOf("'") - 1);

                MainTable.SetCellValue(query_val[1], (fetch_row == -1) ? 0 : current_row,
                Parser.DataConverter(MainTable.Head.GetColumn(query_val[1]).DataType, ans));
            }
            #endregion

            //✔️
            #region for simple names and numbers either single or multi
            else if (que_par == "@" || que_par == "name" || que_par == "number" || que_par == "(" || que_par == "value" || que_par == "nval")
            {
                var lexp = new Logistics.LogicExpressionEngine(query_val[0]);
                string[] variables = lexp.GetVariables();

                if (fetch_row > -1)
                {
                    object[] data_values = dataTableSet.GetRowValues(variables, fetch_row);
                    lexp.SetVariables(variables, data_values);
                    lexp.Solve();
                }

                dynamic ans = lexp.RawResult;

                if (!MainTable.Head.HasColumn(query_val[1].ToString()))
                {
                    MainTable.DefinedColumn(new Column(query_val[1], ans.GetType()));
                    can_add = true;
                }


                if (current_row >= MainTable.Size())
                    MainTable.AddRow(new Row(MainTable.Head.Count), true);

                else if (fetch_row == -1 && MainTable.Size() == 0)
                    MainTable.AddRow(new Row(MainTable.Head.Count), true);

                //Console.WriteLine("Input: " + string.Join(", ", ans));
                //Console.WriteLine("Type: " + ans.GetType());
                //Console.WriteLine("Out: " + string.Join(", ", Parser.DataConverter(MainTable.Head.GetColumn(query_val[1]).DataType, ans)));
                //Console.WriteLine("Type: " + Parser.DataConverter(MainTable.Head.GetColumn(query_val[1]).DataType, ans).GetType() + "\n");


                MainTable.SetCellValue(query_val[1], (fetch_row == -1) ? 0 : current_row,
                Parser.DataConverter(MainTable.Head.GetColumn(query_val[1]).DataType, ans));
            }
            #endregion

            else
                throw new ISqlArguementException($"Error: unknown arguement '{query_val[0]}'");

            //if(fetch_row == -1)
            //MainTable.RemoveRow(0);
        }

        private void PortQuery(object sender, Logistics.PortEvent e)
        {
            var dt = e.DTable;
            var log = new Logistics.LogicExpressionEngine();
            log.allow_port_func = true;
            log.RearrangeLogicTree(e.Data, ref e.Tree);
            Console.WriteLine(string.Join("\t", e.Data));
            if (e.Tree.Contains("PORT"))
            {

                PORT_START:
                int index = e.Tree.LastIndexOf("PORT");
                int end = e.Data.GetEnclose("(", ")", index + 2, 1);
                int arg_index = index + 2;

                List<dynamic> args = new List<dynamic>();
                List<string> args_anno = new List<string>();

                //for annotations
                args_anno.AddRange(e.Tree.GetRange(arg_index, end - arg_index));

                //for PORT args
                args.AddRange(e.Data.GetRange(arg_index, end - arg_index));

                string column = "";
                dynamic row = "";
                dynamic port_data = "";

                if (e.Data[index].ToUpper() == "PORT")
                {
                    //when only one arguement is passed
                    if (args.Count == 1)
                    {
                        if (Parser.IsStringType(args[0]))
                            column = args[0];

                        if (!dt.Head.HasColumn(column) && Parser.IsStringType(args[0]))
                            throw new ISqlColumnNotFoundException($"Error: column '{column}' could not be found");

                        //if it is a number, then row is returned	
                        if (column == "" && !Parser.IsStringType(args[0]))
                        {
                            int r = Parser.DataConverter(typeof(int), args[0]);
                            port_data = new Set(dt[r].GetCurrentValues());
                        }
                        else//if it is a string, then column is returned
                            port_data = new Set(dt[column].ToArray<object>());

                        e.Data.MyRemoveRange(index, end - index + 1);
                        e.Data.Insert(index, port_data);
                        e.Tree.MyRemoveRange(index, end - index);
                        Value value_con = new Value(port_data);
                        e.Tree.Insert(index, value_con.Anno);
                    }

                    //when full two arguements are passed
                    else if (args.Count == 3)
                    {
                        if (Parser.IsStringType(args[0]))
                            column = args[0];

                        row = Parser.DataConverter(typeof(int), args[2]);

                        if (!dt.Head.HasColumn(column) && Parser.IsStringType(args[0]))
                            throw new ISqlColumnNotFoundException($"Error: column '{column}' could not be found");

                        if (row >= dt.Size() && Parser.IsStringType(args[0]))
                            throw new ISqlArguementException($"Error: port index is out of range");

                        //if it is a number - number, row then cell value is returned
                        if (column == "" && !Parser.IsStringType(args[0]))
                        {
                            int r = Parser.DataConverter(typeof(int), args[0]);
                            port_data = dt[r][row].Value;
                        }
                        else//if it is a string - number, column then cell value is returned
                            port_data = dt.MapCell(column, row).Value;

                        e.Data.MyRemoveRange(index, end - index + 1);
                        e.Data.Insert(index, port_data);
                        e.Tree.MyRemoveRange(index, end - index);
                        Value value_con = new Value(port_data);
                        e.Tree.Insert(index, value_con.Anno);
                    }
                }

                else if (e.Data[index].ToUpper() == "SELF")
                {
                    if (args.Count == 1)
                    {
                        if (Parser.IsStringType(args[0]))
                            column = args[0];

                        if (column == "" && !Parser.IsStringType(args[0]))
                        {
                            int r = Parser.DataConverter(typeof(int), args[0]);
                            port_data = new Set(dataTableSet.GetEachRows(r));
                        }

                        else
                        {
                            var coldet = Parser.ColumnDetector(column);
                            port_data = new Set(dataTableSet.GetColumnValues(column)[coldet[1]].ToArray<object>());
                        }

                        e.Data.MyRemoveRange(index, end - index + 1);
                        e.Data.Insert(index, port_data);
                        e.Tree.MyRemoveRange(index, end - index);
                        Value value_con = new Value(port_data);
                        e.Tree.Insert(index, value_con.Anno);

                    }

                    else if (args.Count == 3)
                    {
                        if (Parser.IsStringType(args[0]))
                            column = args[0];

                        row = Parser.DataConverter(typeof(int), args[2]);

                        if (row >= dataTableSet.MaxSize() && Parser.IsStringType(args[0]))
                            throw new ISqlArguementException($"Error: port index is out of range");

                        if (column == "" && !Parser.IsStringType(args[0]))
                        {
                            int r = Parser.DataConverter(typeof(int), args[0]);
                            port_data = dataTableSet.GetEachRows(r)[row];
                        }

                        else
                        {
                            var coldet = Parser.ColumnDetector(column);
                            port_data = dataTableSet.GetColumnValues(column)[coldet[1]].ToArray<object>()[row];
                        }

                        e.Data.MyRemoveRange(index, end - index + 1);
                        e.Data.Insert(index, port_data);
                        e.Tree.MyRemoveRange(index, end - index);
                        Value value_con = new Value(port_data);
                        e.Tree.Insert(index, value_con.Anno);
                    }
                }

                if (e.Tree.Contains("PORT"))
                    goto PORT_START;
            }
        }

        /// <summary>
        /// to filter data fetch from table(s)
        /// </summary>
        /// <param name="expressions">filter expression</param>
        /// <returns>instance of the Query class for chaining</returns>
        public Query Where(string expressions)
        {
            if (usedCon.ConnectionState != 1)
                throw new ISqlConnectionNotFoundException($"Error: no connection found");

            var logic = new Logistics.LogicExpressionEngine(expressions);
            string[] variables = logic.GetVariables();
            
            MainTable.Clear();
            
            int tbrow = 0, nrow = 0;

            for (; nrow < dataTableSet.MaxSize() && dataTableSet.MaxSize() != 0;)
            {
                object[] data_val = dataTableSet.GetRowValues(variables, nrow);

                logic.SetVariables(variables, data_val);
                logic.Solve();

                if (logic.Result)
                {
                    int current_column = 0;
                    if (MainTable.Head == null)
                        MainTable.Head = new ColumnDefination();

                    foreach (string query in select_element_arr)
                    {
                        DataGiver(query, nrow, tbrow, current_column);
                        current_column++;
                    }

                    tbrow++;
                }
                nrow++;
            }

            if (method)
                MainTable.Distinct();

            return this;
        }

        public Query Where(string expression, Query subQuery)
        {
            string filter = expression;
            if (usedCon.ConnectionState != 1)
                throw new ISqlConnectionNotFoundException($"Error: no connection found");

            var logic = new Logistics.LogicExpressionEngine(filter, subQuery.MainTable, true);
            logic.Port_Event += PortQuery;

            string[] variables = logic.GetVariables();

            MainTable.Clear();

            int tbrow = 0, nrow = 0;

            while (nrow < dataTableSet.MaxSize() && dataTableSet.MaxSize() != 0)
            {
                subquery_row = nrow;
                object[] data_val = dataTableSet.GetRowValues(variables, nrow);

                logic.SetVariables(variables, data_val);
                logic.Solve();

                if (logic.Result)
                {
                    int current_column = 0;

                    if (MainTable.Head == null)
                        MainTable.Head = new ColumnDefination();

                    foreach (string query in select_element_arr)
                    {
                        DataGiver(query, nrow, tbrow, current_column);
                        current_column++;
                    }

                    tbrow++;
                }
                nrow++;
            }

            if (method)
                MainTable.Distinct();


            subquery_row = 0;
            return this;
        }

        public Query OrderBy(string[] name, Order order = Order.Asc)
        {
            if (usedCon.ConnectionState != 1)
                throw new ISqlConnectionNotFoundException($"Error: no connection found");

            if (MainTable != null || MainTable.Size() > 1)
            {
                var namesList = new List<string>();
                foreach (var col in name)
                {
                    if (!MainTable.Head.HasColumn(col))
                    {
                        var dt = dataTableSet.GetColumnValues(col);
                        MainTable.SetColumn(dt.Head.GetColumn(col), dt[col].ToCellArray());
                        namesList.Add(col);
                    }
                }

                if (order == Order.Asc)
                {
                    //if (MainTable.Size() > 1)
                    MainTable.OrderBy(name);
                }
                else if (order == Order.Des)
                {
                    //if (MainTable.Size() > 1)
                    MainTable.OrderByDes(name);
                }

                MainTable.RemoveColumns(namesList.ToArray());
            }

            if (method)
                MainTable.Distinct();

            return this;
        }


        

    }

    public enum Order : int
    {
        Asc, Des
    }
    
}
