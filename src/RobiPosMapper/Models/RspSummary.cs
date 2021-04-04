using System;
using System.Collections.Generic;
using System.Linq;
using System.Web; using System.Data; using System.Data.SqlClient;

namespace RobiPosMapper.Models
{
    public class RspSummary
    {
        public int RspId { get; set; }
        public string RspName { get; set; }
        public string AreaName { get; set; }
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
       
        public DateTime? FirstWorkDate { get; set; }

        //koto din dhore kaj cholchhe, tar quantity i.e. 12 din 
        //Today - FirstWorkDate = WorkDays
        public int WorkDays { get; set; }

        //
        public int RetailersPerDay { get; set; }  //Retailers Per Day
        public int EstimatedDaysToComplete { get; set; }
    }

    //Note
    //To submit the bill for partial work progress, this method was coded.
    public class RspSummaryForReport:RspSummary
    {
        public int TotalNewRetailersQuantity { get; set; }
    }

    public static class RspSummaryManager
    {
        public static List<RspSummary> GetRspSummary(int regionId,SqlCommand command)
        {
            List<RspSummary> summaryList = new List<RspSummary>();

            string sqlSelectRspSummary = @"SELECT     dbo.RSP.RspId, dbo.RSP.RspName,
                                            (Select COUNT( RetailerId) from Retailer Where RetailerStatusId<3 And RspId=dbo.RSP.RspId) AS TotalRetailers,
                                            (Select COUNT( RetailerId) from Retailer Where RetailerStatusId=2 AND IsActive=1 And RspId=dbo.RSP.RspId) AS TotalUpdatedRetailers,
                                            (Select COUNT( RetailerId) from Retailer Where IsActive=0 And RspId=dbo.RSP.RspId) AS TotalNotFoundRetailers,
                                            (Select COUNT( RetailerId) from Retailer Where IsVerifiedByRsp=1 And RspId=dbo.RSP.RspId) AS TotalVerifiedRetailers,
                                            (Select COUNT(Distinct (SurveyorId)) from Retailer Where RspId=dbo.RSP.RspId) AS SrCount,
                                            (select Min(SurveyorActivityDateTime) from Retailer Where RspId=dbo.RSP.RspId) AS FirstWorkDate,
                                             dbo.Area.AreaName
                                            FROM dbo.Region INNER JOIN
                                                                  dbo.Area ON dbo.Region.RegionId = dbo.Area.RegionId INNER JOIN
                                                                  dbo.RSP ON dbo.Area.AreaId = dbo.RSP.AreaId
                                            WHERE (dbo.Region.RegionId = " + regionId + @")
                                            order by dbo.RSP.RspName";

            command.CommandText = sqlSelectRspSummary;
            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    RspSummary rsp = new RspSummary();
                    rsp.RspId = reader.GetInt32(0);
                    rsp.RspName = reader.GetString(1);
                    rsp.AreaName = reader.GetString(8);

                    rsp.TotalRetailersQuantity = reader.GetInt32(2);

                    if (reader[7]!=DBNull.Value)
                    {
                        rsp.TotalUpdatedRetailersQuantity = reader.GetInt32(3);
                        rsp.TotalNotFoundRetailersQuantity = reader.GetInt32(4);
                        rsp.TotalVerifiedRetailersQuantity = reader.GetInt32(5);
                        rsp.SrCount = reader.GetInt32(6);
                        rsp.FirstWorkDate = reader.GetDateTime(7);

                        int totalWorked = rsp.TotalUpdatedRetailersQuantity + rsp.TotalNotFoundRetailersQuantity;
                        rsp.ProgressPercentage = (totalWorked * 100) / rsp.TotalRetailersQuantity;

                        if (rsp.TotalVerifiedRetailersQuantity>0 &&  rsp.TotalUpdatedRetailersQuantity>0)
                        {
                            rsp.VerifiedPercetage = (rsp.TotalVerifiedRetailersQuantity * 100) / rsp.TotalUpdatedRetailersQuantity;
                        }

                        if (rsp.TotalRetailersQuantity>0 && rsp.SrCount>0)
                        {
                            rsp.RetailerSrRatio = rsp.TotalRetailersQuantity / rsp.SrCount;
                        }

                        rsp.WorkDays = (DateTime.Now - Convert.ToDateTime(rsp.FirstWorkDate)).Days;

                        if (totalWorked>0 && rsp.WorkDays>0)
                        {
                            rsp.RetailersPerDay = totalWorked / rsp.WorkDays;
                        }

                        if (rsp.RetailersPerDay>0)
                        {
                            rsp.EstimatedDaysToComplete = (rsp.TotalRetailersQuantity - totalWorked) / rsp.RetailersPerDay;
                        }
                        
                       
                       
                    }



                    summaryList.Add(rsp);
                }
            }

