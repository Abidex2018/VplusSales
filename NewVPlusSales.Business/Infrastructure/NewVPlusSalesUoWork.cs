using System;
using System.Data.Entity;
using NewVPlusSales.Business.Infrastructure.Contract;

namespace NewVPlusSales.Business.Infrastructure
{
    internal class NewVPlusSalesUoWork : INewVPlusSalesUoWork, IDisposable
    {
        private readonly NewVPlusSalesContext _dbContext;

        public NewVPlusSalesUoWork(NewVPlusSalesContext context)
		{
			_dbContext = context;
		}

		public NewVPlusSalesUoWork()
		{
            _dbContext = new NewVPlusSalesContext();
		}

		public void SaveChanges()
		{
            _dbContext.NewVPlusSalesDbContext.SaveChanges();
		}
       
        public NewVPlusSalesContext Context => _dbContext;

        #region Implementation of IDisposable
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!disposing) return;
			if (_disposed) return;
			 _dbContext.Dispose();
			 _disposed = true;
		}

		private bool _disposed;

        ~NewVPlusSalesUoWork()
		{
			 Dispose(false);
		}

		#endregion


	//Class File Generated from codeZAPP 3.0.135 | www.xplugng.com | All Rights Reserved.
        public DbContextTransaction BeginTransaction()
	    {
            return _dbContext.NewVPlusSalesDbContext.Database.BeginTransaction();
	    }
    }
}
