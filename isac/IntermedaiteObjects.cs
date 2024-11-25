using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using System.Threading.Tasks;

namespace Isac
{
	//public class IntermedaiteObjects 
	//{         

	//}

	internal enum SelectQueryType
	{
		ALL,
		ALLALIAS,
		SINGLE,
		LOGIC,
		EXPRESSION,
		COLLECTIVE,
		NONE
	}

    internal enum SelectType
    {
        NONE, DISTINCT
    }

	internal interface ISelect
	{
        SelectQueryType GetBlockType();
        bool IsDone();
		int GetIndex();

        int GetFinishedLineIndex();
	}

	internal class SelectObject : ISelect, IEnumerable, IEnumerable<ISelect>
	{
		internal ISelect[] Blocks = new ISelect[0];
        internal bool IsDone()
        {
            return Blocks.All(x => x.IsDone() == true);
        }
        

        internal int Index = -1;
        internal SelectType SelectObjectType = SelectType.NONE;
        internal SelectQueryType BlockType = SelectQueryType.NONE;
        internal int QueryIndex = 0, JoinIndex = 0;


        public void Add(ISelect select)
		{

			if (Blocks.Length > 0)
			{
				List<ISelect> blocks = Blocks.ToList<ISelect>();
				blocks.Add(select);
				blocks.OrderBy(x => x.GetIndex());

				Blocks = new ISelect[blocks.Count];

				Array.Copy(blocks.ToArray<ISelect>(), Blocks, blocks.Count);
				return;
			}
			Blocks = new ISelect[1] { select };
		}

        public void AddBlocks(ISelect[] selects)
        {
            List<ISelect> blocks = Blocks.ToList<ISelect>();
            foreach (var item in selects)
            {
                blocks.Add(item);
            }
            Blocks = new ISelect[blocks.Count];
            Array.Copy(blocks.ToArray<ISelect>(), Blocks, blocks.Count);

        }
		
        public static SelectObject GenerateBlock(params ISelect[] iselects)
        {
            SelectObject sblock = new SelectObject();
            foreach (ISelect select in iselects)
            {
                sblock.Add(select);
            }

            return sblock;
        }

		public override string ToString()
		{
			string str = "";
			foreach(var s in Blocks)
				str += s.ToString() + "\n";
			return str;
		}

        public IEnumerator GetEnumerator()
        {
            return Blocks.GetEnumerator();
        }

        IEnumerator<ISelect> IEnumerable<ISelect>.GetEnumerator()
        {
            return ((IEnumerable<ISelect>)Blocks).GetEnumerator();
        }

        bool ISelect.IsDone() => Blocks.All(x => x.IsDone() == true);

        public int GetIndex() => Index;

        public int Dimension = 1;

        public SelectQueryType GetBlockType() => BlockType;

        public int GetFinishedLineIndex()
        {
            return Blocks.Min(x => x.GetFinishedLineIndex());
        }

        public void MakeQuery(Dictionary<int, List<Isql.QueryCommand.ITargets>> targets, Isql.Collections.DataTable table, Isql.QueryCommand.TableGroup tgroup, bool remove = false)
        {
            List<Isql.Collections.Cell> single_row_cells = new List<Isql.Collections.Cell>();
            foreach (var item in Blocks)
            {
                if(item.GetType() == typeof(SelectBlock))
                {
                    single_row_cells.AddRange(((SelectBlock)item).Fetch(targets[item.GetIndex()], tgroup, QueryIndex));
                }

                else if(item.GetType() == typeof(CollectiveSelectBlock) && !remove)
                {
                    single_row_cells.AddRange(((CollectiveSelectBlock)item).Fetch(targets[item.GetIndex()], table, tgroup, QueryIndex));
                }
            }

            if (!remove)
            {
                table.AddRow(new Isql.Collections.Row(single_row_cells.ToArray<Isql.Collections.Cell>()));
                QueryIndex++;
            }
            else if(remove)
            {
                if (!bool.Parse(single_row_cells[0].Value.ToString()))
                {
                    table.RemoveRow(JoinIndex);
                    return;
                }
                JoinIndex++;
            }
            

            if (tgroup.HasJoinTable)
            {
                var groupswithjoins = tgroup.Where(x => x.HasJoinTable == true);
                foreach (var grp in groupswithjoins)
                {
                    var tables = grp.Where(x => x.GetType() == typeof(Isql.QueryCommand.JoinTableInfo));
                    foreach (var tb in tables)
                    {
                        var tab = (Isql.QueryCommand.JoinTableInfo)tb;
                        tab.On.MakeQuery(tgroup.JoinSelectTargets, table, tgroup, true);
                        //tab.On; SelectObject
                        Console.WriteLine();
                    }
                }
            }

        }
    }

