using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using Excel = Microsoft.Office.Interop.Excel;
using System.IO;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Web.Configuration;
using ClosedXML;
using ClosedXML.Excel;
using System.Configuration;

namespace RobiPosMapper.Areas.RobiAdmin.Controllers
{
    public class ReportsController : Controller
    {
        string sqlInsertMoLog = "INSERT INTO MonitoringOfficerActivityLog(MoId,LogDescription) VALUES (@MoId,@LogDescription)";


        //public ActionResult Index()
        //{

        //    DataTable dtData = new DataTable();
        //    dtData.Columns.Add("Test", typeof(System.String));
        //    DataRow dr = dtData.NewRow();
        //    dr[0] = "hi";
        //    dtData.Rows.Add(dr);
        //    string ReportName = "TestReport";
        //    try
        //    {
        //        Excel.Application MyExcel;
        //        Excel.Workbook MyWorkBook;
        //        Excel.Worksheet MyWorkSheet;

        //        object Missing = System.Reflection.Missing.Value; //a special reflection struct for unnecessary parameters replacement
        //        MyExcel = new Excel.Application();
        //        MyExcel.DisplayAlerts = false;

        //        //Add new workbook
        //        MyWorkBook = MyExcel.Workbooks.Add(Type.Missing);


        //        //Open existing WorkBook (Code is ok)
        //        //MyWorkBook = MyExcel.Workbooks.Open("D:\\output.xls", Missing, Missing, Missing, Missing, Missing, Missing, Missing, Missing, Missing, Missing, Missing, Missing, Missing, Missing);

        //        // creating new Excelsheet in workbook
        //        // MyWorkSheet = null;

        //        // see the excel sheet behind the program
        //        MyExcel.Visible = false;

        //        // get the reference of first sheet. By default its name is Sheet1.
        //        // store its reference to worksheet
        //        //MyWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)MyWorkBook.Sheets["Sheet1"];
        //        MyWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)MyWorkBook.ActiveSheet; //NB. code is ok

        //        // changing the name of active sheet
        //        //MyWorkSheet.Name = "Exported from gridview"; //NB. Code is OK. But rename takes more time to prepare the file.

        //        //string ReportName = "head line";//using in Excel titlebar, page headline, saving path and opening path
        //        int TotalColumns = dtData.Columns.Count;

        //        //-->headline
        //        //first row is intentionaly left blank.
        //        Excel.Range CellRange;
        //        // CellRange = (Excel.Range)MyWorkSheet.get_Range(MyWorkSheet.Cells[2, 1], MyWorkSheet.Cells[2, TotalColumns]);
        //        CellRange = (Excel.Range)MyWorkSheet.Range[MyWorkSheet.Cells[2, 1], MyWorkSheet.Cells[2, TotalColumns]];

        //        CellRange.Font.Bold = true;
        //        CellRange.Font.Size = 12;
        //        CellRange.HorizontalAlignment = Excel.Constants.xlCenter;
        //        CellRange.VerticalAlignment = Excel.Constants.xlCenter;
        //        CellRange.Merge(Missing);
        //        MyWorkSheet.Cells[2, 1] = ReportName;
        //        //<-- headline

        //        //--> column settings
        //        for (int i = 1; i < dtData.Columns.Count + 1; i++)
        //        {
        //            MyWorkSheet.Cells[4, i] = dtData.Columns[i - 1].ColumnName.ToString();
        //        }
        //        CellRange = (Excel.Range)MyWorkSheet.Range[MyWorkSheet.Cells[4, 1], MyWorkSheet.Cells[4, TotalColumns]];
        //        CellRange.Borders[Excel.XlBordersIndex.xlInsideHorizontal].Weight = 2;
        //        CellRange.Borders[Excel.XlBordersIndex.xlInsideVertical].Weight = 2;
        //        CellRange.Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = 2;
        //        CellRange.Borders[Excel.XlBordersIndex.xlEdgeLeft].Weight = 2;
        //        CellRange.Borders[Excel.XlBordersIndex.xlEdgeBottom].Weight = 2;
        //        CellRange.Borders[Excel.XlBordersIndex.xlEdgeRight].Weight = 2;
        //        CellRange.Font.Bold = true; CellRange.Font.Size = 10;
        //        CellRange.HorizontalAlignment = Excel.Constants.xlCenter;
        //        CellRange.VerticalAlignment = Excel.Constants.xlCenter;
        //        CellRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Silver);
        //        //<-- column settings


        //        //--> row data & settings
        //        for (int i = 0; i < dtData.Rows.Count; i++)
        //        {
        //            DataRow row = dtData.Rows[i];

        //            for (int j = 0; j < dtData.Columns.Count; j++)
        //            {
        //                MyWorkSheet.Cells[i + 5, j + 1] = row[j].ToString();
        //            }
        //        }

        //        //CellRange = (Excel.Range)MyWorkSheet.get_Range(MyWorkSheet.Cells[5, 1], MyWorkSheet.Cells[dtData.Rows.Count + 4, TotalColumns]);
        //        CellRange = (Excel.Range)MyWorkSheet.Range[MyWorkSheet.Cells[5, 1], MyWorkSheet.Cells[dtData.Rows.Count + 4, TotalColumns]];

        //        CellRange.Borders[Excel.XlBordersIndex.xlInsideHorizontal].Weight = 2;
        //        CellRange.Borders[Excel.XlBordersIndex.xlInsideVertical].Weight = 2;
        //        CellRange.Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = 2;
        //        CellRange.Borders[Excel.XlBordersIndex.xlEdgeLeft].Weight = 2;
        //        CellRange.Borders[Excel.XlBordersIndex.xlEdgeBottom].Weight = 2;
        //        CellRange.Borders[Excel.XlBordersIndex.xlEdgeRight].Weight = 2;
        //        CellRange.Font.Bold = false; CellRange.Font.Size = 10;
        //        CellRange.HorizontalAlignment = Excel.Constants.xlCenter;
        //        CellRange.VerticalAlignment = Excel.Constants.xlCenter;
        //        CellRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
        //        //<-- row data & settings

        //        // save the application
        //        // MyExcel.ActiveWorkbook.SaveCopyAs("D:\\output1.xls"); //Ok

        //        String path = Path.Combine(HttpContext.Server.MapPath("~/App_Data"), "Excels/1.xlsx");
        //        MyWorkBook.SaveAs(path, Missing, Missing, Missing, Missing, Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Missing, Missing, Missing, Missing, Missing);
        //        MyExcel.ActiveWorkbook.Saved = true;

        //        MyWorkBook.Close(false, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
        //        MyExcel.Workbooks.Close();

        //        //closing excel
        //        MyExcel.Quit();

        //        HttpContext.Response.Clear();
        //        HttpContext.Response.ClearContent();
        //        HttpContext.Response.ClearHeaders();
        //        HttpContext.Response.AddHeader("content-disposition", string.Format("attachment; filename={0}", "1.xlsx"));
        //        HttpContext.Response.ContentType = "application/excel";
        //        HttpContext.Response.WriteFile(path);
        //        HttpContext.Response.End();
        //        System.IO.File.Delete(path);

        //        return View();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public ActionResult RetailersFromAllScenario()
        {

            DataTable dtData = new DataTable();
            string fileName = Guid.NewGuid().ToString() ;
            String reportHeader = "Retailers From All Scenario";
            try
            {
                String conString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

                string sql = @"SELECT   Rsp.RspId, Rsp.RspName, 
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId) AS AllKindsOfRetailers, 
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And RetailerStatusId=3 and SurveyorActivityDateTime is NUll) AS NewlyCreatedByDsr,
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And RetailerStatusId=3 and SurveyorActivityDateTime is not NUll) AS NewlyCreatedBySurveyor,
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And RetailerStatusId=2 and IsReevaluated=1) AS UpdatedByDsrAndEvaluatedBySurveyor,
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And RetailerStatusId=2 and IsReevaluated is null) AS UpdatedByDsrButNotYetEvaluatedBySurveyor,
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And RetailerStatusId=2 and IsReevaluated=0) AS UpdatedBySurveyor,
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And DsrId is null) AS DsrNotTaggedWithRetailer,
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And RetailerStatusId=1 and DsrId is not null) AS DsrTaggedAndRetailerStatusEqualsOne

