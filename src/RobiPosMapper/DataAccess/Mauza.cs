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
    
    public partial class Mauza
    {
        public Mauza()
        {
            this.Retailers = new HashSet<Retailer>();
            this.Villages = new HashSet<Village>();
        }
    
        public int MauzaId { get; set; }
        public string MauzaName { get; set; }
        public int WardId { get; set; }
    
        public virtual Ward Ward { get; set; }
        public virtual ICollection<Retailer> Retailers { get; set; }
        public virtual ICollection<Village> Villages { get; set; }
    }
}
