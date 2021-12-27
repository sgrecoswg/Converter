using System;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace Converter.WebAPI.Controllers
{
    public class ExcelResult : System.Web.Http.IHttpActionResult
    {
        private readonly string _contentType;
        public DataSet CurrentDataSet { get; set; }
        public Properties DocumentProperties { get; private set; }
      

        public ExcelResult(DataSet ds)
        {
            CurrentDataSet = ds;
        }

        public ExcelResult(params DataTable[] tables)
        {
            var ds = new DataSet();
            foreach (var t in tables)
            {
                ds.Tables.Add(t);
            }

            CurrentDataSet = ds;
        }

        public ExcelResult(Properties props, params DataTable[] tables) :this(tables)
        {
            DocumentProperties = props;
        }

        public struct Properties
        {
            public string Author { get; internal set; }
            public string Category { get; internal set; }
            public string Company { get; internal set; }
            public string Keywords { get; internal set; }
            public string Name { get; internal set; }
            public string Subject { get; internal set; }
            public string Title { get; internal set; }
            public bool WrapText { get; internal set; }
            public bool Filtered { get; internal set; }
            public string Comments { get; internal set; }
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK) {
                    Content = new StreamContent(new MemoryStream(BuildExcelFile()))
                };

                var contentType = _contentType ?? "application/vdn.openxmlformats-officedocument.spreadsheet.sheet";
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { 
                    FileName = DocumentProperties.Name ?? $"JsonDownload_{DateTime.Now.Ticks}"
                };

                return Task.FromResult(response);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private byte[] BuildExcelFile()
        {
            try
            {
                using (var stream = new MemoryStream())
                using (var xlPackage = new ExcelPackage(stream))
                {
                    foreach (DataTable tbl in CurrentDataSet.Tables)
                    {
                        if (tbl.Columns.Count > 0 && tbl.Rows.Count > 0)
                        {
                            var worksheet = xlPackage.Workbook.Worksheets.Add(tbl.TableName);
                            worksheet.Cells.LoadFromDataTable(tbl, true);
                            int columnCount = tbl.Columns.Count;
                            for (int i = 0; i < columnCount; i++)
                            {
                                worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(1,26,122,161);
                                worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(1, 255, 255, 255);
                                if (tbl.Columns[i].DataType == typeof(DateTime))
                                {
                                    worksheet.Column(i + 1).Style.Numberformat.Format = "MM/dd/yyyy";
                                }

                                if (tbl.Columns[i].DataType == typeof(string))
                                {
                                    worksheet.Column(i + 1).Style.WrapText = DocumentProperties.WrapText;
                                }

                                if (DocumentProperties.Filtered)
                                {
                                    worksheet.Cells[1, 1, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column].AutoFilter = true;
                                }

                                //freeze top row?
                                //worksheet.View.FreezePanes(1, worksheet.Dimension.End.Column);
                            }
                            xlPackage.Workbook.Properties.Title = DocumentProperties.Title;
                            xlPackage.Workbook.Properties.Author = DocumentProperties.Author;
                            xlPackage.Workbook.Properties.Subject = DocumentProperties.Subject;
                            xlPackage.Workbook.Properties.Keywords = DocumentProperties.Keywords;
                            xlPackage.Workbook.Properties.Category = DocumentProperties.Category;
                            xlPackage.Workbook.Properties.Comments = DocumentProperties.Comments;
                            xlPackage.Workbook.Properties.Company = DocumentProperties.Company;
                            //xlPackage.Workbook.Properties.HyperlinkBase = this.;
                        }

                        /*Save To Disk
                         if(File.Exist(<name>))
                         {
                            File.Delete(<name>);
                         }

                        Stream s = File.Create(<name>);
                        xlPackage.SaveAs(s);
                        s.Close();
                     */

                        xlPackage.Save();
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }

    
}