from RSP WHERE RspId IN (

77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87,
88,
89,
90,
91,
92,
93,
94,
95,
96,
97,
98,
99,
100,
101,
102,
103,
104,
105,
106,
107,
108,
109,
128,
129,
130,
131,
132,
133,
134,
135,
136,
137,
138,
139


) order by Rsp.RspName";

                using (SqlConnection connection = new SqlConnection(conString))
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(command))
                        {
                            connection.Open();
                            da.Fill(dtData); connection.Close();
                        }

                    }
                }

                
                //int TotalColumns = dtData.Columns.Count;

                
                String tableHtml="<table>";
                tableHtml += "<thead><tr>";
                //--> html for thead section
                foreach (DataColumn column in dtData.Columns)
                {
                    String combinedHeaderText = column.ColumnName.ToString();


                    string separatedColumnHeader = "";

                    foreach (char letter in combinedHeaderText)
                    {
                        if (Char.IsUpper(letter) && separatedColumnHeader.Length > 0)
                            separatedColumnHeader += " " + letter;
                        else
                            separatedColumnHeader += letter;
                    }
                    tableHtml += "<th>" + separatedColumnHeader + "</th>";
                }
                //<--- html for thead section

                tableHtml += "</tr></thead><tbody>";

                foreach (DataRow row in dtData.Rows)
                {
                    tableHtml += "<tr>";
                    String rspId = row[0].ToString();
                    foreach (DataColumn column in dtData.Columns)
                    {
                        if (column.Ordinal>1)
                        {
                            string hrefValue = String.Format("/RobiAdmin/Reports/{0}?rspid={1}", column.ColumnName.ToString(),rspId);
                            tableHtml += @"<td><a title=""Click here to download as excel."" href=" + hrefValue + ">" + row[column].ToString() + "</a></td>";
                        }
                        else
                        {
                            tableHtml += "<td>" + row[column].ToString() + "</td>";
                        }
                    }
                    tableHtml += "</tr>";
                }


                tableHtml += "</tbody><tfoot><tr><td colspan=2>Total</td>";
                //---> tfoot section
                for (int i = 2; i < dtData.Columns.Count; i++)
                {
                    Int32 value = Convert.ToInt32( dtData.Compute("Sum(" + dtData.Columns[i].ColumnName.ToString() + ")", ""));
                    tableHtml+= "<td>" + value.ToString() + "</td>";
                }
                //<--- tfoot section

                tableHtml += "</tr></tfoot></table>";
                
                ViewBag.Data = tableHtml;

                return View("ReportViewer");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public ActionResult DownloadRetailersFromAllScenario()
        {

            DataTable dtData = new DataTable();
            string fileName = Guid.NewGuid().ToString();
            String reportHeader = "Retailers From All Scenario";
            try
            {
                String conString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

                string sql = @"SELECT   Rsp.RspId, Rsp.RspName, 
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId) AS AllKindsOfRetailers, 
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And RetailerStatusId=3 and SurveyorActivityDateTime is NUll) AS NewlyCreatedByDsr,
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And RetailerStatusId=3 and SurveyorActivityDateTime is not NUll) AS NewlyCreatedBySurveyor,
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And RetailerStatusId=2 and IsReevaluated=1) AS UpdatedByDsrAndEvaluatedBySurveyor,
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And RetailerStatusId=2 and IsReevaluated is null) AS UpdatedByDsrButNotYetEvaluatedBySurveyor,
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And RetailerStatusId=2 and IsReevaluated=0) AS UpdatedBySurveyor,
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And DsrId is null) AS DsrNotTaggedWithRetailer,
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And RetailerStatusId=1 and DsrId is not null) AS DsrTaggedAndRetailerStatusEqualsOne

from RSP WHERE RspId IN (

77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87,
88,
89,
90,
91,
92,
93,
94,
95,
96,
97,
98,
99,
100,
101,
102,
103,
104,
105,
106,
107,
108,
109,
128,
129,
130,
131,
132,
133,
134,
135,
136,
137,
138,
139


) order by Rsp.RspName";

                using (SqlConnection connection = new SqlConnection(conString))
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(command))
                        {
                            connection.Open();
                            da.Fill(dtData); connection.Close();
                        }

                    }
                }

                //More details- http://closedxml.codeplex.com/
                var MyWorkBook = new XLWorkbook();
                var MyWorkSheet = MyWorkBook.Worksheets.Add("Sheet 1");

                int TotalColumns = dtData.Columns.Count;

                //-->headline
                //first row is intentionaly left blank.
                var headLine = MyWorkSheet.Range(MyWorkSheet.Cell(2, 1).Address, MyWorkSheet.Cell(2, TotalColumns).Address);
                headLine.Style.Font.Bold = true;
                headLine.Style.Font.FontSize = 15;
                headLine.Style.Font.FontColor = XLColor.White;
                headLine.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headLine.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headLine.Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.25);
                headLine.Style.Border.TopBorder = XLBorderStyleValues.Medium;
                headLine.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                headLine.Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                headLine.Style.Border.RightBorder = XLBorderStyleValues.Medium;

                headLine.Merge();
                headLine.Value = reportHeader;
                //<-- headline

                //--> column settings
                for (int i = 1; i < dtData.Columns.Count + 1; i++)
                {
                    String combinedHeaderText = dtData.Columns[i - 1].ColumnName.ToString();
                    string separatedColumnHeader = "";
                    foreach (char letter in combinedHeaderText)
                    {
                        if (Char.IsUpper(letter) && separatedColumnHeader.Length > 0)
                            separatedColumnHeader += " " + letter;
                        else
                            separatedColumnHeader += letter;
                    }

                    MyWorkSheet.Cell(4, i).Value = separatedColumnHeader;
                    MyWorkSheet.Cell(4, i).Style.Alignment.WrapText = true;
                }

                var columnRange = MyWorkSheet.Range(MyWorkSheet.Cell(4, 1).Address, MyWorkSheet.Cell(4, TotalColumns).Address);
                columnRange.Style.Font.Bold = true;
                columnRange.Style.Font.FontSize = 10;
                columnRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                columnRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                columnRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);

                columnRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                //<-- column settings

                //--> row data & settings
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    DataRow row = dtData.Rows[i];

                    for (int j = 0; j < dtData.Columns.Count; j++)
                    {
                        MyWorkSheet.Cell(i + 5, j + 1).Value = row[j].ToString();
                    }
                }

                var dataRowRange = MyWorkSheet.Range(MyWorkSheet.Cell(5, 1).Address, MyWorkSheet.Cell(dtData.Rows.Count + 4, TotalColumns).Address);
                dataRowRange.Style.Font.Bold = false;
                dataRowRange.Style.Font.FontSize = 10;
                dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                dataRowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                dataRowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(219, 229, 241);
                dataRowRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                //<-- row data & settings

                // Prepare the response
                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=\"" + reportHeader + ".xlsx\"");

                // Flush the workbook to the Response.OutputStream
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    MyWorkBook.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.OutputStream);
                    memoryStream.Close();
                }

                Response.End();

                return View("ReportViewer");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

//        public ActionResult UpdatedByDsrButNotYetEvaluatedBySurveyor()
//        {
//            Int32 rspId = Convert.ToInt32(Request.QueryString["rspid"]);
//            DataTable dtData = new DataTable();
//            string fileName = Guid.NewGuid().ToString();
//            String reportHeader = "Updated By Dsr But Not Yet Evaluated By Surveyor";
//            try
//            {
//                String conString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

//                string sql = @"SELECT dbo.Retailer.RegionId, dbo.Region.RegionName, dbo.Retailer.AreaId, dbo.Area.AreaName, dbo.RSP.RspId, dbo.RSP.RspName, 
//                      dbo.Retailer.RetailerId, dbo.Retailer.RetailerName, dbo.Retailer.Address, dbo.Retailer.DsrId, dbo.Retailer.RetailerStatusId, 
//                      dbo.RetailerStatus.ShortStatus AS RetailerStatusName, dbo.Retailer.PosCategoryId, dbo.PosCategory.PosCategoryName, dbo.Retailer.ThanaId, 
//                      dbo.Thana.ThanaName, dbo.Retailer.IsElPos, dbo.Retailer.IsSimPos, dbo.Retailer.IsScPos, dbo.Retailer.Latitude, dbo.Retailer.Longitude, dbo.Retailer.AccuracyLevel, 
//                      dbo.Retailer.VisitDayId, dbo.VisitDay.VisitDays, dbo.Retailer.DefaultPhotoName, dbo.Retailer.PosStructureId, dbo.PosStructure.PosStructureName, 
//                      dbo.Retailer.ShopSignageId, dbo.ShopSignage.ShopSignageName, dbo.Retailer.ShopTypeId, dbo.ShopType.ShopTypeName, dbo.Person.PersonName AS DsrName, 
//                      dbo.Retailer.IsReevaluated, dbo.ElMsisdn.ElMsisdn
//FROM         dbo.RSP INNER JOIN
//                      dbo.Retailer ON dbo.RSP.RspId = dbo.Retailer.RspId INNER JOIN
//                      dbo.PosCategory ON dbo.Retailer.PosCategoryId = dbo.PosCategory.PosCategoryId INNER JOIN
//                      dbo.Region ON dbo.Retailer.RegionId = dbo.Region.RegionId INNER JOIN
//                      dbo.Thana ON dbo.Retailer.ThanaId = dbo.Thana.ThanaId INNER JOIN
//                      dbo.Area ON dbo.Retailer.AreaId = dbo.Area.AreaId INNER JOIN
//                      dbo.VisitDay ON dbo.Retailer.VisitDayId = dbo.VisitDay.VisitDayId INNER JOIN
//                      dbo.PosStructure ON dbo.Retailer.PosStructureId = dbo.PosStructure.PosStructureId INNER JOIN
//                      dbo.ShopSignage ON dbo.Retailer.ShopSignageId = dbo.ShopSignage.ShopSignageId INNER JOIN
//                      dbo.ShopType ON dbo.Retailer.ShopTypeId = dbo.ShopType.ShopTypeId INNER JOIN
//                      dbo.RetailerStatus ON dbo.Retailer.RetailerStatusId = dbo.RetailerStatus.RetailerStatusId INNER JOIN
//                      dbo.Person ON dbo.Retailer.DsrId = dbo.Person.PersonId LEFT OUTER JOIN
//                      dbo.ElMsisdn ON dbo.Retailer.RetailerId = dbo.ElMsisdn.RetailerId
//WHERE     (dbo.RSP.RspId = "+ rspId + @") AND (dbo.Retailer.RetailerStatusId = 2) AND (dbo.Retailer.IsReevaluated IS NULL)
//ORDER BY dbo.Region.RegionName, dbo.Area.AreaName, dbo.RSP.RspName, dbo.Retailer.RetailerName";

