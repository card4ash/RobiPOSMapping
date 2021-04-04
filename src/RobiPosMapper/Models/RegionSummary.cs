using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace RobiPosMapper.Models
{
    public class RegionSummary
    {
        public int RegionId { get; set; }
        public string RegionName { get; set; }
        public int TotalRetailersQuantity { get; set; }
        public int TotalUpdatedRetailersQuantity { get; set; }
        public int TotalNotFoundRetailersQuantity { get; set; }

        //(Updated*100)/Total
        public int ProgressPercentage { get; set; }
        public int TotalVerifiedRetailersQuantity { get; set; }
        public int VerifiedPercetage { get; set; }

        //Distinct SR found 
        public int SrCount { get; set; }

        //TotalRetailersQuantity/SrCount
        public int RetailerSrRatio { get; set; }

        public DateTime FirstWorkDate { get; set; }

        //koto din dhore kaj cholchhe, tar quantity i.e. 12 din 
        //Today - FirstWorkDate = WorkDays
        public int WorkDays { get; set; }

        //
        public int RetailersPerDay { get; set; }  //Retailers Per Day
        public int EstimatedDaysToComplete { get; set; }

        public List<RspSummary> RspSummary { get; set; }
    }

    //Note
    //To submit the bill for partial work progress, this method was coded.
    public class RegionSummaryForReport: RegionSummary
    {

        public List<RspSummaryForReport> RspSummaryForReport { get; set; }
    }

    public static class RegionSummaryManager
    {
        public static List<RegionSummary> GetRegionSummary(int moId,string connectionString)
        {
            List<RegionSummary> summaryList = new List<RegionSummary>();
            try
            {
                string sqlSelectRegionData = @"SELECT dbo.Region.RegionId, dbo.Region.RegionName,
                                                 (Select COUNT( RetailerId) from Retailer Where RetailerStatusId<3 And RegionId=dbo.Region.RegionId) AS TotalRetailers,
                                                 (Select COUNT( RetailerId) from Retailer Where RetailerStatusId=2 AND IsActive=1 And RegionId=dbo.Region.RegionId) AS TotalUpdatedRetailers,
                                                 (Select COUNT( RetailerId) from Retailer Where IsActive=0 And RegionId=dbo.Region.RegionId) AS TotalNotFoundRetailers,
                                                 (Select COUNT( RetailerId) from Retailer Where IsVerifiedByRsp=1 And RegionId=dbo.Region.RegionId) AS TotalVerifiedRetailers,
                                                 (Select COUNT(Distinct (SurveyorId)) from Retailer Where RegionId=dbo.Region.RegionId) AS SrCount,
                                                 (select Min(SurveyorActivityDateTime) from Retailer Where RegionId=dbo.Region.RegionId) AS FirstWorkDate
                                             FROM dbo.Region INNER JOIN
                                                  dbo.MonitoringOfficerRegion ON dbo.Region.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                             WHERE (dbo.MonitoringOfficerRegion.MoId = "+ moId +")";

                using (SqlConnection connection=new SqlConnection(connectionString))
                {
                    using (SqlCommand command=new SqlCommand(sqlSelectRegionData,connection))
                    {
                        command.CommandTimeout = 60; connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                RegionSummary region = new RegionSummary();
                                region.RegionId = reader.GetInt32(0);
                                region.RegionName = reader.GetString(1);
                                region.TotalRetailersQuantity = reader.GetInt32(2);
                                region.TotalUpdatedRetailersQuantity = reader.GetInt32(3);
                                region.TotalNotFoundRetailersQuantity = reader.GetInt32(4);
                                region.TotalVerifiedRetailersQuantity = reader.GetInt32(5);
                                region.SrCount = reader.GetInt32(6);
                                region.FirstWorkDate = reader.GetDateTime(7);

                                int totalWorked = region.TotalUpdatedRetailersQuantity + region.TotalNotFoundRetailersQuantity;
                                region.ProgressPercentage = (totalWorked * 100) / region.TotalRetailersQuantity;
                                region.VerifiedPercetage = (region.TotalVerifiedRetailersQuantity * 100) / region.TotalUpdatedRetailersQuantity;
                                region.RetailerSrRatio = region.TotalRetailersQuantity / region.SrCount;
                                region.WorkDays = (DateTime.Now - region.FirstWorkDate).Days;
                                region.RetailersPerDay = totalWorked / region.WorkDays;
                                region.EstimatedDaysToComplete=(region.TotalRetailersQuantity-totalWorked)/region.RetailersPerDay;
                                
                                summaryList.Add(region);
                            }
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }

                        foreach (var item in summaryList)
                        {
                            item.RspSummary = RspSummaryManager.GetRspSummary(item.RegionId, command);
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                //Just keep silent
                
            }

            return summaryList;
        }


        //Note
        //To submit the bill for partial work progress, this method was coded.
        public static List<RegionSummaryForReport> GetRegionSummaryByDate(int moId, string connectionString)
        {
            List<RegionSummaryForReport> summaryList = new List<RegionSummaryForReport>();
            try
            {
                string sqlSelectRegionData = @"SELECT dbo.Region.RegionId, dbo.Region.RegionName,
                                                 (Select COUNT( RetailerId) from Retailer Where RetailerStatusId<3 And RegionId=dbo.Region.RegionId) AS TotalRetailers,
                                                 (Select COUNT( RetailerId) from Retailer Where RetailerStatusId=2 AND IsActive=1 And RegionId=dbo.Region.RegionId) AS TotalUpdatedRetailers,
                                                 (Select COUNT( RetailerId) from Retailer Where IsActive=0 And RegionId=dbo.Region.RegionId) AS TotalNotFoundRetailers,
                                                 (Select COUNT( RetailerId) from Retailer Where IsVerifiedByRsp=1 And RegionId=dbo.Region.RegionId) AS TotalVerifiedRetailers,
                                                 (Select COUNT(Distinct (SurveyorId)) from Retailer Where RegionId=dbo.Region.RegionId) AS SrCount,
                                                 (select Min(SurveyorActivityDateTime) from Retailer Where RegionId=dbo.Region.RegionId) AS FirstWorkDate
                                             FROM dbo.Region INNER JOIN
                                                  dbo.MonitoringOfficerRegion ON dbo.Region.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                             WHERE (dbo.MonitoringOfficerRegion.MoId = " + moId + ")";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(sqlSelectRegionData, connection))
                    {
                        command.CommandTimeout = 60; connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                RegionSummaryForReport region = new RegionSummaryForReport();
                                region.RegionId = reader.GetInt32(0);
                                region.RegionName = reader.GetString(1);
                                region.TotalRetailersQuantity = reader.GetInt32(2);
                                region.TotalUpdatedRetailersQuantity = reader.GetInt32(3);
                                region.TotalNotFoundRetailersQuantity = reader.GetInt32(4);
                                region.TotalVerifiedRetailersQuantity = reader.GetInt32(5);
                                region.SrCount = reader.GetInt32(6);
                                region.FirstWorkDate = reader.GetDateTime(7);

                                int totalWorked = region.TotalUpdatedRetailersQuantity + region.TotalNotFoundRetailersQuantity;
                                region.ProgressPercentage = (totalWorked * 100) / region.TotalRetailersQuantity;
                                region.VerifiedPercetage = (region.TotalVerifiedRetailersQuantity * 100) / region.TotalUpdatedRetailersQuantity;
                                region.RetailerSrRatio = region.TotalRetailersQuantity / region.SrCount;
                                region.WorkDays = (DateTime.Now - region.FirstWorkDate).Days;
                                region.RetailersPerDay = totalWorked / region.WorkDays;
                                region.EstimatedDaysToComplete = (region.TotalRetailersQuantity - totalWorked) / region.RetailersPerDay;

                                summaryList.Add(region);
                            }
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }

                        foreach (var item in summaryList)
                        {
                            item.RspSummaryForReport = RspSummaryManager.GetRspSummaryByDate(item.RegionId, command);
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                //Just keep silent

            }

            return summaryList;
        }
    }
}