﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace vv.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class pgtabir1_shineEntities7 : DbContext
    {
        public pgtabir1_shineEntities7()
            : base("name=pgtabir1_shineEntities7")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Contacts> Contacts { get; set; }
        public virtual DbSet<Devices> Devices { get; set; }
        public virtual DbSet<GroupMessages> GroupMessages { get; set; }
        public virtual DbSet<Groups> Groups { get; set; }
        public virtual DbSet<GroupsMember> GroupsMember { get; set; }
        public virtual DbSet<MessageSeens> MessageSeens { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<ahkam> ahkam { get; set; }
        public virtual DbSet<pmp> pmp { get; set; }
        public virtual DbSet<pvupdate> pvupdate { get; set; }
        public virtual DbSet<SDATA> SDATA { get; set; }
        public virtual DbSet<shenasname> shenasname { get; set; }
        public virtual DbSet<SUSERS> SUSERS { get; set; }
        public virtual DbSet<tanbih> tanbih { get; set; }
        public virtual DbSet<tarfi> tarfi { get; set; }
        public virtual DbSet<tiket> tiket { get; set; }
        public virtual DbSet<update> update { get; set; }
        public virtual DbSet<zemanatname> zemanatname { get; set; }
    }
}
