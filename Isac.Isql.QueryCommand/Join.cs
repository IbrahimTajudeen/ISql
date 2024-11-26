using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isac.Isql.QueryCommand
{
    /// <summary>
    /// To include table rows based on a criteria
    /// </summary>
    public class JoinTable
    {
        private string table = "";
        private string alias = "";
        private SelectObject on;
        private JoinType join = JoinType.InnerJoin;

        internal string Table
        {
            get
            {
                return table;
            }
        }

        internal string Alias
        {
            get { return alias; }
        }

        internal SelectObject On
        {
            get
            {
                return on;
            }
        }

        internal JoinType Join
        {
            get
            {
                return join;
            }
        }

        internal Dictionary<int, List<object>> JoinTargets = new Dictionary<int, List<object>>();

        /// <summary>
        /// Instantiate JionTable class with required datas on the go.
        /// </summary>
        /// <param name="table">the name of the table to join</param>
        /// <param name="on">join on certain criteria</param>
        /// <param name="jointype">how to join the table</param>
        public JoinTable(string table, string on = "`true`", JoinType jointype = JoinType.InnerJoin)
        {
            var name_alias = Parser.AliasGetter(table);
            this.table = name_alias[0].ToLower();
            alias = (name_alias.Length == 2) ? name_alias[1].ToLower() : "`";
            this.on = new Query().ParallelSelectQuery(Parser.QuerySeprator(on), never_collective: true); join = jointype;
        }
    }

    /// <summary>
    /// Use to specify the type of join
    /// </summary>
    public enum JoinType
    {
        /// <summary>
        /// include table row based on certain criteria
        /// </summary>
        InnerJoin,
        /// <summary>
        /// include all Left Table row no matter criteria
        /// </summary>
        LeftJoin,
        /// <summary>
        /// include all Right Table row no matter criteria
        /// </summary>
        RightJoin
    }
}
