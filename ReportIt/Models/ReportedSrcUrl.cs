//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ReportIt.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ReportedSrcUrl
    {
        public int pid { get; set; }
        public string PageUrlHash { get; set; }
        public string SrcUrlHash { get; set; }
    
        public virtual EUReported EUReported { get; set; }
        public virtual SrcUrl SrcUrl { get; set; }
    }
}
