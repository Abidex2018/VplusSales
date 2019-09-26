using System;
using System.Data.Entity;

namespace NewVPlusSales.Business.Infrastructure.Contract
{
    internal interface INewVPlusSalesContext : IDisposable 
    {
        DbContext NewVPlusSalesDbContext { get; }
    }
}
