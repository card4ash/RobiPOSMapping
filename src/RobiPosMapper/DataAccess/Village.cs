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
    
    public partial class Village
    {
        public Village()
        {
            this.Retailers = new HashSet<Retailer>();
        }
    
        public int VillageId { get; set; }
        public string VillageName { get; set; }
        public int MauzaId { get; set; }
    
        public virtual Mauza Mauza { get; set; }
        public virtual ICollection<Retailer> Retailers { get; set; }
    }
}