//                using (SqlConnection connection = new SqlConnection(conString))
//                {
//                    using (SqlCommand command = new SqlCommand(sql, connection))
//                    {
//                        using (SqlDataAdapter da = new SqlDataAdapter(command))
//                        {
//                            connection.Open();
//                            da.Fill(dtData); connection.Close();
//                        }

//                    }
//                }

//                Excel.Application MyExcel;
//                Excel.Workbook MyWorkBook;
//                Excel.Worksheet MyWorkSheet;

//                object Missing = System.Reflection.Missing.Value; //a special reflection struct for unnecessary parameters replacement
//                MyExcel = new Excel.Application();
//                MyExcel.DisplayAlerts = false;

//                //Add new workbook
//                MyWorkBook = MyExcel.Workbooks.Add(Type.Missing);


//                //Open existing WorkBook (Code is ok)
//                //MyWorkBook = MyExcel.Workbooks.Open("D:\\output.xls", Missing, Missing, Missing, Missing, Missing, Missing, Missing, Missing, Missing, Missing, Missing, Missing, Missing, Missing);

//                // creating new Excelsheet in workbook
//                // MyWorkSheet = null;

//                // see the excel sheet behind the program
//                MyExcel.Visible = false;

//                // get the reference of first sheet. By default its name is Sheet1.
//                // store its reference to worksheet
//                //MyWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)MyWorkBook.Sheets["Sheet1"];
//                MyWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)MyWorkBook.ActiveSheet; //NB. code is ok

//                // changing the name of active sheet
//                //MyWorkSheet.Name = "Exported from gridview"; //NB. Code is OK. But rename takes more time to prepare the file.

//                //string ReportName = "head line";//using in Excel titlebar, page headline, saving path and opening path
//                int TotalColumns = dtData.Columns.Count;

//                //-->headline
//                //first row is intentionaly left blank.
//                Excel.Range CellRange;
//                // CellRange = (Excel.Range)MyWorkSheet.get_Range(MyWorkSheet.Cells[2, 1], MyWorkSheet.Cells[2, TotalColumns]);
//                CellRange = (Excel.Range)MyWorkSheet.Range[MyWorkSheet.Cells[2, 1], MyWorkSheet.Cells[2, TotalColumns]];

//                CellRange.Font.Bold = true;
//                CellRange.Font.Size = 12;
//                CellRange.HorizontalAlignment = Excel.Constants.xlCenter;
//                CellRange.VerticalAlignment = Excel.Constants.xlCenter;
//                CellRange.Merge(Missing);
//                MyWorkSheet.Cells[2, 1] = reportHeader; //ReportName;
//                //<-- headline

//                //--> column settings
//                for (int i = 1; i < dtData.Columns.Count + 1; i++)
//                {
//                    MyWorkSheet.Cells[4, i] = dtData.Columns[i - 1].ColumnName.ToString();
//                }
//                CellRange = (Excel.Range)MyWorkSheet.Range[MyWorkSheet.Cells[4, 1], MyWorkSheet.Cells[4, TotalColumns]];
//                CellRange.Borders[Excel.XlBordersIndex.xlInsideHorizontal].Weight = 2;
//                CellRange.Borders[Excel.XlBordersIndex.xlInsideVertical].Weight = 2;
//                CellRange.Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = 2;
//                CellRange.Borders[Excel.XlBordersIndex.xlEdgeLeft].Weight = 2;
//                CellRange.Borders[Excel.XlBordersIndex.xlEdgeBottom].Weight = 2;
//                CellRange.Borders[Excel.XlBordersIndex.xlEdgeRight].Weight = 2;
//                CellRange.Font.Bold = true; CellRange.Font.Size = 10;
//                CellRange.HorizontalAlignment = Excel.Constants.xlCenter;
//                CellRange.VerticalAlignment = Excel.Constants.xlCenter;
//                CellRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Silver);
//                //<-- column settings


//                //--> row data & settings
//                for (int i = 0; i < dtData.Rows.Count; i++)
//                {
//                    DataRow row = dtData.Rows[i];

//                    for (int j = 0; j < dtData.Columns.Count; j++)
//                    {
//                        MyWorkSheet.Cells[i + 5, j + 1] = row[j].ToString();
//                    }
//                }

//                //CellRange = (Excel.Range)MyWorkSheet.get_Range(MyWorkSheet.Cells[5, 1], MyWorkSheet.Cells[dtData.Rows.Count + 4, TotalColumns]);
//                CellRange = (Excel.Range)MyWorkSheet.Range[MyWorkSheet.Cells[5, 1], MyWorkSheet.Cells[dtData.Rows.Count + 4, TotalColumns]];

//                CellRange.Borders[Excel.XlBordersIndex.xlInsideHorizontal].Weight = 2;
//                CellRange.Borders[Excel.XlBordersIndex.xlInsideVertical].Weight = 2;
//                CellRange.Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = 2;
//                CellRange.Borders[Excel.XlBordersIndex.xlEdgeLeft].Weight = 2;
//                CellRange.Borders[Excel.XlBordersIndex.xlEdgeBottom].Weight = 2;
//                CellRange.Borders[Excel.XlBordersIndex.xlEdgeRight].Weight = 2;
//                CellRange.Font.Bold = false; CellRange.Font.Size = 10;
//                CellRange.HorizontalAlignment = Excel.Constants.xlCenter;
//                CellRange.VerticalAlignment = Excel.Constants.xlCenter;
//                CellRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
//                //<-- row data & settings

//                // save the application
//                // MyExcel.ActiveWorkbook.SaveCopyAs("D:\\output1.xls"); //Ok

//                String path = Path.Combine(HttpContext.Server.MapPath("~/App_Data"), "Excels/" + fileName + ".xlsx");
//                MyWorkBook.SaveAs(path, Missing, Missing, Missing, Missing, Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Missing, Missing, Missing, Missing, Missing);
//                MyExcel.ActiveWorkbook.Saved = true;

//                MyWorkBook.Close(false, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
//                MyExcel.Workbooks.Close();

//                //closing excel
//                MyExcel.Quit();

//                HttpContext.Response.Clear();
//                HttpContext.Response.ClearContent();
//                HttpContext.Response.ClearHeaders();
//                HttpContext.Response.AddHeader("content-disposition", string.Format("attachment; filename={0}", "" + fileName + ".xlsx"));
//                HttpContext.Response.ContentType = "application/excel";
//                HttpContext.Response.WriteFile(path);
//                HttpContext.Response.End();
//                System.IO.File.Delete(path);

//                return View("ReportViewer");
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//        }

        public ActionResult RetailersFromAllScenarioTest()
        {

            DataTable dtData = new DataTable();
            string fileName = Guid.NewGuid().ToString();
            String reportHeader = "Retailers From All Scenario";
            try
            {
                String conString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

                string sql = @"SELECT   Rsp.RspId, Rsp.RspName, 
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId) AS AllKindsOfRetailers, 
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And RetailerStatusId=3 and SurveyorActivityDateTime is NUll) AS NewlyCreatedByDsr,
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And RetailerStatusId=3 and SurveyorActivityDateTime is not NUll) AS NewlyCreatedBySurveyor,
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And RetailerStatusId=2 and IsReevaluated=1) AS UpdatedByDsrAndEvaluatedBySurveyor,
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And RetailerStatusId=2 and IsReevaluated is null) AS UpdatedByDsrButNotYetEvaluatedBySurveyor,
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And RetailerStatusId=2 and IsReevaluated=0) AS UpdatedBySurveyor,
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And DsrId is null) AS DsrNotTaggedWithRetailer,
(select Count(Retailer.RetailerId) from Retailer where Retailer.RspId=RSP.RspId And RetailerStatusId=1 and DsrId is not null) AS DsrTaggedAndRetailerStatusEqualsOne

