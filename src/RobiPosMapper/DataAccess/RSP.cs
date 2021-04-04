//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RobiPosMapper.DataAccess
{
    using System;
    using System.Collections.Generic;
    
    public partial class RSP
    {
        public RSP()
        {
            this.Retailers = new HashSet<Retailer>();
        }
    
        public int RspId { get; set; }
        public Nullable<int> RspMsisdn { get; set; }
        public string RspCode { get; set; }
        public string Password { get; set; }
        public string RspName { get; set; }
        public string Address { get; set; }
        public Nullable<int> AreaId { get; set; }
        public Nullable<bool> Active { get; set; }
        public string Remarks { get; set; }
        public Nullable<int> IsPrinted { get; set; }
        public Nullable<int> IsDistributed { get; set; }
        public Nullable<System.DateTime> PrintDate { get; set; }
        public Nullable<System.DateTime> DistributionDate { get; set; }
    
        public virtual ICollection<Retailer> Retailers { get; set; }
    }
}
