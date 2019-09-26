namespace NewVPlusSales.Business.Infrastructure.Contract
{
    internal interface INewVPlusSalesUoWork
    {
        void SaveChanges();
        NewVPlusSalesContext Context { get; }
    }
}