from RSP WHERE RspId IN (77, 78, 79, 80, 81, 82, 87, 89, 90) order by Rsp.RspName";

                using (SqlConnection connection = new SqlConnection(conString))
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(command))
                        {
                            connection.Open();
                            da.Fill(dtData); connection.Close();
                        }

                    }
                }


                //More details- http://closedxml.codeplex.com/
                var MyWorkBook = new XLWorkbook();
                var MyWorkSheet = MyWorkBook.Worksheets.Add("Sheet 1");
               
                int TotalColumns = dtData.Columns.Count;

                //-->headline
                //first row is intentionaly left blank.
                var headLine = MyWorkSheet.Range(MyWorkSheet.Cell(2, 1).Address, MyWorkSheet.Cell(2, TotalColumns).Address);
                headLine.Style.Font.Bold = true;
                headLine.Style.Font.FontSize = 15;
                headLine.Style.Font.FontColor = XLColor.White;
                headLine.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headLine.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headLine.Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.25);
                headLine.Style.Border.TopBorder = XLBorderStyleValues.Medium;
                headLine.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                headLine.Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                headLine.Style.Border.RightBorder = XLBorderStyleValues.Medium;

                headLine.Merge();
                headLine.Value = reportHeader;
                //<-- headline

                //--> column settings
                for (int i = 1; i < dtData.Columns.Count + 1; i++)
                {
                    String combinedHeaderText = dtData.Columns[i - 1].ColumnName.ToString();
                    string separatedColumnHeader = "";
                    foreach (char letter in combinedHeaderText)
                    {
                        if (Char.IsUpper(letter) && separatedColumnHeader.Length > 0)
                            separatedColumnHeader += " " + letter;
                        else
                            separatedColumnHeader += letter;
                    }

                    MyWorkSheet.Cell(4, i).Value = separatedColumnHeader;
                    MyWorkSheet.Cell(4, i).Style.Alignment.WrapText = true;
                }

                var columnRange = MyWorkSheet.Range(MyWorkSheet.Cell(4, 1).Address, MyWorkSheet.Cell(4, TotalColumns).Address);
                columnRange.Style.Font.Bold = true;
                columnRange.Style.Font.FontSize = 10;
                columnRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                columnRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                columnRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223); 

                columnRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                //<-- column settings

                //--> row data & settings
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    DataRow row = dtData.Rows[i];

                    for (int j = 0; j < dtData.Columns.Count; j++)
                    {
                        MyWorkSheet.Cell(i + 5, j + 1).Value = row[j].ToString();
                    }
                }

                var dataRowRange = MyWorkSheet.Range(MyWorkSheet.Cell(5, 1).Address, MyWorkSheet.Cell(dtData.Rows.Count + 4, TotalColumns).Address);
                dataRowRange.Style.Font.Bold = false;
                dataRowRange.Style.Font.FontSize = 10;
                dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                dataRowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                dataRowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(219,229,241);
                dataRowRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                //<-- row data & settings

                // Prepare the response
                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=\""+ reportHeader +".xlsx\"");

                // Flush the workbook to the Response.OutputStream
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    MyWorkBook.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.OutputStream);
                    memoryStream.Close();
                }

                Response.End();

                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// This is only an addhoc method called only from URL.
        /// </summary>
        /// <returns></returns>
        public ActionResult SignedRspRetailerStats()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            try
            {
                string sqlSelectRsp = @"SELECT Region.RegionName, Area.AreaName, RSP.RspId, RSP.RspName
                                       FROM Area INNER JOIN RSP ON dbo.Area.AreaId = dbo.RSP.AreaId INNER JOIN Region ON dbo.Area.RegionId = dbo.Region.RegionId
                                       WHERE (RSP.RspId In (
77,
78,
79,
80,
81,
82,
83,
84,
85,
86,
87,
88,
89,
90,
91,
92,
93,
94,
95,
96,
97,
98,
99,
100,
101,
102,
103,
104,
105,
106,
107,
108,
109,
128,
129,
130,
131,
132,
133,
134,
135,
136,
137,
138,
139

))
                                       ORDER BY dbo.Region.RegionName, dbo.Area.AreaName, dbo.RSP.RspName";

                //Signed 18 RSP: 82, 77, 78, 79, 86, 96, 81, 80,83,84,85, 107, 108, 109, 89, 90, 100, 101, 103, 99, 102
                String sqlSelectRetailers = @"select RetailerId,RetailerStatusId, IsActive, RspId, IsReevaluated, SurveyorId from Retailer  WHERE (RspId In (

77,
78,
79,
80,
81,
82,
83,
84,
85,
86,
87,
88,
89,
90,
91,
92,
93,
94,
95,
96,
97,
98,
99,
100,
101,
102,
103,
104,
105,
106,
107,
108,
109,
128,
129,
130,
131,
132,
133,
134,
135,
136,
137,
138,
139


) AND (SurveyorId is not null))";
                DataTable dtRspList = new DataTable(); DataTable dtRetailerList = new DataTable();

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        using (SqlDataAdapter da = new SqlDataAdapter(command))
                        {
                            command.CommandText = sqlSelectRsp;
                            connection.Open();
                            da.Fill(dtRspList);// fill RSP table
                            command.CommandText = sqlSelectRetailers;
                            da.Fill(dtRetailerList); //fill Retailer table
                            connection.Close();
                        }
                    }
                }

                //Contains only todays work
                DataTable dtTodaysRspStatistics = new DataTable();
                dtTodaysRspStatistics.Columns.Add("RegionName", typeof(System.String));
                dtTodaysRspStatistics.Columns.Add("AreaName", typeof(System.String));
                dtTodaysRspStatistics.Columns.Add("RspId", typeof(System.Int32));
                dtTodaysRspStatistics.Columns.Add("RspName", typeof(System.String));
                dtTodaysRspStatistics.Columns.Add("Updated", typeof(System.Int32));
                dtTodaysRspStatistics.Columns.Add("New", typeof(System.Int32));
                dtTodaysRspStatistics.Columns.Add("Reevaluated", typeof(System.Int32));
                dtTodaysRspStatistics.Columns.Add("Total", typeof(System.Int32));

                

                DataRow row;
               
                //---> read every RSP
                foreach (DataRow rsp in dtRspList.Rows)
                {
                    Int32 rspId = Convert.ToInt32(rsp["RspId"]);
                    String rspName = rsp["RspName"].ToString();
                    String regionName = rsp["RegionName"].ToString();
                    String areaName = rsp["AreaName"].ToString();
                    DataRow[] isRecordsFound = dtRetailerList.Select("RspId=" + rspId + "");
                    if (isRecordsFound.Length > 0)
                    { 
                        row = AddAsRspRecord(dtRetailerList, dtTodaysRspStatistics, rspId, rspName, regionName, areaName);
                        dtTodaysRspStatistics.Rows.Add(row);
                    }
                    else
                    {
                        DataRow dr;
                        dr = dtTodaysRspStatistics.NewRow();
                        dr["RspId"] = rspId;
                        dr["RspName"] = rspName;
                        dr["RegionName"] = regionName;
                        dr["AreaName"] = areaName;
                        Int32 reevaluated =0;
                        dr["Reevaluated"] = reevaluated;
                        Int32 updated = 0;
                        dr["Updated"] = updated;
                        Int32 newRetailer = 0;
                        dr["New"] = newRetailer;
                        Int32 total = reevaluated + updated + newRetailer;
                        dr["Total"] = total;
                        dtTodaysRspStatistics.Rows.Add(dr);
                    }
                }
                //<--- read every RSP

                //More details- http://closedxml.codeplex.com/
                var MyWorkBook = new XLWorkbook();
                var MyWorkSheet = MyWorkBook.Worksheets.Add("Sheet 1");

                int TotalColumns = dtTodaysRspStatistics.Columns.Count;

                //-->headline
                //first row is intentionaly left blank.
                var headLine = MyWorkSheet.Range(MyWorkSheet.Cell(2, 1).Address, MyWorkSheet.Cell(2, TotalColumns).Address);
                headLine.Style.Font.Bold = true;
                headLine.Style.Font.FontSize = 15;
                headLine.Style.Font.FontColor = XLColor.White;
                headLine.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headLine.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headLine.Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.25);
                headLine.Style.Border.TopBorder = XLBorderStyleValues.Medium;
                headLine.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                headLine.Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                headLine.Style.Border.RightBorder = XLBorderStyleValues.Medium;

                String reportHeader = "Phase-2 RSP-POS Status";
                headLine.Merge();
                headLine.Value = reportHeader;
                //<-- headline

                //--> column settings
                for (int i = 1; i < dtTodaysRspStatistics.Columns.Count + 1; i++)
                {
                    String combinedHeaderText = dtTodaysRspStatistics.Columns[i - 1].ColumnName.ToString();
                    string separatedColumnHeader = "";
                    foreach (char letter in combinedHeaderText)
                    {
                        if (Char.IsUpper(letter) && separatedColumnHeader.Length > 0)
                            separatedColumnHeader += " " + letter;
                        else
                            separatedColumnHeader += letter;
                    }

                    MyWorkSheet.Cell(4, i).Value = separatedColumnHeader;
                    MyWorkSheet.Cell(4, i).Style.Alignment.WrapText = true;
                }

                var columnRange = MyWorkSheet.Range(MyWorkSheet.Cell(4, 1).Address, MyWorkSheet.Cell(4, TotalColumns).Address);
                columnRange.Style.Font.Bold = true;
                columnRange.Style.Font.FontSize = 10;
                columnRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                columnRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                columnRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);

                columnRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                //<-- column settings

                //--> row data & settings
                for (int i = 0; i < dtTodaysRspStatistics.Rows.Count; i++)
                {
                    DataRow thirRow = dtTodaysRspStatistics.Rows[i];

                    for (int j = 0; j < dtTodaysRspStatistics.Columns.Count; j++)
                    {
                        MyWorkSheet.Cell(i + 5, j + 1).Value = thirRow[j].ToString();
                    }
                }

                var dataRowRange = MyWorkSheet.Range(MyWorkSheet.Cell(5, 1).Address, MyWorkSheet.Cell(dtTodaysRspStatistics.Rows.Count + 4, TotalColumns).Address);
                dataRowRange.Style.Font.Bold = false;
                dataRowRange.Style.Font.FontSize = 10;
                dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                dataRowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                dataRowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(219, 229, 241);
                dataRowRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                //<-- row data & settings

                // Prepare the response
                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=\"" + reportHeader + ".xlsx\"");

                // Flush the workbook to the Response.OutputStream
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    MyWorkBook.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.OutputStream);
                    memoryStream.Close();
                }

                Response.End();

                return View();
            }
            catch (Exception ex)
            {
                var path = HttpContext.Server.MapPath("~/App_Data");
                //ExceptionLogger.CreateLog(ex, path);
                return View("ErrorContent");
            }
            return View();
        }

        private static DataRow AddAsRspRecord(DataTable dtRetailers, DataTable dtTodaysRspStatistics, Int32 rspId, String rspName, String regionName, String areaName)
        {
            DataRow row;
            row = dtTodaysRspStatistics.NewRow();
            row["RspId"] = rspId;
            row["RspName"] = rspName;
            row["RegionName"] = regionName;
            row["AreaName"] = areaName;
            Int32 reevaluated = Convert.ToInt32(dtRetailers.Compute("Count(RetailerId)", "RspId=" + rspId + " and RetailerStatusId=2 and IsReevaluated=1"));
            row["Reevaluated"] = reevaluated;
            Int32 updated = Convert.ToInt32(dtRetailers.Compute("Count(RetailerId)", "RspId=" + rspId + " and RetailerStatusId=2 and IsReevaluated=0"));
            row["Updated"] = updated;
            Int32 newRetailer = Convert.ToInt32(dtRetailers.Compute("Count(RetailerId)", "RspId=" + rspId + " and RetailerStatusId=3 AND IsReevaluated=0"));
            row["New"] = newRetailer;
            Int32 total = reevaluated + updated + newRetailer;
            row["Total"] = total;
            return row;
        }

        public ActionResult SrRetailerStats()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            int moid = Convert.ToInt32(Request.QueryString["MoId"]);
            try
            {
                string sqlSelectSurveyors = @"SELECT S.SurveyorId, S.SurveyorName, S.ContactNo
                                        FROM dbo.Surveyors AS S INNER JOIN
                                          dbo.MonitoringOfficerRegion AS MOR ON S.RegionId = MOR.RegionId
                                        WHERE  (S.SurveyorId > 0) AND (MOR.MoId = "+ moid + @")
                                        ORDER BY S.SurveyorId";


                String sqlSelectRetailers = @"SELECT R.RetailerId, R.RetailerStatusId, R.IsActive, R.SurveyorId
                                              FROM dbo.Retailer AS R INNER JOIN
                                                dbo.MonitoringOfficerRegion AS MOR ON R.RegionId = MOR.RegionId
                                              WHERE (R.SurveyorId IS NOT NULL) AND (MOR.MoId = "+ moid +")";




                DataTable dtSrList = new DataTable(); DataTable dtRetailerList = new DataTable();

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        using (SqlDataAdapter da = new SqlDataAdapter(command))
                        {
                            command.CommandText = sqlSelectSurveyors;
                            connection.Open();
                            da.Fill(dtSrList);// fill Sr table
                            command.CommandText = sqlSelectRetailers;
                            da.Fill(dtRetailerList); //fill Retailer table
                            connection.Close();
                        }

                        command.CommandText = sqlInsertMoLog;
                        command.Parameters.Clear();
                        command.Parameters.Add("@MoId", SqlDbType.Int).Value = Convert.ToInt32(moid);
                        command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = "Clicked on Surveyors Work Summary";
                        connection.Open(); command.ExecuteNonQuery(); connection.Close();
                    }
                }

                
                DataTable dtTodaysRspStatistics = new DataTable();
                dtTodaysRspStatistics.Columns.Add("SurveyorId", typeof(System.String));
                dtTodaysRspStatistics.Columns.Add("SurveyorName", typeof(System.String));
                dtTodaysRspStatistics.Columns.Add("ContactNo", typeof(System.String));
                dtTodaysRspStatistics.Columns.Add("Updated", typeof(System.Int32));
                dtTodaysRspStatistics.Columns.Add("New", typeof(System.Int32));
                dtTodaysRspStatistics.Columns.Add("Total", typeof(System.Int32));

                //---> read every RSP
                foreach (DataRow sr in dtSrList.Rows)
                {
                    Int32 srId = Convert.ToInt32(sr["SurveyorId"]);
                    String srName = sr["SurveyorName"].ToString();
                    String contactNo = sr["ContactNo"].ToString();
                  
                    DataRow[] isRecordsFound = dtRetailerList.Select("SurveyorId=" + srId + "");
                    if (isRecordsFound.Length > 0)
                    {
                        DataRow dr;
                        dr = dtTodaysRspStatistics.NewRow();
                        dr["SurveyorId"] = srId;
                        dr["SurveyorName"] = srName;
                        dr["ContactNo"] = contactNo;

                        Int32 updated = Convert.ToInt32(dtRetailerList.Compute("Count(RetailerId)", "SurveyorId=" + srId + " and RetailerStatusId=2"));
                        dr["Updated"] = updated;
                        Int32 newRetailer = Convert.ToInt32(dtRetailerList.Compute("Count(RetailerId)", "SurveyorId=" + srId + " and RetailerStatusId=3"));
                        dr["New"] = newRetailer;
                        Int32 total = updated + newRetailer;
                        dr["Total"] = total;
                        dtTodaysRspStatistics.Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr;
                        dr = dtTodaysRspStatistics.NewRow();
                        dr["SurveyorId"] = srId;
                        dr["SurveyorName"] = srName;
                        dr["ContactNo"] = contactNo;
                        Int32 updated = 0;
                        dr["Updated"] = updated;
                        Int32 newRetailer = 0;
                        dr["New"] = newRetailer;
                        Int32 total =  updated + newRetailer;
                        dr["Total"] = total;
                        dtTodaysRspStatistics.Rows.Add(dr);
                    }
                }
                //<--- read every RSP

                //More details- http://closedxml.codeplex.com/
                var MyWorkBook = new XLWorkbook();
                var MyWorkSheet = MyWorkBook.Worksheets.Add("Sheet 1");

                int TotalColumns = dtTodaysRspStatistics.Columns.Count;

                //-->headline
                //first row is intentionaly left blank.
                var headLine = MyWorkSheet.Range(MyWorkSheet.Cell(2, 1).Address, MyWorkSheet.Cell(2, TotalColumns).Address);
                headLine.Style.Font.Bold = true;
                headLine.Style.Font.FontSize = 15;
                headLine.Style.Font.FontColor = XLColor.White;
                headLine.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headLine.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headLine.Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.25);
                headLine.Style.Border.TopBorder = XLBorderStyleValues.Medium;
                headLine.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                headLine.Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                headLine.Style.Border.RightBorder = XLBorderStyleValues.Medium;

                String reportHeader = "Phase-III Surveyors Work Summary";
                headLine.Merge();
                headLine.Value = reportHeader;
                //<-- headline

                //--> column settings
                for (int i = 1; i < dtTodaysRspStatistics.Columns.Count + 1; i++)
                {
                    String combinedHeaderText = dtTodaysRspStatistics.Columns[i - 1].ColumnName.ToString();
                    string separatedColumnHeader = "";
                    foreach (char letter in combinedHeaderText)
                    {
                        if (Char.IsUpper(letter) && separatedColumnHeader.Length > 0)
                            separatedColumnHeader += " " + letter;
                        else
                            separatedColumnHeader += letter;
                    }

                    MyWorkSheet.Cell(4, i).Value = separatedColumnHeader;
                    MyWorkSheet.Cell(4, i).Style.Alignment.WrapText = true;
                }

                var columnRange = MyWorkSheet.Range(MyWorkSheet.Cell(4, 1).Address, MyWorkSheet.Cell(4, TotalColumns).Address);
                columnRange.Style.Font.Bold = true;
                columnRange.Style.Font.FontSize = 10;
                columnRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                columnRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                columnRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);

                columnRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                //<-- column settings

                //--> row data & settings
                for (int i = 0; i < dtTodaysRspStatistics.Rows.Count; i++)
                {
                    DataRow thirRow = dtTodaysRspStatistics.Rows[i];

                    for (int j = 0; j < dtTodaysRspStatistics.Columns.Count; j++)
                    {
                        MyWorkSheet.Cell(i + 5, j + 1).Value = thirRow[j].ToString();
                    }
                }

                var dataRowRange = MyWorkSheet.Range(MyWorkSheet.Cell(5, 1).Address, MyWorkSheet.Cell(dtTodaysRspStatistics.Rows.Count + 4, TotalColumns).Address);
                dataRowRange.Style.Font.Bold = false;
                dataRowRange.Style.Font.FontSize = 10;
                dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                dataRowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                dataRowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(219, 229, 241);
                dataRowRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                //<-- row data & settings

                // Prepare the response
                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=\"" + reportHeader + ".xlsx\"");

                // Flush the workbook to the Response.OutputStream
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    MyWorkBook.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.OutputStream);
                    memoryStream.Close();
                }

                Response.End();

                return View();
            }
            catch (Exception ex)
            {
                var path = HttpContext.Server.MapPath("~/App_Data");
                //ExceptionLogger.CreateLog(ex, path);
                return View("ErrorContent");
            }
            return View();
        }


        public ActionResult SelectedRspStatus()
        {

            String ConnectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            try
            {
                string sqlSelectRsp = @"SELECT Region.RegionName, Area.AreaName, RSP.RspId, RSP.RspName
                                       FROM Area INNER JOIN RSP ON dbo.Area.AreaId = dbo.RSP.AreaId INNER JOIN Region ON dbo.Area.RegionId = dbo.Region.RegionId
                                       WHERE (RSP.RspId In (
77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87,
88,
89,
90,
91,
92,
93,
94,
95,
96,
97,
98,
99,
100,
101,
102,
103,
104,
105,
106,
107,
108,
109,
128,
129,
130,
131,
132,
133,
134,
135,
136,
137,
138,
139

))
                                       ORDER BY dbo.RSP.RspName";


                DataTable dtRspList = new DataTable(); 
                

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        using (SqlDataAdapter da = new SqlDataAdapter(command))
                        {
                            command.CommandText = sqlSelectRsp;
                            connection.Open();
                            da.Fill(dtRspList);// fill RSP table
                            connection.Close();
                        }
                    }
                }

                ViewBag.RspList = dtRspList;
                return View("SelectedRspStatus");
            }
            catch (Exception ex)
            {
                var path = HttpContext.Server.MapPath("~/App_Data");
                //ExceptionLogger.CreateLog(ex, path);
                return View("ErrorContent");
            }
        }

