using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Converter.WebAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class JsonParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToCSV(string input)
        {

            return ToDataTable(input).AsCSV();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jArray"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static string ToCSV(JArray jArray, params string[] depth)
        {

            return ToDataTable(jArray).AsCSV();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="fieldProps"></param>
        public static void BuildColumns(System.Data.DataTable result, IEnumerable<JProperty> fieldProps)
        {
            result.Columns.Add("UID");
            foreach (var item in fieldProps)
            {
                var _key = item.Path.Split('.').Last();
                if (!result.Columns.Contains(_key))
                    result.Columns.Add(_key);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="token"></param>
        public static void BuildColumns(System.Data.DataTable result, JToken token)
        {
            var _fieldProps = token.WalkTokens().OfType<JProperty>().Where(x => x.Value.Type != JTokenType.Array);
            var _keys = _fieldProps.Where(x => x.Value.Children().Count() <= 0);
            BuildColumns(result,_keys);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static System.Data.DataTable ToDataTable(string input) 
        {
            try
            {
                //var _baseJson = JArray.Parse(input);
                return ToDataTable(JArray.Parse(input));
            }
            catch (Exception)
            {

                throw;
            }
        }

        private static DataTable ToDataTable(JArray jArray)
        {
            try
            {
                var _result = new DataTable();
                _result.TableName = $"Json_{DateTime.Now.ToString("yyyyDDMM")}";
                BuildColumns(_result, jArray);
                _result.PrimaryKey = new DataColumn[] { _result.Columns[0] };

                //build rows
                foreach (var token in jArray)
                {
                    List<DataRow> _rows = new List<DataRow>();
                    DataRow _row = _result.NewRow();
                    GetRootData(token, ref _row);

                    _rows = GetDataRows(token, ref _row,_rows);
                    foreach (var dr in _rows)
                    {
                        if (!_result.Rows.Contains(dr))
                        {
                           // var t = _rows.IndexOf(dr);
                            _result.Rows.Add(dr);
                        }
                    }
                }

                _result.PrimaryKey = null;
                _result.Columns.RemoveAt(0);
                return _result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private static void GetRootData(JToken token, ref DataRow row)
        {
            try
            {
                foreach (var d in token.OfType<JProperty>())
                {
                    if (d.Value.Type != JTokenType.Object ||
                        d.Value.Type != JTokenType.Property ||
                        d.Value.Type != JTokenType.Array)
                    {
                        var _key = d.Path.Split('.').Last();
                        row[_key] = d.Value;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="row"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        private static List<DataRow> GetDataRows(JToken tokens, ref DataRow row, List<DataRow> rows)
        {
            try
            {
                bool _hasValuesWithArrays = false;
                var props = tokens.OfType<JProperty>();
                if (props.Count()>0)
                {
                    props = tokens.Values().OfType<JProperty>();
                }

                AssignValueProperties(row,rows,props);

                AssignObjectProperties(row,props);

                var newRows = AssignFlatArrayValues(rows, row, props);

                if (newRows.Count()>0)
                {
                    return newRows;
                }

                //get ones that have arrays in them
                var objectsWitharrays = props.Where(x => x.Value.Type == JTokenType.Object)
                                             .Where(x => x.Value.OfType<JProperty>().Any(a => a.Value.Type == JTokenType.Array));
                _hasValuesWithArrays = objectsWitharrays.Count() > 0;
                foreach (var t in objectsWitharrays)
                {
                    var duplicataes = GetDataRows(t.Value,ref row,rows);
                }

                if (!_hasValuesWithArrays)
                {
                    if (!rows.Contains(row))
                    {
                        rows.Add(row);
                    }
                }

                return rows;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="row"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        private static List<DataRow> AssignFlatArrayValues(List<DataRow> rows, DataRow row, IEnumerable<JProperty> props)
        {
            var arrays = props.Where(x => x.Value.Type == JTokenType.Array && x.Value.Count() > 0)
                .Where(x => !x.Value.OfType<JProperty>().Any(a => a.Value.Type == JTokenType.Array))
                .Where(x => !x.Value.OfType<JProperty>().Any(a => a.Value.Type == JTokenType.Object));

            foreach (JToken item in arrays.Values())
            {
                foreach (var i in item)
                {
                    var newRow = row.Table.NewRow();
                    newRow.ItemArray = row.ItemArray as object[];
                    newRow["UID"] = Guid.NewGuid();
                    AssignObjectProperties(newRow,i.OfType<JProperty>());

                    foreach (var v in i.Values())
                    {
                        switch (v.Type)
                        {
                            case JTokenType.None:
                                break;
                            case JTokenType.Object:
                                break;
                            case JTokenType.Array:
                                break;
                            case JTokenType.Constructor:
                                break;
                            case JTokenType.Property:
                                break;
                            case JTokenType.Comment:
                                break;
                            case JTokenType.Integer:
                                break;
                            case JTokenType.Float:
                                break;
                            case JTokenType.String:
                                break;
                            case JTokenType.Boolean:
                                break;
                            case JTokenType.Null:
                                break;
                            case JTokenType.Undefined:
                                break;
                            case JTokenType.Date:
                                break;
                            case JTokenType.Raw:
                                break;
                            case JTokenType.Bytes:
                                break;
                            case JTokenType.Guid:
                                break;
                            case JTokenType.Uri:
                                break;
                            case JTokenType.TimeSpan:
                                break;
                            default:
                                newRow[v.Path.Split('.').Last()] = v;
                                break;
                        }
                    }
                    rows.Add(newRow);
                }
            }
            return rows;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newRow"></param>
        /// <param name="props"></param>
        private static void AssignObjectProperties(DataRow newRow, IEnumerable<JProperty> props)
        {
            var objs = props.Where(x => x.Value.Type == JTokenType.Object)
                .SelectMany(x=>x.Value.OfType<JProperty>().Where(a=>a.Value.Type != JTokenType.Array));

            foreach (var o in objs)
            {
                newRow[o.Path.Split('.').Last()] = o.Value;
            }
        }

       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="rows"></param>
        /// <param name="props"></param>
        private static void AssignValueProperties(DataRow row, List<DataRow> rows, IEnumerable<JProperty> props)
        {
            var values = props.Where(x=>x.Value.Type != JTokenType.Object && x.Value.Type != JTokenType.Array);
            foreach (var v in values)
            {
                row[v.Path.Split('.').Last()] = v;
            }
        }
    }
}
