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
    public class DepartmentController : Controller
    {
        readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:44375/API/")
        };

        // GET: Department
        public ActionResult Index()
        {
            return View(LoadDepartment());
        }

        public JsonResult LoadDepartment()
        {
            IEnumerable<Department> departments = null;
            var responseTask = client.GetAsync("Department");
            responseTask.Wait();
            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readTask = result.Content.ReadAsAsync<IList<Department>>();
                readTask.Wait();
                departments = readTask.Result;
            }
            else
            {
                departments = Enumerable.Empty<Department>();
                ModelState.AddModelError(string.Empty, "server error, try after some time");
            }
            return new JsonResult { Data = departments, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult InsertOrUpdate(Department department)
        {
            var myContent = JsonConvert.SerializeObject(department);
            var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            if (department.Id == 0) //insert
            {
                var result = client.PostAsync("Department", byteContent).Result;
                return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            else //update
            {
                var result = client.PutAsync("Department/" + department.Id, byteContent).Result;
                return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        public async Task<JsonResult> GetById(int id)
        {
            HttpResponseMessage response = await client.GetAsync("Department");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsAsync<IList<Department>>();
                var dept = data.FirstOrDefault(S => S.Id == id);
                var json = JsonConvert.SerializeObject(dept, Formatting.None, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                return new JsonResult { Data = json, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            return Json("Internal Server Error");
        }

        public JsonResult Delete(int id)
        {
            var result = client.DeleteAsync("Department/" + id).Result;
            return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        public async Task<ActionResult> PDF()
        {
            DepartmentReport departmentReport = new DepartmentReport();
            var readTask = await GetDepartment();
            byte[] abytes = departmentReport.PrepareReport(readTask);
            return File(abytes, "application/pdf");
        }

        public async Task<List<Department>> GetDepartment()
        {
            List<Department> departements = new List<Department>();
            var responseTask = await client.GetAsync("Department");
            departements = await responseTask.Content.ReadAsAsync<List<Department>>();
            return departements;
        }

        public async Task<ActionResult> Excel()
        {

            var comlumHeadrs = new string[]
            {
                "Id",
                "Nama Departemen",
                "Tanggal Ditambahkan",
                 "Tanggal Diperbaharui",
            };

            byte[] result;

            using (var package = new ExcelPackage())
            {
                // add a new worksheet to the empty workbook

                var worksheet = package.Workbook.Worksheets.Add("Current Department"); //Worksheet name
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


                HttpResponseMessage response = await client.GetAsync("Department");
                if (response.IsSuccessStatusCode)
                {
                    var readTask = await response.Content.ReadAsAsync<IList<Department>>();
                    foreach (var dept in readTask)
                    {
                        worksheet.Cells["A" + j].Value = dept.Id;
                        worksheet.Cells["B" + j].Value = dept.DepartmentName;
                        worksheet.Cells["C" + j].Value = dept.CreateDate.ToString("MM/dd/yyyy");
                        worksheet.Cells["D" + j].Value = dept.UpdateDate.ToString("MM/dd/yyyy");
                        j++;
                    }
                }
                    result = package.GetAsByteArray();
                }

                return File(result, "application/ms-excel", $"Department.xlsx");
            }


            public async Task<ActionResult> CSV()
            {
                var comlumHeadrs = new string[]
                {
                "Id",
                "Nama Departemen",
                "Tanggal Ditambahkan",
                 "Tanggal Diperbaharui",
                };

                var departmentcsv = new StringBuilder();

                 HttpResponseMessage response = await client.GetAsync("Department");
                if (response.IsSuccessStatusCode)
                {
                    var readTask = await response.Content.ReadAsAsync<IList<Department>>();

                    var departmentRecords = (from department in readTask
                                             select new object[]
                                             {
                                            department.Id,
                                            $"\"{department.DepartmentName}\"",
                                            $"\"{department.CreateDate.ToString("MM/dd/yyyy")}\"",
                                            $"\"{department.UpdateDate.ToString("MM/dd/yyyy")}\"",
                                             }).ToList();

                    // Build the file content
                    departmentRecords.ForEach(line =>
                    {
                        departmentcsv.AppendLine(string.Join(",", line));
                    });
                }

                byte[] buffer = Encoding.ASCII.GetBytes($"{string.Join(",", comlumHeadrs)}\r\n{departmentcsv.ToString()}");
                return File(buffer, "text/csv", $"Department.csv");

            }



        }
    }
