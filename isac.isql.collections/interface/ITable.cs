using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isac.Isql.Collections
{
    interface ITable
    {
        void DataTypeConverter(string[] datatype_to_chechk, string[] orginal_types);
        
    }
}
