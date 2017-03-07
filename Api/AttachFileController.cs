using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.IO;
using Newtonsoft.Json;
using OPAS2Model;
using System.Configuration;

namespace OPAS2.Api
{
  public class AttachFileController : BaseApiController
  {
    private static string AttachFileStoredPath = 
      ConfigurationManager.AppSettings["OPAS_AttachFileStoredPath"];
    
    [HttpPost]
    [Route("api/AttachFile/Up/{docGuid}")]
    public async Task<HttpResponseMessage> Up(string docGuid)
    {
      if (!Request.Content.IsMimeMultipartContent())
      {
        return Request.CreateErrorResponse(
          HttpStatusCode.UnsupportedMediaType,
          "The request doesn't contain valid content!");
      }

      try
      {
        var provider = new MultipartMemoryStreamProvider();
        await Request.Content.ReadAsMultipartAsync(provider);
        foreach (var file in provider.Contents)
        {
          var dataStream = await file.ReadAsStreamAsync();
          var fileOriginName = file.Headers.ContentDisposition.FileName.
            Replace("\"", "");
          var OutputDirectory = System.Web.Hosting.HostingEnvironment
            .MapPath(AttachFileStoredPath);
          var attachFileGuid = Guid.NewGuid().ToString();
          var fileServerName = attachFileGuid + ".opas";

          if (!Directory.Exists(OutputDirectory))
          {
            Directory.CreateDirectory(OutputDirectory);
          }


          var filePath = Path.Combine(OutputDirectory, // Directory +
            fileServerName); // FileName
          using (var fileStream = File.Create(filePath))
          using (var reader = new StreamReader(dataStream))
          {
            dataStream.CopyTo(fileStream);
            fileStream.Flush();
          }

          using (var db = new OPAS2DbContext())
          {
            var attachFile = db.attachFiles.Create();
            attachFile.guid = attachFileGuid;
            attachFile.bizDocumentGuid = docGuid;
            attachFile.originalName = fileOriginName;
            attachFile.serverName = fileServerName;
            attachFile.serverPath = OutputDirectory;
            db.attachFiles.Add(attachFile);
            db.SaveChanges();
          }

          var response = Request.CreateResponse(HttpStatusCode.OK);
          response.Content = new StringContent("Successful upload", Encoding.UTF8, "text/plain");
          response.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(@"text/html");
          return response;
        }

        return Request.CreateResponse(HttpStatusCode.BadRequest,
          "file not found?");
      }
      catch (Exception e)
      {
        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
      }
    }

    [HttpPost]
    [Route("api/AttachFile/Remove/{docGuid}")]
    public IHttpActionResult Remove(string docGuid)
    {
      var result = Request.Content.ReadAsStringAsync().Result;
      dynamic fileObj = JsonConvert.DeserializeObject(result);
      var fileOriginName = (string)fileObj.fileName;
      if (!string.IsNullOrWhiteSpace(docGuid) && // legal bizdoc
        !string.IsNullOrWhiteSpace(fileOriginName)) // and attach file
      {
        using (var db = new OPAS2DbContext())
        {
          var attachFile = db.attachFiles.Where(
            att => att.bizDocumentGuid == docGuid && att.originalName == fileOriginName).FirstOrDefault();
          if (attachFile != null)
          {
            //db.attachFiles.Remove(attachFile);
            attachFile.isVisible = false;
            db.SaveChanges();
          }
        }
      }
      return Ok();
    }


    // GET api/<controller>
    public IEnumerable<string> Get()
    {
      return new string[] { "value1", "value2" };
    }

    // GET api/<controller>/5
    public string Get(int id)
    {
      return "value";
    }

    // POST api/<controller>
    public void Post([FromBody]string value)
    {
    }

    // PUT api/<controller>/5
    public void Put(int id, [FromBody]string value)
    {
    }

    // DELETE api/<controller>/5
    public void Delete(string id)
    {
    }
  }
}