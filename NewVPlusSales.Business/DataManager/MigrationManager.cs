using System;
using System.Data.Entity.Migrations;


namespace NewVPlusSales.Business.Migrations
{
    internal class MigrationService
    {
        public static bool Migrate(out string msg)
        {
            try
            {
                var configuration = new Configuration();
                var migrator = new DbMigrator(configuration);
                migrator.Update();
                msg = "";
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }


        }

    }
}
