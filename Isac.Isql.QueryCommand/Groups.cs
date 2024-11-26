using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using System.Threading.Tasks;

using System.IO;
using System.IO.Compression;

using Isac.Isql.Collections;

namespace Isac.Isql.QueryCommand
{
	internal class OnGroupLineFinished : EventArgs
	{
		internal string GroupName = "";
        internal int MinLineCompleted = 0;
    }
    
    internal class TableLineEvent : EventArgs
    {
        internal int Line = 0;
    }



	internal class TableInfo
	{
		private DataTable table = new DataTable();
		private int table_row = 0;
		private int table_row_repeate = 1;
		private int table_repeate = 0;
        
        public bool Full = false;

		public bool IsDone
		{
			get { return (TableRepeate * TableRowRepeate) == table.Size(); }
		}

		public DataTable Table
		{
			get { return (DataTable)table.Clone(); }
			set { table = value; }
		}
		public int TableRow
		{
			get { return table_row; }
			set { table_row = value; }
		}

		public int TableRowRepeate
		{
			get { return table_row_repeate; }
			set { table_row_repeate = value; }
		}

		public int TableRepeate
		{
			get { return table_repeate; }
			set { table_repeate = value; }
		}

		public void RepeatTable()
		{
			Table.RepeatSelf(TableRepeate - 1);
        }
	}

    internal class JoinTableInfo : TableInfo
    {
        public SelectObject on = new SelectObject();

        private TableGroup refObj;
        
        public SelectObject On
        {
            get { return on; }
        }
    }

	internal sealed class Group : IEnumerable, IEnumerable<TableInfo>
	{
		private string grp_name = "";

		internal event EventHandler<OnGroupLineFinished> GroupLineEvent;

		public void OnLineFinished(OnGroupLineFinished e)
		{
			GroupLineEvent?.Invoke(this, e);
		}

        public bool HasJoinTable = false;

        public int GroupLine
        {
            get { return infos.Min(x => x.Table.Size()); }
        }

		public bool IsDone
		{
			get { return infos.All(x => x.IsDone == true); }
		}

		object _lock = new object();
		string[] datas = new string[0];
		internal TableInfo[] infos = new TableInfo[0];

		public string GroupName
		{
            get { return grp_name; }
			set { grp_name = value; }
		}

        public Group() { }

		public Group(TableInfo tableInfo)
		{
			GroupName = tableInfo.Table.Name.ToLower();
			AddTableInfo(tableInfo); 
		}

		public void AddTableInfo(TableInfo tableInfo)
		{
			if (tableInfo.Table.Name.ToLower() != GroupName.ToLower() && GroupName != string.Empty)
				throw new ISqlException($"Error: invalid data. can not add item to group");

            if (infos.Any(x => x.Table.Alias.ToLower() == tableInfo.Table.Alias.ToLower() && tableInfo.Table.Alias != "`"))
                throw new ISqlException($"Error: cannot have same table name with same alias.\nTable Name: '{tableInfo.Table.Name}', Table Alias: '{tableInfo.Table.Alias}'");

            HasJoinTable = (HasJoinTable == false) ? tableInfo.GetType() == typeof(JoinTableInfo) : HasJoinTable;

			if (infos.Length > 0)
			{
				List<TableInfo> t_info = infos.ToList<TableInfo>();
				t_info.Add(tableInfo);

				t_info.OrderByDescending(x => x.TableRowRepeate);
				infos = new TableInfo[t_info.Count];

				Array.Copy(t_info.ToArray<TableInfo>(), infos, t_info.Count);

				return;
			}

			infos = new TableInfo[1] { tableInfo };
		}


        //fetch the table body
		public async Task GroupFetch(Stream stream, Connection con, int MaxSize)
		{
            using (StreamReader reader = new StreamReader(stream, con.CharSet))
            {
                datas = reader.ReadToEnd().Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                Task.Run(() =>
                 {
                     GroupWork(con, MaxSize);
                 });
            }
		}