	internal class SelectBlock : ISelect
	{
		internal SelectQueryType BlockType = SelectQueryType.NONE;
		internal List<dynamic> Query = new List<dynamic>();
		internal Dictionary<string, object[]> QueryVariables = new Dictionary<string, object[]>();
		internal List<string> Tree = new List<string>();
		internal string QueryAlias = "";
		internal int Index = -1;
        internal List<int> ColumnIndex = new List<int>();
        internal int TargetQueryIndex = 0;
        internal int TableQueryIndex = 0;

        internal Isql.Collections.Cell[] Fetch(List<Isql.QueryCommand.ITargets> targets, Isql.QueryCommand.TableGroup tgroup, int index)
        {
            //await Task.Run(() =>
            //{
                TargetQueryIndex = index;
                TargetQueryIndex = index;
            if (BlockType == SelectQueryType.ALL || BlockType == SelectQueryType.ALLALIAS)
            {
                return FetchAllTargetData(targets, tgroup.Groups, index).ToCellArray();
            }

            else if (BlockType == SelectQueryType.SINGLE || BlockType == SelectQueryType.LOGIC)
            {
                Isql.Logistics.LogicExpressionEngine lexp = Isql.Logistics.LogicExpressionEngine.GenerateLogicExpressionEngine(this);
                var datas = FetchTargetData(targets, tgroup.Groups, QueryVariables.Keys.ToArray());
                lexp.SetVariables(datas.Keys.ToArray(), datas.Values.ToArray());
                lexp.Solve();
                return new Isql.Collections.Cell[] { new Isql.Collections.Cell(lexp.RawResult) };
            }

            else if (BlockType == SelectQueryType.EXPRESSION)
            {
                Isql.Logistics.ExpressionEngine exp = Isql.Logistics.ExpressionEngine.GenerateExpressionEngine(this);
                var datas = FetchTargetData(targets, tgroup.Groups, QueryVariables.Keys.ToArray());
                
                exp.SetVariables(datas.Keys.ToArray(), datas.Values.ToArray());
                exp.Calculate();

                return new Isql.Collections.Cell[] { new Isql.Collections.Cell(exp.RawResult) };
            }

            throw new Isql.ISqlException($"Error: invalid block");
        }

        internal Dictionary<string, dynamic> FetchTargetData(List<Isql.QueryCommand.ITargets> target, Isql.QueryCommand.Group[] groups, params string[] needed_datas)
        {
            Dictionary<string, dynamic> datas = new Dictionary<string, dynamic>();
            foreach (var t in target)
            {
                var tags = (Isql.QueryCommand.TargetObjects)t;
                string[] column_detector = Parser.ColumnDetector(tags.targetInfos.Keys.ToArray()[0]);
                var table = groups.Last(x => x.GroupName == tags.Name.ToLower())
                            .Last(x => (column_detector[0] == "alias")
                                ? x.Table.Alias.ToLower() == column_detector[1].ToLower()
                                : x.Table.Name.ToLower() == column_detector[1].ToLower()).Table;
                
                if (TargetQueryIndex < table.Size() && tags.Index < needed_datas.Length)
                    datas.Add(needed_datas[tags.Index], 
                        table[TargetQueryIndex].
                            GetCell(tags
                                .targetInfos["@" + string.Join(".", column_detector)]
                                .ToArray()
                                .Last())
                            .Value);
            }

            return datas;
        }

        internal List<Isql.Collections.Cell> FetchAllTargetData(List<Isql.QueryCommand.ITargets> target, Isql.QueryCommand.Group[] groups, int row)
        {
            List<Isql.Collections.Cell> cells = new List<Isql.Collections.Cell>();
            foreach (var t in target)
            {
                var tags = (Isql.QueryCommand.TargetObjects)t;
                string[] column_detector = Parser.ColumnDetector(tags.targetInfos.Keys.ToArray()[0]);

                cells.AddRange(groups.Last(x => x.GroupName == tags.Name.ToLower())
                            .Last(x => (column_detector[0] == "alias")
                                ? x.Table.Alias.ToLower() == column_detector[1].ToLower()
                                : x.Table.Name.ToLower() == column_detector[1].ToLower())
                                    .Table[row][tags.targetInfos["@" + string.Join(".", column_detector)].ToArray()]
                                    .ToCellArray());
            }

            return cells;
        }