//        [HttpPost]
//        public FileStreamResult SelectedRspStatus(FormCollection data)
//        {

//            var arrOfWorks = data["Rsps"].ToString();
//            var arrOfData = data["Test"].ToString();
//            arrOfData = arrOfData.Remove(0, 1);

//            String ConnectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
//            try
//            {
//                string sqlSelectRsp = @"SELECT Region.RegionName, Area.AreaName, RSP.RspId, RSP.RspName
//                                       FROM Area INNER JOIN RSP ON dbo.Area.AreaId = dbo.RSP.AreaId INNER JOIN Region ON dbo.Area.RegionId = dbo.Region.RegionId
//                                       WHERE (RSP.RspId In (" + arrOfData + ")) ORDER BY dbo.Region.RegionName, dbo.Area.AreaName, dbo.RSP.RspName";

//                //Signed 18 RSP: 82, 77, 78, 79, 86, 96, 81, 80,83,84,85, 107, 108, 109, 89, 90, 100, 101, 103, 99, 102
//                String sqlSelectRetailers = @"select RetailerId,RetailerStatusId, IsActive, RspId, IsReevaluated, SurveyorId from Retailer  WHERE (RspId In (" + arrOfData + ") AND (SurveyorId is not null))";
//                DataTable dtRspList = new DataTable(); DataTable dtRetailerList = new DataTable();

