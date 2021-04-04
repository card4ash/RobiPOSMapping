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
    
    public partial class Person
    {
        public Person()
        {
            this.Retailers = new HashSet<Retailer>();
        }
    
        public int PersonId { get; set; }
        public string PersonName { get; set; }
        public Nullable<int> RspId { get; set; }
        public Nullable<int> PersonTypeId { get; set; }
        public Nullable<int> AreaId { get; set; }
        public Nullable<int> PersonMsisdn { get; set; }
        public string Remarks { get; set; }
        public string LoginPassword { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string LanguagePreference { get; set; }
        public Nullable<int> IsPrinted { get; set; }
        public Nullable<int> IsDistributed { get; set; }
        public Nullable<System.DateTime> PrintDate { get; set; }
        public Nullable<System.DateTime> DistributionDate { get; set; }
    
        public virtual ICollection<Retailer> Retailers { get; set; }
    }
}
