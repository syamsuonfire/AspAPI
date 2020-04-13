using API.Models;
using API.ViewModel;
using AspAPI.Report;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspAPI.Controllers
{
    public class DivisionController : Controller
    {
        readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:44375/API/")
        };

        // GET: Division
        public ActionResult Index()
        {
            return View(LoadDivision());
        }

        public JsonResult LoadDivision()
        {
            IEnumerable<DivisionVM> divisions = null;
            var responseTask = client.GetAsync("Division");
            responseTask.Wait();
            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readTask = result.Content.ReadAsAsync<IList<DivisionVM>>();
                readTask.Wait();
                divisions = readTask.Result;
            }
            else
            {
                divisions = Enumerable.Empty<DivisionVM>();
                ModelState.AddModelError(string.Empty, "server error, try after some time");
            }
            return new JsonResult { Data = divisions, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult InsertOrUpdate(Division division)
        {
            var myContent = JsonConvert.SerializeObject(division);
            var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            if (division.Id == 0)
            {
                var result = client.PostAsync("Division", byteContent).Result;
                return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            else
            {
                var result = client.PutAsync("Division/" + division.Id, byteContent).Result;
                return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        public async Task<JsonResult> GetById(int id)
        {
            HttpResponseMessage response = await client.GetAsync("Division");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsAsync<IList<DivisionVM>>();
                var div = data.FirstOrDefault(S => S.Id == id);
                var json = JsonConvert.SerializeObject(div, Formatting.None, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                return new JsonResult { Data = json, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            return Json("Internal Server Error");
        }

        public JsonResult Delete(int id)
        {
            var result = client.DeleteAsync("Division/" + id).Result;
            return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public async Task<ActionResult> PDF()
        {
            DivisionReport divisionReport = new DivisionReport();
            var readTask = await GetDivision();
            byte[] abytes = divisionReport.PrepareReport(readTask);
            return File(abytes, "application/pdf");
        }

        public async Task<List<DivisionVM>> GetDivision()
        {
            List<DivisionVM> divisions = new List<DivisionVM>();
            var responseTask = await client.GetAsync("Division");
            divisions = await responseTask.Content.ReadAsAsync<List<DivisionVM>>();
            return divisions;
        }

        public async Task<ActionResult> Excel()
        {

            var comlumHeadrs = new string[]
            {
                "Id",
                "Nama Division",
                "Nama Departemen",
                "Tanggal Ditambahkan",
                 "Tanggal Diperbaharui",
            };

            byte[] result;

            using (var package = new ExcelPackage())

            {
                // add a new worksheet to the empty workbook

                var worksheet = package.Workbook.Worksheets.Add("Current Division"); //Worksheet name
                using (var cells = worksheet.Cells[1, 1, 1, 5]) //(1,1) (1,5)
                {
                    cells.Style.Font.Bold = true;
                }

                //First add the headers
                for (var i = 0; i < comlumHeadrs.Count(); i++)
                {
                    worksheet.Cells[1, i + 1].Value = comlumHeadrs[i];
                }

                //Add values
                var j = 2;

                HttpResponseMessage response = await client.GetAsync("Division");
                if (response.IsSuccessStatusCode)
                {
                    var readTask = await response.Content.ReadAsAsync<IList<DivisionVM>>();
                    foreach (var division in readTask)
                    {
                        worksheet.Cells["A" + j].Value = division.Id;
                        worksheet.Cells["B" + j].Value = division.DivisionName;
                        worksheet.Cells["C" + j].Value = division.DepartmentName;
                        worksheet.Cells["D" + j].Value = division.CreateDate.ToString("MM/dd/yyyy");
                        worksheet.Cells["E" + j].Value = division.UpdateDate.ToString("MM/dd/yyyy");
                        j++;
                    }
                }
                result = package.GetAsByteArray();
            }
            return File(result, "application/ms-excel", $"Division.xlsx");
        }

        public async Task<ActionResult> CSV()
        {
            var comlumHeadrs = new string[]
            {
                "Id",
                "Nama Division",
                "Nama Departemen",
                "Tanggal Ditambahkan",
                 "Tanggal Diperbaharui",
            };

            var divisioncsv = new StringBuilder();

            HttpResponseMessage response = await client.GetAsync("Division");
            if (response.IsSuccessStatusCode)
            {
                var readTask = await response.Content.ReadAsAsync<IList<DivisionVM>>();

                var divisionRecords = (from division in readTask
                                         select new object[]
                                         {
                                            division.Id,
                                            $"\"{division.DivisionName}\"",
                                            $"\"{division.DepartmentName}\"",
                                            $"\"{division.CreateDate.ToString("MM/dd/yyyy")}\"",
                                            $"\"{division.UpdateDate.ToString("MM/dd/yyyy")}\"",
                                         }).ToList();

                // Build the file content
                divisionRecords.ForEach(line =>
                {
                    divisioncsv.AppendLine(string.Join(",", line));
                });
            }

            byte[] buffer = Encoding.ASCII.GetBytes($"{string.Join(",", comlumHeadrs)}\r\n{divisioncsv.ToString()}");
            return File(buffer, "text/csv", $"Division.csv");

        }

    }
}