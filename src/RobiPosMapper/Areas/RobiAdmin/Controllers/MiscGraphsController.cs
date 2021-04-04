using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RobiPosMapper.Models;
using System.IO;
using ClosedXML;
using System.Data;
using System.Web.Configuration;
using System.Data.SqlClient;
using ClosedXML.Excel;

namespace RobiPosMapper.Areas.RobiAdmin.Controllers
{
    public class MiscGraphsController : Controller
    {

        #region Global Variables


        CultureInfo ci = new CultureInfo("bn-BD-robi");

        string[] InputDateFormatsAllowed = {
                                                       "d/M/yy", "d/M/yyyy",
                                                       "d/MM/yy", "d/MM/yyyy", 
                                                       "dd/M/yy", "dd/M/yyyy", 
                                                       "dd/MM/yy", "dd/MM/yyyy",

                                                       "d-M-yy", "d-M-yyyy",
                                                       "d-MM-yy", "d-MM-yyyy", 
                                                       "dd-M-yy", "dd-M-yyyy", 
                                                       "dd-MM-yy", "dd-MM-yyyy",

                                                       "d.M.yy", "d.M.yyyy",
                                                       "d.MM.yy", "d.MM.yyyy", 
                                                       "dd.M.yy", "dd.M.yyyy", 
                                                       "dd.MM.yy", "dd.MM.yyyy"

                                                    };

        String ConnectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
        string sqlInsertMoLog = "INSERT INTO MonitoringOfficerActivityLog(MoId,LogDescription) VALUES (@MoId,@LogDescription)";
        #endregion

        public ActionResult Index()
        {

            ViewBag.UserName = Request.QueryString["user"].ToString();
            ViewBag.MoId = Request.QueryString["moid"].ToString();

            ViewBag.ActiveMenuName = "Summary";
            ViewBag.Title = "Summary";
            ViewBag.MyCulture = ci;
            List<RegionSummary> regionSummaryList = RegionSummaryManager.GetRegionSummary(Convert.ToInt32(Request.QueryString["moid"]), ConnectionString);
            return View(regionSummaryList);
        }

        //Note
        //To submit the bill for partial work progress, this method was coded.
        //It has not UI. Can be accessed only via hand-typed URL.
        public ActionResult ExcelReport()
        {
            ViewBag.UserName = Request.QueryString["user"].ToString();
            ViewBag.MoId = Request.QueryString["moid"].ToString();

            ViewBag.ActiveMenuName = "Summary";
            ViewBag.Title = "Summary";
            ViewBag.MyCulture = ci;
            List<RegionSummaryForReport> regionSummaryList = RegionSummaryManager.GetRegionSummaryByDate(Convert.ToInt32(Request.QueryString["moid"]), ConnectionString);

            DataTable dtData = new DataTable();
            dtData.Columns.Add("RegionName", typeof(System.String));
            dtData.Columns.Add("AreaName", typeof(System.String));
            dtData.Columns.Add("RspName", typeof(System.String));
            dtData.Columns.Add("TotalRetailersQuantity", typeof(System.Int32));
            dtData.Columns.Add("TotalUpdatedRetailersQuantity", typeof(System.Int32));
           // dtData.Columns.Add("TotalNotFoundRetailersQuantity", typeof(System.Int32));
            dtData.Columns.Add("TotalVerifiedRetailersQuantity", typeof(System.Int32));
           
            dtData.Columns.Add("TotalNewRetailersQuantity", typeof(System.Int32));

            DataRow dr;
            foreach (RegionSummaryForReport region in regionSummaryList)
            {
                foreach (RspSummaryForReport rsp in region.RspSummaryForReport)
                {
                    dr = dtData.NewRow();
                    dr["RegionName"] = region.RegionName;
                    dr["AreaName"] = rsp.AreaName;
                    dr["RspName"] = rsp.RspName;
                    dr["TotalRetailersQuantity"] = rsp.TotalRetailersQuantity;
                    dr["TotalUpdatedRetailersQuantity"] = rsp.TotalUpdatedRetailersQuantity;
                    //dr["TotalNotFoundRetailersQuantity"] = rsp.TotalNotFoundRetailersQuantity;
                    dr["TotalNewRetailersQuantity"] = rsp.TotalNewRetailersQuantity;
                    dr["TotalVerifiedRetailersQuantity"] = rsp.TotalVerifiedRetailersQuantity;
                    
                    dtData.Rows.Add(dr);
                }
               
            }

            string fileName = Guid.NewGuid().ToString();
            String reportHeader = "This is a header.";
            try
            {


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
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
	}
}