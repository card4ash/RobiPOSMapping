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
    
    public partial class SimPosCode
    {
        public int SimPosCodeId { get; set; }
        public string SimPosCode1 { get; set; }
        public Nullable<int> RetailerId { get; set; }
    
        public virtual Retailer Retailer { get; set; }
    }
}
