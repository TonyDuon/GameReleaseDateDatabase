//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GamesDatabase_Project.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class gameGenre
    {
        public int ID { get; set; }
        public Nullable<int> gameID { get; set; }
        public Nullable<int> genreID { get; set; }
    
        public virtual Game Game { get; set; }
        public virtual genre genre { get; set; }
    }
}
