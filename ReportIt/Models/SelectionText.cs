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
    
    public partial class SelectionText
    {
        public int pId { get; set; }
        public int Id { get; set; }
        public string SelectionText1 { get; set; }
        public string SelectionTextHash { get; set; }
        public byte[] CreatedOn { get; set; }
        public System.DateTime UpdatedOn { get; set; }
        public bool Processed { get; set; }
    
        public virtual EUReported EUReported { get; set; }
    }
}
