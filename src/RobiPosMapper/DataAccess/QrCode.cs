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
    
    public partial class QrCode
    {
        public int QrCodeId { get; set; }
        public Nullable<int> AreaId { get; set; }
        public Nullable<int> RspId { get; set; }
        public Nullable<int> IsPrinted { get; set; }
        public Nullable<int> IsDistributed { get; set; }
        public int SerialNo { get; set; }
        public Nullable<System.DateTime> PrintDate { get; set; }
        public Nullable<System.DateTime> DistributionDate { get; set; }
    }
}
