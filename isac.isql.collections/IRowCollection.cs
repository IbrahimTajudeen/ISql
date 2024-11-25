using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace Isac.Isql.Collections
{
    public interface IRowCollection : IEnumerable<Row>, IEnumerable
    {         
        bool HasRow(Row row);
        void AddRow(Row row);
        void AddRow(params object[] cellValues);
        void RemoveRow(int index);
		void RemoveIndex(params int[] index);
		void RemoveRange(int index, int count);
        Row FindRow(Func<Row, bool> predict);
        IEnumerable<Row> FindRows(Func<Row, bool> predict);
        IEnumerator<Row> GetEnumerator();
    }
}