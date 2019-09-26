using System;
using System.Data.Entity;
using NewVPlusSales.Business.DataManager;
using NewVPlusSales.Business.Infrastructure.Contract;


namespace NewVPlusSales.Business.Infrastructure
{
    internal  class NewVPlusSalesContext : INewVPlusSalesContext
    {
        public NewVPlusSalesContext(DbContext context)
		{
		    NewVPlusSalesDbContext = context ?? throw new ArgumentNullException(nameof(context));
            NewVPlusSalesDbContext.Configuration.LazyLoadingEnabled = false;
		}

        public NewVPlusSalesContext()
		{
            NewVPlusSalesDbContext = new NewVPlusSalesModel(); 
            NewVPlusSalesDbContext.Configuration.LazyLoadingEnabled = false;
		}

		public void Dispose()
		{
            NewVPlusSalesDbContext.Dispose();
		}
        
        public DbContext NewVPlusSalesDbContext { get; private set; }

       
    }
}
