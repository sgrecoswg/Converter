using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Converter.WebAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public static class CommonExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IEnumerable<JToken> WalkTokens(this JToken node) {

            if (node == null)
                yield break;
            yield return node;
            foreach (var child in node.Children())
                foreach (var childNode in child.WalkTokens())
                    yield return childNode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool HasChildren(this JToken t) {

            return t.Children().Count() > 0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="props"></param>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static System.Data.DataRow AssignValues(this IEnumerable<JProperty> props, ref System.Data.DataRow dr)
        {

            foreach (var d in props)
            {
                if ((d.Value.Type == JTokenType.Object || 
                    d.Value.Type == JTokenType.Property || 
                    d.Value.Type == JTokenType.Array))
                {

                }
                else
                {
                    var _key = d.Path.Split('.').Last();
                    var v = d.Value;
                    dr[_key] = v;
                }
            }
            return dr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="_rows"></param>
        /// <returns></returns>
        public static bool IsIn(this System.Data.DataRow dr, List<System.Data.DataRow> _rows)
        {
            foreach (System.Data.DataRow r in _rows)
            {
                if (r.ItemArray == dr.ItemArray)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="delimeter"></param>
        /// <returns></returns>
        public static string AsCSV(this System.Data.DataTable dt, string delimeter = ",")
        {
            StringBuilder _sb = new StringBuilder();
            IEnumerable<string> _columnNames = dt.Columns.Cast<System.Data.DataColumn>().Select(c => c.ColumnName);
            _sb.AppendLine(string.Join(delimeter,_columnNames));
            foreach (System.Data.DataRow row in dt.Rows)
            {
                IEnumerable<string> _fields = row.ItemArray.Select(f => f.ToString());
                _sb.AppendLine(string.Join(delimeter, _fields));
            }
            return _sb.ToString();
        }
    }
}