		public void GroupWork(Connection con, int MaxSize)
		{
			int line = infos[0].TableRow;
			int count = 0;
			foreach (string red in datas)
			{
				if (count == line)
					break;

				Encryption encrypt = new Encryption();

				var cells = encrypt.DecryptAll(red,
								con.Key, con.IV,
								con.CharSet).ToCellArray();
				Row row = new Row(cells);

				Parallel.ForEach(infos, currentTableInfo =>
				{
                    if(!currentTableInfo.Full)
                    {
                        currentTableInfo.Table.AddRows(
                                    Enumerable.Repeat(row, currentTableInfo.TableRowRepeate)
                                    .ToArray());

                        if (currentTableInfo.Table.Size() == (currentTableInfo.TableRow * currentTableInfo.TableRowRepeate))
                        {
                            currentTableInfo.RepeatTable();
                        }

                        var ev = new OnGroupLineFinished { GroupName = GroupName, MinLineCompleted = GroupLine };

                        OnLineFinished(ev);
                    }

                    currentTableInfo.Full =  currentTableInfo.Table.Size() == MaxSize;
                    
                });

				
				count++;
			}
		}

		public IEnumerator<TableInfo> GetEnumerator()
		{
			return ((IEnumerable<TableInfo>)infos).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<TableInfo>)infos).GetEnumerator();
		}
	}

    internal sealed class TableGroup : IEnumerable, IEnumerable<Group>
    {

        internal event EventHandler<TableLineEvent> LineEvent;

        public void OnLineEvent(TableLineEvent e)
        {
            LineEvent?.Invoke(this, e);
        }

        public bool HasJoinTable = false;

        private Group[] groups = new Group[0];

        public int QueryLine = -1;

        public int Query
        {
            get { return QueryLine; }
            set
            {
                QueryLine = value;
                //Console.WriteLine("New Line: " + Query);
                OnLineEvent(new TableLineEvent { Line = QueryLine });
            }
        }

        public Dictionary<string, int> GroupMinLine = new Dictionary<string, int>();

        //for each key is for a select block
        public Dictionary<int, List<ITargets>> SelectTargets = new Dictionary<int, List<ITargets>>();
        public Dictionary<int, List<ITargets>> JoinSelectTargets = new Dictionary<int, List<ITargets>>();
        //for store table columns and type
        public Dictionary<string, Type> ColumnDefs = new Dictionary<string, Type>();

        public Group[] Groups
        {
            get { return groups; }
            set { groups = value; }
        }
        volatile bool isQueryDone = false;
        public bool IsDone
        {
            get
            {
                return groups.All(x => x.infos.All(y => y.Table.Size() == RealDimension));// && isQueryDone;
            }
        }

        private volatile int dimension = 0;
        private volatile int RealDimension = 0;
        public int Dimension
        {
            get { return dimension; }
            set { dimension = value; }
        }

        public TableGroup(int dimension)
        {
            this.dimension = dimension;
            this.RealDimension = dimension;
        }

		public void AddToGroup(string name, string alias, object[] infos, bool isJoin = false)
		{
			List<Group> tb_grp = groups.ToList<Group>();
            if(!isJoin)
            {

                int tbrow = (int)infos[2];
                TableInfo tableInfo = new TableInfo
                {
                    Table = new DataTable
                    {
                        Name = name,
                        Alias = alias
                    },

                    TableRow = tbrow,
                    TableRowRepeate = Dimension / tbrow,
                    TableRepeate = RealDimension / ((Dimension / tbrow) * tbrow)
                };

                Dimension = Dimension / tbrow;

                string[] cols = infos[0].ToString().Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                string[] dats = infos[1].ToString().Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);

                ColumnDefination cdefl = new ColumnDefination();

                int pKey = 0;
                foreach (var icol in cols)
                {
                    Column columns = new Column(icol, Parser.TypeConverter(dats[pKey]));
                    columns.ColumnIndex = pKey;
                    cdefl.AddColumn(columns);
                    pKey++;
                }

                tableInfo.Table.Defined(cdefl);

                if (tb_grp.Where(x => x.GroupName.ToLower() == name.ToLower()).Count() > 0)
                    tb_grp[tb_grp.FindIndex(x => x.GroupName.ToLower() == name.ToLower())].AddTableInfo(tableInfo);

                else
                {
                    var g = new Group(tableInfo);

                    g.GroupLineEvent += (s, b) =>
                    {
                        if (groups.Min(x => x.GroupLine) > QueryLine && QueryLine < RealDimension)
                            Query = groups.Min(x => x.GroupLine);
                    };

                    tb_grp.Add(g);
                }
                    

                groups = new Group[tb_grp.Count];

                Array.Copy(tb_grp.ToArray<Group>(), groups, tb_grp.Count);
            }

            else if (isJoin)
            {

                int tbrow = (int)infos[2];
                JoinTableInfo tableInfo = new JoinTableInfo
                {
                    Table = new DataTable
                    {
                        Name = name,
                        Alias = alias
                    },
                    on = (SelectObject)infos[3],
                    TableRow = tbrow,
                    TableRowRepeate = Dimension / tbrow,
                    TableRepeate = RealDimension / ((Dimension / tbrow) * tbrow)
                };

                Dimension = Dimension / tbrow;

                string[] cols = infos[0].ToString().Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                string[] dats = infos[1].ToString().Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);

                ColumnDefination cdefl = new ColumnDefination();

                int pKey = 0;
                foreach (var icol in cols)
                {
                    Column columns = new Column(icol, Parser.TypeConverter(dats[pKey]));
                    columns.ColumnIndex = pKey;
                    cdefl.AddColumn(columns);
                    pKey++;
                }

                tableInfo.Table.Defined(cdefl);

                if (tb_grp.Where(x => x.GroupName.ToLower() == name.ToLower()).Count() > 0)
                    tb_grp[tb_grp.FindIndex(x => x.GroupName.ToLower() == name.ToLower())].AddTableInfo(tableInfo);

                else
                    tb_grp.Add(new Group(tableInfo));

                groups = new Group[tb_grp.Count];

                Array.Copy(tb_grp.ToArray<Group>(), groups, tb_grp.Count);
            }

            if (GroupMinLine.ContainsKey(Groups.Last().GroupName))
                GroupMinLine[Groups.Last().GroupName] = Groups.Last().GroupLine;
            else
                GroupMinLine.Add(Groups.Last().GroupName, Groups.Last().GroupLine);

        }

		public static TableGroup GenerateGroup(List<string[]> normalNames, List<object[]> infos, int dimension, SelectObject selectObject)
		{
            var arr = selectObject.ToArray<ISelect>().OrderBy(x => x.GetIndex());

			if (normalNames.Count != infos.Count) throw new ISqlException($"Error: internal error");
			TableGroup tg = new TableGroup(dimension);

			int count = 0;
			foreach (var name in normalNames)
			{
				tg.AddToGroup(name[0], name[1], infos[count]);
				count++;
			}
            
            GetTargetVariables(tg.groups, selectObject, out tg.SelectTargets, out tg.ColumnDefs);
            if (tg.SelectTargets.Count() != selectObject.Count())
                throw new ISqlException($"Error: internal error");

            selectObject.Dimension = dimension;
            return tg;
		}


        public static TableGroup GenerateGroup(List<string[]> normalNames, List<object[]> normalHeads, List<string[]> joinNames, List<object[]> joinHeads, int dimension, SelectObject selectObject)
        {
            if (normalNames.Count != normalHeads.Count) throw new ISqlException($"Error: internal error");
            TableGroup tg = new TableGroup(dimension);

            int count = 0;
            foreach (var name in normalNames)
            {
                tg.AddToGroup(name[0], name[1], normalHeads[count], false);
                count++;
            }

            if(joinNames.Count > 0 && joinHeads.Count > 0)
            {

                if (joinNames.Count != joinHeads.Count) throw new ISqlException($"Error: internal error");
                int joinCount = 0;
                foreach (var name in joinNames)
                {
                    tg.AddToGroup(name[0], name[1], joinHeads[joinCount], true);
                    joinCount++;
                }
                tg.HasJoinTable = true;
            }
            //"@workers[_id + @w._name], @w[_name, _id], @a.age + @a.id"
            Dictionary<string, Type> cd = new Dictionary<string, Type>();

            GetTargetVariables(tg.groups, selectObject, out tg.SelectTargets, out tg.ColumnDefs);
            foreach (var grp in tg.Where(x => x.HasJoinTable == true))
            {
                foreach (var tinfo in grp.Where(x => x.GetType() == typeof(JoinTableInfo)))
                {
                    var tab = (Isql.QueryCommand.JoinTableInfo)tinfo;
                    GetTargetVariables(tg.groups, tab.On, out tg.JoinSelectTargets, out cd);
                }
            }
            if (tg.SelectTargets.Count() != selectObject.Count())
                throw new ISqlException($"Error: internal error");

            selectObject.Dimension = dimension;
            return tg;
        }

        public static void GetTargetCollectives(Group[] groups, SelectObject selectObject, out CollectiveTarget collectiveTarget, out Dictionary<string, Type> columns_Types)
        {
            collectiveTarget = new CollectiveTarget();
            columns_Types = new Dictionary<string, Type>();
            Dictionary<int, List<ITargets>> tags = new Dictionary<int, List<ITargets>>();
            //Dictionary<string, Type> columns = new Dictionary<string, Type>();
            GetTargetVariables(groups, selectObject, out tags, out columns_Types);
            collectiveTarget.CollectiveTargets = tags;
        }

        public static void GetTargetVariables(Group[] groups, SelectObject selectObject, out Dictionary<int, List<ITargets>> selectTargets, out Dictionary<string, Type> columns_Types)
        {
            selectTargets = new Dictionary<int, List<ITargets>>();
            columns_Types = new Dictionary<string, Type>();
            
            foreach (var iselects in selectObject)
            {
                int var_index = 0;
                if (iselects.GetType() == typeof(CollectiveSelectBlock))
                {
                    CollectiveTarget ct = new CollectiveTarget();
                    Dictionary<string, Type> types = new Dictionary<string, Type>();
                    GetTargetCollectives(groups, SelectObject.GenerateBlock(((CollectiveSelectBlock)iselects).GetBlocks()), out ct, out types);
                    
                    if (selectTargets.ContainsKey(((CollectiveSelectBlock)iselects).GetIndex()))
                        selectTargets[((CollectiveSelectBlock)iselects).GetIndex()].Add(ct);
                    else
                        selectTargets.Add(((CollectiveSelectBlock)iselects).GetIndex(), new List<ITargets> { ct });

                    columns_Types.AddRange(types);

                    continue;
                }

                bool grpfound = false;
                bool colfound = false;

                #region for ALL and ALLALIAS
                if (((SelectBlock)iselects).QueryVariables.Count == 0 && (((SelectBlock)iselects).BlockType == SelectQueryType.ALL || ((SelectBlock)iselects).BlockType == SelectQueryType.ALLALIAS))
                {
                    TargetObjects TarObj = new TargetObjects();
                    
                    string[] column_detector = Parser.ColumnDetector(((SelectBlock)iselects).Query[0]);
                    if (column_detector[1] != "*")
                        throw new ISqlException($"Error: query column detector error");

                    //for *
                    if (((SelectBlock)iselects).BlockType == SelectQueryType.ALL)
                    {
                        //Get groups with similar id
                        foreach (var grp in groups.Where(x => x.infos.Any(y => y.Table.Alias == "`")))
                        {
                            grpfound = true; int infoIndex = 0;
                            //Get tables with similar id
                            foreach (var grpTInfos in grp.infos.Where(x => x.Table.Alias == "`"))
                            {
                                colfound = true;
                                if (TarObj.targetInfos.ContainsKey("@alias." + grpTInfos.Table.Alias.ToLower()))
                                {
                                    TarObj.targetInfos["@alias." + grpTInfos.Table.Alias.ToLower()].AddRange(Enumerable.Range(0, grpTInfos.Table.ColumnCount));
                                    TarObj.targetInfos["@alias." + grpTInfos.Table.Alias.ToLower()] = TarObj.targetInfos["@alias." + grpTInfos.Table.Alias.ToLower()].Distinct().ToList<int>();
                                }

                                else
                                    TarObj.targetInfos.Add("@alias." + grpTInfos.Table.Alias.ToLower(), new List<int>(Enumerable.Range(0, grpTInfos.Table.ColumnCount)));
                                infoIndex++;

                                columns_Types.AddRangeOutIndexes(grpTInfos.Table.GetNameDataType("@" + grp.GroupName + ".", TarObj.targetInfos["@alias." + grpTInfos.Table.Alias.ToLower()].ToArray<int>()), ref ((SelectBlock)iselects).ColumnIndex);
                            }

                            if (!colfound)
                                throw new ISqlException($"Error: no column could be found in Table '{grp.GroupName}' Index: '{infoIndex}' with a defualt alias");

                            TarObj.Name = grp.GroupName;
                            TarObj.Index = var_index;

                            if (selectTargets.ContainsKey(((SelectBlock)iselects).GetIndex()))
                                selectTargets[((SelectBlock)iselects).GetIndex()].Add(TarObj);
                            else
                                selectTargets.Add(((SelectBlock)iselects).GetIndex(), new List<ITargets> { TarObj });

                            TarObj = new TargetObjects();
                            
                        }
                        if(!grpfound)
                            throw new ISqlException($"Error: no table could find target to columns with default alias");
                    }

                    //for @<alias_name>.*
                    if (((SelectBlock)iselects).BlockType == SelectQueryType.ALLALIAS)
                    {

                        //Get groups with similar id
                        foreach (var grp in groups.Where(x => x.infos.Any(y => y.Table.Alias.ToLower() == column_detector[0].ToLower() || y.Table.Name.ToLower() == column_detector[0])))
                        {
                            grpfound = true; int infoIndex = 0;
                            //Get tables with similar id
                            foreach (var grpTInfos in grp.infos.Where(x => (x.Table.Alias == "`") ? x.Table.Name.ToLower() == column_detector[0].ToString().ToLower() : x.Table.Alias.ToLower() == column_detector[0]))
                            {
                                colfound = true;
                                if (TarObj.targetInfos.ContainsKey((grpTInfos.Table.Alias == "`") ? "@name." + grpTInfos.Table.Name.ToLower() : "@alias." + grpTInfos.Table.Alias.ToLower()))
                                {
                                    TarObj.targetInfos[(grpTInfos.Table.Alias == "`") ? "@name." + grpTInfos.Table.Name.ToLower() : "@alias." + grpTInfos.Table.Alias.ToLower()].AddRange(Enumerable.Range(0, grpTInfos.Table.ColumnCount));
                                    TarObj.targetInfos[(grpTInfos.Table.Alias == "`") ? "@name." + grpTInfos.Table.Name.ToLower() : "@alias." + grpTInfos.Table.Alias.ToLower()] = TarObj.targetInfos[(grpTInfos.Table.Alias == "`") ? "@name." + grpTInfos.Table.Name.ToLower() : "@alias." + grpTInfos.Table.Alias.ToLower()].Distinct().ToList<int>();

                                }

                                else
                                    TarObj.targetInfos.Add((grpTInfos.Table.Alias == "`") ? "@name." + grpTInfos.Table.Name.ToLower() : "@alias." + grpTInfos.Table.Alias.ToLower(), new List<int>(Enumerable.Range(0, grpTInfos.Table.ColumnCount)));

                                columns_Types.AddRangeOutIndexes(grpTInfos.Table.GetNameDataType("@" + column_detector[0] + ".", TarObj.targetInfos[(grpTInfos.Table.Alias == "`") ? "@name." + grpTInfos.Table.Name.ToLower() : "@alias." + grpTInfos.Table.Alias.ToLower()].ToArray<int>()), ref ((SelectBlock)iselects).ColumnIndex);
                            }
                            if (!colfound)
                                throw new ISqlException($"Error: no column could be found in Table '{grp.GroupName}' Index: '{infoIndex}' with an Alias: '{column_detector[0]}'");

                            TarObj.Name = grp.GroupName;
                            TarObj.Index = var_index;

                            if (selectTargets.ContainsKey(((SelectBlock)iselects).GetIndex()))
                                selectTargets[((SelectBlock)iselects).GetIndex()].Add(TarObj);
                            else
                                selectTargets.Add(((SelectBlock)iselects).GetIndex(), new List<ITargets> { TarObj });

                            TarObj = new TargetObjects();
                        }
                        if (!grpfound)
                            throw new ISqlException($"Error: '{column_detector[0]}' could not find target to any table. Are you missing a table reference?");
                    }
                    continue;
                }
                #endregion

                int varIndex = 0;
                TargetObjects tObj = new TargetObjects();
                tObj.GoDirect = false;
                foreach (string varname in ((SelectBlock)iselects).QueryVariables.Keys)
                {
                    var column_detector = Parser.ColumnDetector(varname);
                    var lastgrp = new Group();
                    var lastinfo = new TableInfo();
                    //for simple <name>
                    if (column_detector[0] == "`")
                    {
                        try
                        {
                            //Get last group with similar id
                            lastgrp = groups.Where(x => x.infos.Any(y => y.Table.Alias == "`")).Last(x => x.infos.Any(y => y.Table.Head.HasColumn(column_detector[1])));
                        }
                        catch
                        {
                            throw new ISqlException($"Error: no table could find target to column: '{column_detector[1]}' with default alias");
                        }
                        
                        try
                        {
                            //Get last info with similar id
                            lastinfo = lastgrp.infos.Last(x => x.Table.ColumnNames().Any(y => y.ToLower() == column_detector[1].ToLower()) && x.Table.Alias.ToLower() == "`");
                        }
                        catch
                        {
                            throw new ISqlException($"Error: column: '{column_detector[1]}' could be found in the target Table: '{lastgrp.GroupName}' with a default alias");
                        }

                        //Add to target object
                        if (tObj.targetInfos.ContainsKey((lastinfo.Table.Alias == "`") ? "@name." + lastinfo.Table.Name.ToLower() : "@alias." + lastinfo.Table.Alias.ToLower()))
                            tObj.targetInfos[(lastinfo.Table.Alias == "`") ? "@name." + lastinfo.Table.Name.ToLower() : "@alias." + lastinfo.Table.Alias.ToLower()].Add(lastinfo.Table.ColumnNames().ToList().FindLastIndex(x => x.ToLower() == column_detector[1].ToLower()));

                        else
                            tObj.targetInfos.Add((lastinfo.Table.Alias == "`") ? "@name." + lastinfo.Table.Name.ToLower() : "@alias." + lastinfo.Table.Alias.ToLower(), new List<int> { lastinfo.Table.ColumnNames().ToList().FindLastIndex(x => x.ToLower() == column_detector[1].ToLower()) });

                        tObj.Name = lastgrp.GroupName;
                        tObj.Index = var_index;

                        if (selectTargets.ContainsKey(((SelectBlock)iselects).GetIndex()))
                            selectTargets[((SelectBlock)iselects).GetIndex()].Add(tObj);
                        else
                            selectTargets.Add(((SelectBlock)iselects).GetIndex(), new List<ITargets> { tObj });


                        tObj = new TargetObjects();
                    }

                    //for @<alias_name>.<name>
                    else if (column_detector[0] != "`")
                    {
                        try
                        {
                            //Get last group with similar id
                            lastgrp = groups.Where(x => x.GroupName == column_detector[0].ToLower() || x.infos.Any(y => y.Table.Alias.ToLower() == column_detector[0].ToLower())).Last(x => x.infos.Any(y => y.Table.Head.HasColumn(column_detector[1])));
                        }
                        catch
                        {
                            throw new ISqlException($"Error: '{column_detector[0]}' could not find target to any table. Are you missing a table reference?");
                        }
                        
                        try
                        {
                            //Get last info with similar id
                            lastinfo = lastgrp.infos.Where(x => (x.Table.Alias == "`") ? x.Table.Name.ToLower() == column_detector[0].ToString().ToLower() : x.Table.Alias.ToLower() == column_detector[0]).Last(x => x.Table.Head.HasColumn(column_detector[1]));
                        }
                        catch
                        {
                            throw new ISqlException($"Error: column: '{column_detector[1]}' could be found in the target Table: '{lastgrp.GroupName}' with an Alias: '{column_detector[0]}'");
                        }
                        
                        //Add to target object
                        if (tObj.targetInfos.ContainsKey((lastinfo.Table.Alias == "`") ? "@name." + lastinfo.Table.Name.ToLower() : "@alias." + lastinfo.Table.Alias.ToLower()))
                            tObj.targetInfos[(lastinfo.Table.Alias == "`") ? "@name." + lastinfo.Table.Name.ToLower() : "@alias." + lastinfo.Table.Alias.ToLower()].Add(lastinfo.Table.ColumnNames().ToList().FindLastIndex(x => x.ToLower() == column_detector[1].ToLower()));

                        else
                            tObj.targetInfos.Add((lastinfo.Table.Alias == "`") ? "@name." + lastinfo.Table.Name.ToLower() : "@alias." + lastinfo.Table.Alias.ToLower(), new List<int> { lastinfo.Table.ColumnNames().ToList().FindLastIndex(x => x.ToLower() == column_detector[1].ToLower()) });

                        tObj.Name = lastgrp.GroupName;
                        tObj.Index = var_index;

                        if (selectTargets.ContainsKey(((SelectBlock)iselects).GetIndex()))
                            selectTargets[((SelectBlock)iselects).GetIndex()].Add(tObj);
                        else
                            selectTargets.Add(((SelectBlock)iselects).GetIndex(), new List<ITargets> { tObj });

                        tObj = new TargetObjects();
                    }
                    var_index++;
                }

                columns_Types.Add(((SelectBlock)iselects).QueryAlias, typeof(None));
                ((SelectBlock)iselects).ColumnIndex.Add(columns_Types.Keys.ToList().LastIndexOf(((SelectBlock)iselects).QueryAlias));
            }
        }
        
        public async Task GroupQuery(Connection con, DataTable storedtable, SelectObject selectObject)
		{

            using (FileStream fs = new FileStream(con.Database, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (ZipArchive zipArchive = new ZipArchive(fs, ZipArchiveMode.Read))
				{
					foreach (var grp in groups)
					{
                        var stream = zipArchive.GetEntry(grp.GroupName.ToLower() + ".idb");
						
						if (stream == null)
							throw new ISqlException($"Error: table object not found");

                        await grp.GroupFetch(stream.Open(), con, RealDimension);
                    }
                }
			}
            
        }

		public void GroupQueryAsync(ZipArchive archive, Connection con)
		{
			Task.Run(() =>
			{
				Parallel.ForEach(groups, currentgroup =>
				{
					//currentgroup.GroupWork(archive, con);
				});
			});
		}

		public IEnumerator<Group> GetEnumerator()
		{
			return ((IEnumerable<Group>)groups).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Group>)groups).GetEnumerator();
		}
	}

    internal interface ITargets
    {

    }
    internal class TargetObjects : ITargets
    {
        public string Name;
        public bool GoDirect = true;
        public int Index = -1;
        public Dictionary<string, List<int>> targetInfos = new Dictionary<string, List<int>>();
    }

    // Not in use anymore
    internal class CollectiveTarget : ITargets
    {
        public Dictionary<int, List<ITargets>> CollectiveTargets = new Dictionary<int, List<ITargets>>();
    }

}