            if (!reader.IsClosed)
            {
                reader.Close();
            }

            return summaryList;
        }

        //Note
        //To submit the bill for partial work progress, this method was coded.
        public static List<RspSummaryForReport> GetRspSummaryByDate(int regionId, SqlCommand command)
        {
            List<RspSummaryForReport> summaryList = new List<RspSummaryForReport>();

            string sqlSelectRspSummary = @"SELECT     dbo.RSP.RspId, dbo.RSP.RspName,
                                            (Select COUNT( RetailerId) from Retailer Where RetailerStatusId<3 And RspId=dbo.RSP.RspId) AS TotalRetailers,
                                            (Select COUNT( RetailerId) from Retailer Where RetailerStatusId=2 AND IsActive=1 And RspId=dbo.RSP.RspId AND CAST(SurveyorActivityDateTime AS DATE)<'2015-06-09') AS TotalUpdatedRetailers,
                                            (Select COUNT( RetailerId) from Retailer Where IsActive=0 And RspId=dbo.RSP.RspId) AS TotalNotFoundRetailers,
                                            (Select COUNT( RetailerId) from Retailer Where IsVerifiedByRsp=1 And RspId=dbo.RSP.RspId AND CAST(RspVerificationDateTime AS DATE)< '2015-06-09') AS TotalVerifiedRetailers,
                                            (Select COUNT(Distinct (SurveyorId)) from Retailer Where RspId=dbo.RSP.RspId) AS SrCount,
                                            (select Min(SurveyorActivityDateTime) from Retailer Where RspId=dbo.RSP.RspId) AS FirstWorkDate,
                                             dbo.Area.AreaName,
                                            (Select COUNT(RetailerId) from Retailer Where RetailerStatusId=3 And RspId=dbo.RSP.RspId AND CAST(SurveyorActivityDateTime AS DATE)<'2015-06-09') AS TotalNewRetailers
                                            FROM dbo.Region INNER JOIN
                                                                  dbo.Area ON dbo.Region.RegionId = dbo.Area.RegionId INNER JOIN
                                                                  dbo.RSP ON dbo.Area.AreaId = dbo.RSP.AreaId
                                            WHERE (dbo.Region.RegionId = " + regionId + @")
                                            order by dbo.RSP.RspName";

            command.CommandText = sqlSelectRspSummary;
            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    RspSummaryForReport rsp = new RspSummaryForReport();
                    rsp.RspId = reader.GetInt32(0);
                    rsp.RspName = reader.GetString(1);
                    rsp.AreaName = reader.GetString(8);

                    rsp.TotalRetailersQuantity = reader.GetInt32(2);
                    rsp.TotalNewRetailersQuantity = reader.GetInt32(9);

                    if (reader[7] != DBNull.Value)
                    {
                        rsp.TotalUpdatedRetailersQuantity = reader.GetInt32(3);
                        rsp.TotalNotFoundRetailersQuantity = reader.GetInt32(4);
                        rsp.TotalVerifiedRetailersQuantity = reader.GetInt32(5);
                        rsp.SrCount = reader.GetInt32(6);
                        rsp.FirstWorkDate = reader.GetDateTime(7);

                        int totalWorked = rsp.TotalUpdatedRetailersQuantity + rsp.TotalNotFoundRetailersQuantity;
                        rsp.ProgressPercentage = (totalWorked * 100) / rsp.TotalRetailersQuantity;

                        if (rsp.TotalVerifiedRetailersQuantity > 0 && rsp.TotalUpdatedRetailersQuantity > 0)
                        {
                            rsp.VerifiedPercetage = (rsp.TotalVerifiedRetailersQuantity * 100) / rsp.TotalUpdatedRetailersQuantity;
                        }

                        if (rsp.TotalRetailersQuantity > 0 && rsp.SrCount > 0)
                        {
                            rsp.RetailerSrRatio = rsp.TotalRetailersQuantity / rsp.SrCount;
                        }

                        rsp.WorkDays = (DateTime.Now - Convert.ToDateTime(rsp.FirstWorkDate)).Days;

                        if (totalWorked > 0 && rsp.WorkDays > 0)
                        {
                            rsp.RetailersPerDay = totalWorked / rsp.WorkDays;
                        }

                        if (rsp.RetailersPerDay > 0)
                        {
                            rsp.EstimatedDaysToComplete = (rsp.TotalRetailersQuantity - totalWorked) / rsp.RetailersPerDay;
                        }



                    }



                    summaryList.Add(rsp);
                }
            }

            if (!reader.IsClosed)
            {
                reader.Close();
            }

            return summaryList;
        }
    }
}