//                using (SqlConnection connection = new SqlConnection(ConnectionString))
//                {
//                    using (SqlCommand command = new SqlCommand())
//                    {
//                        command.Connection = connection;
//                        using (SqlDataAdapter da = new SqlDataAdapter(command))
//                        {
//                            command.CommandText = sqlSelectRsp;
//                            connection.Open();
//                            da.Fill(dtRspList);// fill RSP table
//                            command.CommandText = sqlSelectRetailers;
//                            da.Fill(dtRetailerList); //fill Retailer table
//                            connection.Close();
//                        }
//                    }
//                }

//                //Contains only todays work
//                DataTable dtTodaysRspStatistics = new DataTable();
//                dtTodaysRspStatistics.Columns.Add("RegionName", typeof(System.String));
//                dtTodaysRspStatistics.Columns.Add("AreaName", typeof(System.String));
//                dtTodaysRspStatistics.Columns.Add("RspId", typeof(System.Int32));
//                dtTodaysRspStatistics.Columns.Add("RspName", typeof(System.String));
//                dtTodaysRspStatistics.Columns.Add("Updated", typeof(System.Int32));
//                dtTodaysRspStatistics.Columns.Add("New", typeof(System.Int32));
//                dtTodaysRspStatistics.Columns.Add("Reevaluated", typeof(System.Int32));
//                dtTodaysRspStatistics.Columns.Add("Total", typeof(System.Int32));



//                DataRow row;

//                //---> read every RSP
//                foreach (DataRow rsp in dtRspList.Rows)
//                {
//                    Int32 rspId = Convert.ToInt32(rsp["RspId"]);
//                    String rspName = rsp["RspName"].ToString();
//                    String regionName = rsp["RegionName"].ToString();
//                    String areaName = rsp["AreaName"].ToString();
//                    DataRow[] isRecordsFound = dtRetailerList.Select("RspId=" + rspId + "");
//                    if (isRecordsFound.Length > 0)
//                    {
//                        row = AddAsRspRecord(dtRetailerList, dtTodaysRspStatistics, rspId, rspName, regionName, areaName);
//                        dtTodaysRspStatistics.Rows.Add(row);
//                    }
//                    else
//                    {
//                        DataRow dr;
//                        dr = dtTodaysRspStatistics.NewRow();
//                        dr["RspId"] = rspId;
//                        dr["RspName"] = rspName;
//                        dr["RegionName"] = regionName;
//                        dr["AreaName"] = areaName;
//                        Int32 reevaluated = 0;
//                        dr["Reevaluated"] = reevaluated;
//                        Int32 updated = 0;
//                        dr["Updated"] = updated;
//                        Int32 newRetailer = 0;
//                        dr["New"] = newRetailer;
//                        Int32 total = reevaluated + updated + newRetailer;
//                        dr["Total"] = total;
//                        dtTodaysRspStatistics.Rows.Add(dr);
//                    }
//                }
//                //<--- read every RSP

//                //More details- http://closedxml.codeplex.com/
//                var MyWorkBook = new XLWorkbook();
//                var MyWorkSheet = MyWorkBook.Worksheets.Add("Sheet 1");

//                int TotalColumns = dtTodaysRspStatistics.Columns.Count;

//                //-->headline
//                //first row is intentionaly left blank.
//                var headLine = MyWorkSheet.Range(MyWorkSheet.Cell(2, 1).Address, MyWorkSheet.Cell(2, TotalColumns).Address);
//                headLine.Style.Font.Bold = true;
//                headLine.Style.Font.FontSize = 15;
//                headLine.Style.Font.FontColor = XLColor.White;
//                headLine.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
//                headLine.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
//                headLine.Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.25);
//                headLine.Style.Border.TopBorder = XLBorderStyleValues.Medium;
//                headLine.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
//                headLine.Style.Border.LeftBorder = XLBorderStyleValues.Medium;
//                headLine.Style.Border.RightBorder = XLBorderStyleValues.Medium;

//                String reportHeader = "Phase-2 RSP-POS Status";
//                headLine.Merge();
//                headLine.Value = reportHeader;
//                //<-- headline

//                //--> column settings
//                for (int i = 1; i < dtTodaysRspStatistics.Columns.Count + 1; i++)
//                {
//                    String combinedHeaderText = dtTodaysRspStatistics.Columns[i - 1].ColumnName.ToString();
//                    string separatedColumnHeader = "";
//                    foreach (char letter in combinedHeaderText)
//                    {
//                        if (Char.IsUpper(letter) && separatedColumnHeader.Length > 0)
//                            separatedColumnHeader += " " + letter;
//                        else
//                            separatedColumnHeader += letter;
//                    }

//                    MyWorkSheet.Cell(4, i).Value = separatedColumnHeader;
//                    MyWorkSheet.Cell(4, i).Style.Alignment.WrapText = true;
//                }

//                var columnRange = MyWorkSheet.Range(MyWorkSheet.Cell(4, 1).Address, MyWorkSheet.Cell(4, TotalColumns).Address);
//                columnRange.Style.Font.Bold = true;
//                columnRange.Style.Font.FontSize = 10;
//                columnRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
//                columnRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
//                columnRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);

