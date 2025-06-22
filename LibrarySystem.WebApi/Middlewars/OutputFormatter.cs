using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LibrarySystem.WebApi.Middlewars
{

    public class OutPutFormatter : TextOutputFormatter
    {
        /// <summary>
        /// 
        /// </summary>
        public OutPutFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="selectedEncoding"></param>
        /// <returns></returns>
        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var httpContext = context.HttpContext;
            var response = new ResponseDto
            {
                Data = context.Object,
                IsSuccess = context.HttpContext.Response.StatusCode == StatusCodes.Status200OK || context.HttpContext.Response.StatusCode == StatusCodes.Status201Created ? true : false,
            };
            var buffer = new StringBuilder();
            buffer.Append(JsonSerializer.Serialize(response, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            await httpContext.Response.WriteAsync(buffer.ToString(), selectedEncoding);

        }
    }
}
