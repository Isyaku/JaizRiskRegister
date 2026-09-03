using JaizRiskRegister.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace JaizRiskRegister.DbData
{
    public class AppDbContext
    {   
        public class RiskDbContext : DbContext
        {
            public RiskDbContext(DbContextOptions<RiskDbContext> options)
                : base(options)
            {
            }
            public DbSet<Department> departments { get; set; }
            public DbSet<RR_Submission> RR_Submission { get; set; }
            public DbSet<RR_General_Template> RR_General_Template { get; set; }
            public DbSet<RR_Schedule> RR_Schedule { get; set; }
            public DbSet<RR_Action_Log> RR_Action_Log { get; set; }
        }

        //public class DeptOnPortalDbContext : DbContext
        //{
        //    public DeptOnPortalDbContext(DbContextOptions<DeptOnPortalDbContext> options)
        //        : base(options)
        //    {
        //    }
        //    public DbSet<Department> departments { get; set; }           
        //}
    }
}