//                columnRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
//                columnRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
//                columnRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
//                columnRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
//                //<-- column settings

//                //--> row data & settings
//                for (int i = 0; i < dtTodaysRspStatistics.Rows.Count; i++)
//                {
//                    DataRow thirRow = dtTodaysRspStatistics.Rows[i];

//                    for (int j = 0; j < dtTodaysRspStatistics.Columns.Count; j++)
//                    {
//                        MyWorkSheet.Cell(i + 5, j + 1).Value = thirRow[j].ToString();
//                    }
//                }

//                var dataRowRange = MyWorkSheet.Range(MyWorkSheet.Cell(5, 1).Address, MyWorkSheet.Cell(dtTodaysRspStatistics.Rows.Count + 4, TotalColumns).Address);
//                dataRowRange.Style.Font.Bold = false;
//                dataRowRange.Style.Font.FontSize = 10;
//                dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
//                dataRowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
//                dataRowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(219, 229, 241);
//                dataRowRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
//                dataRowRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
//                dataRowRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
//                dataRowRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
//                //<-- row data & settings

//                // Prepare the response
//                Response.Clear();
//                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
//                Response.AddHeader("content-disposition", "attachment;filename=\"" + reportHeader + ".xlsx\"");

//                // Flush the workbook to the Response.OutputStream
//                using (MemoryStream memoryStream = new MemoryStream())
//                {
//                    MyWorkBook.SaveAs(memoryStream);
//                    memoryStream.WriteTo(Response.OutputStream);
//                    memoryStream.Close();
//                }

//                Response.End();

