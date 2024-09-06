using CNCO.Unify.Communications.Http.Routing;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace UnifyTests.Communications.Http.Routing {
    internal class TestController : Controller {
        public static string FormatUrlParameters(string str, int id, DateTime dateTime, Guid guid) {
            var json = new JsonObject {
                { "string", str },
                { "id", id },
                { "date", dateTime },
                { "guid", guid }
            };
            return JsonSerializer.Serialize(json);
        }


        #region Route methods
        [HttpAll("all")]
        public void All() {
            Response.Send($"All-{Request.Verb}");
        }

        [HttpConnect]
        [HttpConnect("connect")]
        public void Connect() {
            Response.Send("Connect");
        }


        [HttpDelete]
        [HttpDelete("delete")]
        public void Delete() {
            Response.Send("Delete");
        }


        [HttpGet]
        [HttpGet("get")]
        public void Get() {
            Response.Send("Get");
        }


        [HttpHead]
        [HttpHead("head")]
        public void Head() {
            Response.Send("Head");
        }


        [HttpOptions]
        [HttpOptions("options")]
        public void Options() {
            Response.Send("Options");
        }


        [HttpPatch]
        [HttpPatch("patch")]
        public void Patch() {
            Response.Send("Patch");
        }


        [HttpPost]
        [HttpPost("post")]
        public void Post() {
            Response.Send("Post");
        }


        [HttpPut]
        [HttpPut("put")]
        public void Put() {
            Response.Send("Put");
        }


        [HttpTrace]
        [HttpTrace("trace")]
        public void Trace() {
            Response.Send("Trace");
        }


        #endregion

        #region Route parameters
        [HttpGet(":id:")]
        public void ColonStringRouteParameter(string id) {
            Response.Send(id);
        }

        [HttpGet("curly/{id}")]
        public void CurlyBraceStringRouteParameter(string id) {
            Response.Send(id);
        }

        #region Numbers
        [HttpGet("ushort/:id:")]
        public void ColonNumberRouteParameter(ushort id) {
            Response.Send(id.ToString());
        }

        [HttpGet("int/:id:")]
        [HttpGet("curly/int/{id}")]
        public void ColonNumberRouteParameter(int id) {
            Response.Send(id.ToString());
        }

        [HttpGet("decimal/:id:")]
        public void ColonNumberRouteParameter(decimal id) {
            Response.Send(id.ToString());
        }

        [HttpGet("double/:id:")]
        public void ColonNumberRouteParameter(double id) {
            Response.Send(id.ToString());
        }

        [HttpGet("float/:id:")]
        public void ColonNumberRouteParameter(float id) {
            Response.Send(id.ToString());
        }

        [HttpGet("long/:id:")]
        public void ColonNumberRouteParameter(long id) {
            Response.Send(id.ToString());
        }

        [HttpGet("bigInteger/:id:")]
        public void ColonNumberRouteParameter(BigInteger id) {
            Response.Send(id.ToString());
        }
        #endregion

        [HttpGet("date/:date:")]
        [HttpGet("curly/date/{date}")]
        public void ColonDateRouteParameter(DateTime dateTime) {
            Response.Send(dateTime.ToString("o"));
        }

        [HttpGet("guid/:guid:")]
        [HttpGet("curly/guid/{guid}")]
        public void ColonGuidRouteParameter(Guid id) {
            Response.Send(id.ToString());
        }


        // Multiple
        [HttpAll(":string:/:id:/:date:/:count:")]
        public void StringGuidDateTimeInt(string @string, Guid id, DateTime dateTime, int count) {
            Response.Send(FormatUrlParameters(@string, count, dateTime, id));
        }

        [HttpAll("curly/{string}/{id}/{date}/{count}")]
        public void CurlyBraceStringGuidDateTimeInt(string @string, Guid id, DateTime dateTime, int count) {
            Response.Send(FormatUrlParameters(@string, count, dateTime, id));
        }
        #endregion


        [HttpHead("/neverEnding")]
        public void NeverEnding() {
            while (true) {
                Thread.Sleep(100);
            }
        }
    }
}
