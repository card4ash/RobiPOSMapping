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
    
    public partial class District
    {
        public District()
        {
            this.Thanas = new HashSet<Thana>();
        }
    
        public int DistrictId { get; set; }
        public string DistrictName { get; set; }
        public int DivisionId { get; set; }
    
        public virtual Area Area { get; set; }
        public virtual ICollection<Thana> Thanas { get; set; }
    }
}