//                //  return View("SelectedRspStatus");
//            }
//            catch (Exception ex)
//            {
//                var path = HttpContext.Server.MapPath("~/App_Data");
//                //ExceptionLogger.CreateLog(ex, path);
//                //   return View("ErrorContent");
//            }
//        }

        [HttpPost]
        public JsonResult SelectedRspStatus(FormCollection data)
        {

            var arrOfWorks = data["Rsps"].ToString();
            var arrOfData = data["Test"].ToString();
            arrOfData = arrOfData.Remove(0, 1);

            String ConnectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            try
            {
                string sqlSelectRsp = @"SELECT Region.RegionName, Area.AreaName, RSP.RspId, RSP.RspName
                                       FROM Area INNER JOIN RSP ON dbo.Area.AreaId = dbo.RSP.AreaId INNER JOIN Region ON dbo.Area.RegionId = dbo.Region.RegionId
                                       WHERE (RSP.RspId In (" + arrOfData + ")) ORDER BY dbo.Region.RegionName, dbo.Area.AreaName, dbo.RSP.RspName";

                //Signed 18 RSP: 82, 77, 78, 79, 86, 96, 81, 80,83,84,85, 107, 108, 109, 89, 90, 100, 101, 103, 99, 102
                String sqlSelectRetailers = @"select RetailerId,RetailerStatusId, IsActive, RspId, IsReevaluated, SurveyorId from Retailer  WHERE (RspId In (" + arrOfData + ") AND (SurveyorId is not null))";
                DataTable dtRspList = new DataTable(); DataTable dtRetailerList = new DataTable();

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        using (SqlDataAdapter da = new SqlDataAdapter(command))
                        {
                            command.CommandText = sqlSelectRsp;
                            connection.Open();
                            da.Fill(dtRspList);// fill RSP table
                            command.CommandText = sqlSelectRetailers;
                            da.Fill(dtRetailerList); //fill Retailer table
                            connection.Close();
                        }
                    }
                }

                //Contains only todays work
                DataTable dtTodaysRspStatistics = new DataTable();
                dtTodaysRspStatistics.Columns.Add("RegionName", typeof(System.String));
                dtTodaysRspStatistics.Columns.Add("AreaName", typeof(System.String));
                dtTodaysRspStatistics.Columns.Add("RspId", typeof(System.Int32));
                dtTodaysRspStatistics.Columns.Add("RspName", typeof(System.String));
                dtTodaysRspStatistics.Columns.Add("Updated", typeof(System.Int32));
                dtTodaysRspStatistics.Columns.Add("New", typeof(System.Int32));
                dtTodaysRspStatistics.Columns.Add("Reevaluated", typeof(System.Int32));
                dtTodaysRspStatistics.Columns.Add("Total", typeof(System.Int32));

                DataRow row;
                //---> read every RSP
                foreach (DataRow rsp in dtRspList.Rows)
                {
                    Int32 rspId = Convert.ToInt32(rsp["RspId"]);
                    String rspName = rsp["RspName"].ToString();
                    String regionName = rsp["RegionName"].ToString();
                    String areaName = rsp["AreaName"].ToString();
                    DataRow[] isRecordsFound = dtRetailerList.Select("RspId=" + rspId + "");
                    if (isRecordsFound.Length > 0)
                    {
                        row = AddAsRspRecord(dtRetailerList, dtTodaysRspStatistics, rspId, rspName, regionName, areaName);
                        dtTodaysRspStatistics.Rows.Add(row);
                    }
                    else
                    {
                        DataRow dr;
                        dr = dtTodaysRspStatistics.NewRow();
                        dr["RspId"] = rspId;
                        dr["RspName"] = rspName;
                        dr["RegionName"] = regionName;
                        dr["AreaName"] = areaName;
                        Int32 reevaluated = 0;
                        dr["Reevaluated"] = reevaluated;
                        Int32 updated = 0;
                        dr["Updated"] = updated;
                        Int32 newRetailer = 0;
                        dr["New"] = newRetailer;
                        Int32 total = reevaluated + updated + newRetailer;
                        dr["Total"] = total;
                        dtTodaysRspStatistics.Rows.Add(dr);
                    }
                }
                //<--- read every RSP

                //More details- http://closedxml.codeplex.com/
                var MyWorkBook = new XLWorkbook();
                var MyWorkSheet = MyWorkBook.Worksheets.Add("Sheet 1");

                int TotalColumns = dtTodaysRspStatistics.Columns.Count;

                //-->headline
                //first row is intentionaly left blank.
                var headLine = MyWorkSheet.Range(MyWorkSheet.Cell(2, 1).Address, MyWorkSheet.Cell(2, TotalColumns).Address);
                headLine.Style.Font.Bold = true;
                headLine.Style.Font.FontSize = 15;
                headLine.Style.Font.FontColor = XLColor.White;
                headLine.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headLine.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headLine.Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.25);
                headLine.Style.Border.TopBorder = XLBorderStyleValues.Medium;
                headLine.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                headLine.Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                headLine.Style.Border.RightBorder = XLBorderStyleValues.Medium;

                String reportHeader = "Phase-2 RSP-POS Status";
                headLine.Merge();
                headLine.Value = reportHeader;
                //<-- headline

                //--> column settings
                for (int i = 1; i < dtTodaysRspStatistics.Columns.Count + 1; i++)
                {
                    String combinedHeaderText = dtTodaysRspStatistics.Columns[i - 1].ColumnName.ToString();
                    string separatedColumnHeader = "";
                    foreach (char letter in combinedHeaderText)
                    {
                        if (Char.IsUpper(letter) && separatedColumnHeader.Length > 0)
                            separatedColumnHeader += " " + letter;
                        else
                            separatedColumnHeader += letter;
                    }

                    MyWorkSheet.Cell(4, i).Value = separatedColumnHeader;
                    MyWorkSheet.Cell(4, i).Style.Alignment.WrapText = true;
                }

                var columnRange = MyWorkSheet.Range(MyWorkSheet.Cell(4, 1).Address, MyWorkSheet.Cell(4, TotalColumns).Address);
                columnRange.Style.Font.Bold = true;
                columnRange.Style.Font.FontSize = 10;
                columnRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                columnRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                columnRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);

                columnRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                //<-- column settings

                //--> row data & settings
                for (int i = 0; i < dtTodaysRspStatistics.Rows.Count; i++)
                {
                    DataRow thirRow = dtTodaysRspStatistics.Rows[i];

                    for (int j = 0; j < dtTodaysRspStatistics.Columns.Count; j++)
                    {
                        MyWorkSheet.Cell(i + 5, j + 1).Value = thirRow[j].ToString();
                    }
                }

                var dataRowRange = MyWorkSheet.Range(MyWorkSheet.Cell(5, 1).Address, MyWorkSheet.Cell(dtTodaysRspStatistics.Rows.Count + 4, TotalColumns).Address);
                dataRowRange.Style.Font.Bold = false;
                dataRowRange.Style.Font.FontSize = 10;
                dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                dataRowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                dataRowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(219, 229, 241);
                dataRowRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                //<-- row data & settings

                var guiid = Guid.NewGuid();
                 var filepath = HttpContext.Server.MapPath("~/App_Data");
                 MyWorkBook.SaveAs(Path.Combine(filepath, "Excels", guiid + ".xlsx"));
                //// Prepare the response
                //Response.Clear();
                //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //Response.AddHeader("content-disposition", "attachment;filename=\"" + reportHeader + ".xlsx\"");

                //// Flush the workbook to the Response.OutputStream
                //using (MemoryStream memoryStream = new MemoryStream())
                //{
                //    MyWorkBook.SaveAs(memoryStream);
                //    memoryStream.WriteTo(Response.OutputStream);
                //    memoryStream.Close();
                //}

                //Response.End();

                 return Json(new {result="ok",filename=guiid + ".xlsx" },JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var path = HttpContext.Server.MapPath("~/App_Data");
                //ExceptionLogger.CreateLog(ex, path);
                return Json(new { result = "bad" }, JsonRequestBehavior.AllowGet);

            }
        }


        public JsonResult SelectedSrStatus(FormCollection data)
        {

            var arrOfWorks = data["Rsps"].ToString();
            var arrOfData = data["Test"].ToString();
            arrOfData = arrOfData.Remove(0, 1);

            String ConnectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            try
            {
                string sqlSelectRsp = @"select SurveyorId,SurveyorName, ContactNo,AreaName from Surveyors where SurveyorId>0 order by SurveyorId";

                String sqlSelectRetailers = @"select RetailerId,RetailerStatusId, IsActive, RspId, IsReevaluated, SurveyorId from Retailer  WHERE (RspId In (" + arrOfData + ") AND (SurveyorId is not null))";

                DataTable dtSrList = new DataTable(); DataTable dtRetailerList = new DataTable();

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        using (SqlDataAdapter da = new SqlDataAdapter(command))
                        {
                            command.CommandText = sqlSelectRsp;
                            connection.Open();
                            da.Fill(dtSrList);// fill RSP table
                            command.CommandText = sqlSelectRetailers;
                            da.Fill(dtRetailerList); //fill Retailer table
                            connection.Close();
                        }
                    }
                }


                DataTable dtTodaysRspStatistics = new DataTable();
                dtTodaysRspStatistics.Columns.Add("SurveyorId", typeof(System.String));
                dtTodaysRspStatistics.Columns.Add("SurveyorName", typeof(System.String));
                dtTodaysRspStatistics.Columns.Add("ContactNo", typeof(System.String));
                dtTodaysRspStatistics.Columns.Add("AreaName", typeof(System.String));
                dtTodaysRspStatistics.Columns.Add("Updated", typeof(System.Int32));
                dtTodaysRspStatistics.Columns.Add("New", typeof(System.Int32));
                dtTodaysRspStatistics.Columns.Add("Reevaluated", typeof(System.Int32));
                dtTodaysRspStatistics.Columns.Add("Total", typeof(System.Int32));

                //---> read every SR
                foreach (DataRow sr in dtSrList.Rows)
                {
                    Int32 srId = Convert.ToInt32(sr["SurveyorId"]);
                    String srName = sr["SurveyorName"].ToString();
                    String contactNo = sr["ContactNo"].ToString();
                    String areaName = sr["AreaName"].ToString();
                    DataRow[] isRecordsFound = dtRetailerList.Select("SurveyorId=" + srId + "");
                    if (isRecordsFound.Length > 0)
                    {
                        DataRow dr;
                        dr = dtTodaysRspStatistics.NewRow();
                        dr["SurveyorId"] = srId;
                        dr["SurveyorName"] = srName;
                        dr["ContactNo"] = contactNo;
                        dr["AreaName"] = areaName;
                        Int32 reevaluated = Convert.ToInt32(dtRetailerList.Compute("Count(RetailerId)", "SurveyorId=" + srId + " and RetailerStatusId=2 and IsReevaluated=1"));
                        dr["Reevaluated"] = reevaluated;
                        Int32 updated = Convert.ToInt32(dtRetailerList.Compute("Count(RetailerId)", "SurveyorId=" + srId + " and RetailerStatusId=2 and IsReevaluated=0"));
                        dr["Updated"] = updated;
                        Int32 newRetailer = Convert.ToInt32(dtRetailerList.Compute("Count(RetailerId)", "SurveyorId=" + srId + " and RetailerStatusId=3 AND IsReevaluated=0"));
                        dr["New"] = newRetailer;
                        Int32 total = reevaluated + updated + newRetailer;
                        dr["Total"] = total;
                        dtTodaysRspStatistics.Rows.Add(dr);
                    }
                    //else
                    //{
                    //    DataRow dr;
                    //    dr = dtTodaysRspStatistics.NewRow();
                    //    dr["SurveyorId"] = srId;
                    //    dr["SurveyorName"] = srName;
                    //    dr["ContactNo"] = contactNo;
                    //    dr["AreaName"] = areaName;
                    //    Int32 reevaluated = 0;
                    //    dr["Reevaluated"] = reevaluated;
                    //    Int32 updated = 0;
                    //    dr["Updated"] = updated;
                    //    Int32 newRetailer = 0;
                    //    dr["New"] = newRetailer;
                    //    Int32 total = reevaluated + updated + newRetailer;
                    //    dr["Total"] = total;
                    //    dtTodaysRspStatistics.Rows.Add(dr);
                    //}
                }
                //<--- read every RSP

                //More details- http://closedxml.codeplex.com/
                var MyWorkBook = new XLWorkbook();
                var MyWorkSheet = MyWorkBook.Worksheets.Add("Sheet 1");

                int TotalColumns = dtTodaysRspStatistics.Columns.Count;

                //-->headline
                //first row is intentionaly left blank.
                var headLine = MyWorkSheet.Range(MyWorkSheet.Cell(2, 1).Address, MyWorkSheet.Cell(2, TotalColumns).Address);
                headLine.Style.Font.Bold = true;
                headLine.Style.Font.FontSize = 15;
                headLine.Style.Font.FontColor = XLColor.White;
                headLine.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headLine.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headLine.Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.25);
                headLine.Style.Border.TopBorder = XLBorderStyleValues.Medium;
                headLine.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                headLine.Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                headLine.Style.Border.RightBorder = XLBorderStyleValues.Medium;

                String reportHeader = "Phase-2 Surveyor-POS Status";
                headLine.Merge();
                headLine.Value = reportHeader;
                //<-- headline

                //--> column settings
                for (int i = 1; i < dtTodaysRspStatistics.Columns.Count + 1; i++)
                {
                    String combinedHeaderText = dtTodaysRspStatistics.Columns[i - 1].ColumnName.ToString();
                    string separatedColumnHeader = "";
                    foreach (char letter in combinedHeaderText)
                    {
                        if (Char.IsUpper(letter) && separatedColumnHeader.Length > 0)
                            separatedColumnHeader += " " + letter;
                        else
                            separatedColumnHeader += letter;
                    }

                    MyWorkSheet.Cell(4, i).Value = separatedColumnHeader;
                    MyWorkSheet.Cell(4, i).Style.Alignment.WrapText = true;
                }

                var columnRange = MyWorkSheet.Range(MyWorkSheet.Cell(4, 1).Address, MyWorkSheet.Cell(4, TotalColumns).Address);
                columnRange.Style.Font.Bold = true;
                columnRange.Style.Font.FontSize = 10;
                columnRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                columnRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                columnRange.Style.Fill.BackgroundColor = XLColor.FromArgb(171, 195, 223);

                columnRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                columnRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                //<-- column settings

                //--> row data & settings
                for (int i = 0; i < dtTodaysRspStatistics.Rows.Count; i++)
                {
                    DataRow thirRow = dtTodaysRspStatistics.Rows[i];

                    for (int j = 0; j < dtTodaysRspStatistics.Columns.Count; j++)
                    {
                        MyWorkSheet.Cell(i + 5, j + 1).Value = thirRow[j].ToString();
                    }
                }

                var dataRowRange = MyWorkSheet.Range(MyWorkSheet.Cell(5, 1).Address, MyWorkSheet.Cell(dtTodaysRspStatistics.Rows.Count + 4, TotalColumns).Address);
                dataRowRange.Style.Font.Bold = false;
                dataRowRange.Style.Font.FontSize = 10;
                dataRowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                dataRowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                dataRowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(219, 229, 241);
                dataRowRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                //<-- row data & settings

                var guiid = Guid.NewGuid();
                var filepath = HttpContext.Server.MapPath("~/App_Data");
                MyWorkBook.SaveAs(Path.Combine(filepath, "Excels", guiid + ".xlsx"));


                return Json(new { result = "ok", filename = guiid + ".xlsx" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var path = HttpContext.Server.MapPath("~/App_Data");
                //ExceptionLogger.CreateLog(ex, path);
                return Json(new { result = "bad" }, JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult DownloadReportFile() 
        {
            string fileName = Request.QueryString["name"].ToString();
            var filepath = HttpContext.Server.MapPath("~/App_Data");
            filepath= Path.Combine(filepath, "Excels", fileName);

            HttpContext.Response.Clear();
            HttpContext.Response.ClearContent();
            HttpContext.Response.ClearHeaders();
            HttpContext.Response.AddHeader("content-disposition", string.Format("attachment; filename={0}", "" + fileName ));
            HttpContext.Response.ContentType = "application/excel";
            HttpContext.Response.WriteFile(filepath);
            HttpContext.Response.End();
            System.IO.File.Delete(filepath);

            return View();
        }
       
	}
}