        internal void AddParentName(string parent)
        {
            if (!parent.StartsWith("@"))
                throw new Isql.ISqlException($"Error: invalid parent name");

            Dictionary<string, object[]> new_variables = new Dictionary<string, object[]>();
            List<dynamic> new_query = new List<dynamic>();

            foreach (var item in QueryVariables.Keys)
            {
                if (item.StartsWith("@"))
                {
                    new_variables.Add(item, QueryVariables[item]);
                    continue;
                }
                new_variables.Add(parent + "." + item, QueryVariables[item]);
            }

            Query.ForEach(x =>
                {
                    if (x.GetType() == typeof(string) && QueryVariables.ContainsKey(x.ToString()) && !x.ToString().StartsWith("@"))
                        new_query.Add(parent + "." + x);

                    else
                        new_query.Add(x);
                });
            
            QueryVariables = new_variables;
            Query = new_query;
        }

        internal bool done = false;
        public bool IsDone() => done;
        public int GetIndex() => Index;

        public SelectQueryType GetBlockType() => BlockType;
        public override string ToString()
		{
			return "SelectBlock\n{\n" +
					$"\tBlockType: {BlockType}\n" +
					$"\tQuery: {string.Join(" ", Query)}\n" +
					$"\tTree: {string.Join(" ", Tree)}\n" +
					$"\tAlias: {QueryAlias}\n" +
					$"\tIndex: {Index}\n" +
					"}";
		}

        public int GetFinishedLineIndex()
        {
            return TargetQueryIndex;
        }
    }

	internal sealed class CollectiveSelectBlock : ISelect, IEnumerable, IEnumerable<SelectBlock>
	{
		internal SelectQueryType BlockType = SelectQueryType.NONE;
		internal string CollectiveName = "";
		public SelectBlock[] SBs = new SelectBlock[0];
		internal int Index = -1;
        public bool IsDone() => SBs.All(x => x.IsDone() == true);
        public int GetIndex() => Index;
        

        public void Add(SelectBlock sb)
		{

			if (SBs.Length > 0)
			{
				List<SelectBlock> _sb = SBs.ToList<SelectBlock>();
				_sb.Add(sb);
				_sb.OrderBy(x => x.Index);

				SBs = new SelectBlock[_sb.Count];

				Array.Copy(_sb.ToArray<SelectBlock>(), SBs, _sb.Count);
				return;
			}
			SBs = new SelectBlock[1] { sb };
		}
        public SelectQueryType GetBlockType() => BlockType;

        public ISelect[] GetBlocks()
        {
            return SBs;
        }

        internal Isql.Collections.Cell[] Fetch(List<Isql.QueryCommand.ITargets> targets, Isql.Collections.DataTable table, Isql.QueryCommand.TableGroup tgroup, int index)
        {
            List<Isql.Collections.Cell> cells = new List<Isql.Collections.Cell>();
            foreach (SelectBlock curr in SBs)
            {
                cells.AddRange(curr.Fetch(((Isql.QueryCommand.CollectiveTarget)targets[0]).CollectiveTargets[Index], tgroup, index));
            }
            return cells.ToCellArray();
        }

        public void PassName()
        {
            foreach (var block in SBs)
            {
                block.Index += Index;
                block.AddParentName(CollectiveName);
            }
        }

        public SelectBlock[] GiveBack()
        {
            return SBs;
        }

        public void QueryString(string[] elements)
		{
			var query = new Isql.QueryCommand.Query();
			query.ParallelSelectQuery(elements, this, false);
		}

		public override string ToString()
		{
			string sbs = "";
			foreach (var s in SBs)
				sbs += s.ToString() + "\n";
			return "CollectiveSelectBlock\n{\n" +
					$"\tCollectiveName: {CollectiveName}\n" +
					$"Index: {Index}\n" +
					$"=> {sbs}\n" +
					"}";
		}

		public IEnumerator<SelectBlock> GetEnumerator()
		{
			return ((IEnumerable<SelectBlock>)SBs).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<SelectBlock>)SBs).GetEnumerator();
		}

        public int GetFinishedLineIndex()
        {
            return SBs.Min(x => x.GetFinishedLineIndex());
        }
    }
}





