using System;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Extensions.Logging;

namespace Converter.WebAPI.Controllers
{
    [RoutePrefix("")]
    [Route("[controller]")]
    public class ConverterController : ApiController
    {
        public enum OutputType { Excel };

        private readonly ILogger<ConverterController> _logger;

        public ConverterController(ILogger<ConverterController> logger)
        {
            _logger = logger;
        }


        [HttpPost,Route("{output:regex(excel|Excel|EXCEL|xls|xlsx)}")]
        public IHttpActionResult Post(string output, [FromBody] dynamic input)
        {
            try
            {
                /*
                 <ajaxcall>({
                 type:'POST',
                 data:[{"name":"qwerty","age":15,"value":"33"}],
                 content-type="application/json",
                 dataType="text",
                 success:function(d){
                    let url = '<server>/Json/Excel',
                    window.location = url
                }
                });
                 */     
                var _contentType = Request.Content.Headers.ContentType.MediaType;
                System.Data.DataTable _dataTable = new System.Data.DataTable();

                if (_contentType == "application/json")
                {
                    _dataTable = JsonParser.ToDataTable(input);
                    if (_dataTable.Columns.Count == 0 || _dataTable.Rows.Count == 0)
                    {
                        throw new HttpResponseException(new HttpResponseMessage() {
                            StatusCode = System.Net.HttpStatusCode.NoContent,
                            ReasonPhrase = "No rows or columns found.",
                            Content = new StringContent("No rows or columns found. Maybe the json did not parse correctly."),
                        });
                    }
                }
                else
                {
                    throw new HttpResponseException(System.Net.HttpStatusCode.UnsupportedMediaType);
                }

                return new ExcelResult(new ExcelResult.Properties(){ 
                    Author="",
                    Category="",Company="",Keywords="",Name=$"ExcelDownload{DateTime.Now.Ticks}.xlsx",
                    Subject="",Title="",WrapText=false,Filtered=true
                },_dataTable);

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                throw;
            }
        }
    }
}
