using System;
using System.Linq;
using System.Text;
using System.Threading;
using NewVPlusSale.Business.Infrastructure.Contract;
using NewVPlusSales.Business.Infrastructure;
using NewVPlusSales.Business.Infrastructure.Contract;
using XPLUG.WEBTOOLS;
using XPLUG.WEBTOOLS.Security;

namespace NewVPlusSales.Business.Core
{
    internal class IdGenerator
    {

       
        private static INewVPlusSalesRepository<Candidate> _repository;
        private static NewVPlusSalesUoWork _uoWork;

        private static void Init()
        {
            _uoWork = new NewVPlusSaleUoWork(/*You can specify you custom context here*/);
            _repository = new NewVPlusSaleRepository<Candidate>(_uoWork);
        }


       
        internal static string genRefNo(string firstName, string mobileNo, string confirmCode, out string msg)
        {
            Init();
            try
            {

                var serialId = SerialNumberKeeperRepository.Generate();
                if (serialId < 1)
                {
                    ErrorManager.LogApplicationError("Serial Gen", "Message", "Empty Val");
                    msg = "Unable to generate candidate key [Serial Gen]";
                    return "";
                }
                var keyHash = serialId + "_" +  mobileNo.Trim().Replace(" ", "") + "_" + confirmCode;
                var keyCode = UniqueHashing.GetStandardHash(keyHash);
                long keyDev = 0;
                if (keyCode.ToString().Length < 12)
                {
                    keyDev = long.Parse(Math.Abs(keyCode) + "" + Math.Abs(Environment.TickCount)) ;
                }
                if (keyDev.ToString().Length < 12)
                {
                    var thisCheck = keyDev + "" + Math.Abs(Environment.TickCount);
                    if (thisCheck.Length > 17)
                    {
                        thisCheck = thisCheck.Substring(0, 17);
                    }
                    keyDev = long.Parse(thisCheck);
                }
                if (keyDev.ToString().Length < 12)
                {
                    ErrorManager.LogApplicationError("Key Code", "Message", "Empty Val");
                    msg = "Unable to generate candidate key [Key Code]";
                    return "";
                }
                msg = "";
                return keyDev.ToString().Substring(0, 12);
            }
            catch (Exception ex)
            {
                msg = ex.GetBaseException().Message;
                ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
                return "";
            }
        }
        internal static string RandCodeGen(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        internal static string RandCodeGenMixed(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjklmnpq0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        internal static string OTPCodeGen(int size)
        {
            try
            {
                if (size > 8)
                {
                    return "";
                }
                var thisTick = Math.Abs(Environment.TickCount).ToString();
                if (thisTick.Length < 10)
                {
                    Thread.Sleep(50);
                    var thisTick2 = Math.Abs(Environment.TickCount).ToString();
                    thisTick = thisTick + "" + thisTick2;
                }

                var rnx = new Random();
                var rndId = rnx.Next(1, 6);

                switch (size)
                {
                    case 4:
                        return thisTick.Substring(8, 2) + "" + thisTick.Substring(3, 2);
                    case 5:
                        return thisTick.Substring(8, 2) + "" + thisTick.Substring(3, 2) + "" + thisTick.Substring(0, 1);
                    case 6:
                        switch (rndId)
                        {
                            case 1:
                                return thisTick.Substring(8, 2) + "" + thisTick.Substring(3, 2) + "" + thisTick.Substring(7, 1) + "" + thisTick.Substring(1, 1);
                            case 2:
                                return thisTick.Substring(7, 2) + "" + thisTick.Substring(4, 2) + "" + thisTick.Substring(0, 1) + "" + thisTick.Substring(3, 1);
                            case 3:
                                return thisTick.Substring(5, 2) + "" + thisTick.Substring(2, 1) + "" + thisTick.Substring(8, 1) + "" + thisTick.Substring(0, 2);
                            case 4:
                                return thisTick.Substring(9, 1) + "" + thisTick.Substring(6, 2) + "" + thisTick.Substring(2, 2) + "" + thisTick.Substring(0, 1);
                            case 5:
                                return thisTick.Substring(7, 2) + "" + thisTick.Substring(4, 2) + "" + thisTick.Substring(0, 2);
                            case 6:
                                return thisTick.Substring(0, 2) + "" + thisTick.Substring(7, 2) + "" + thisTick.Substring(4, 1) + "" + thisTick.Substring(9, 1);
                            default:
                                return thisTick.Substring(3, 3) + "" + thisTick.Substring(8, 1) + "" + thisTick.Substring(0, 2);
                        }

                    case 7:
                        return thisTick.Substring(8, 2) + "" + thisTick.Substring(3, 2) + "" + thisTick.Substring(7, 1) + "" + thisTick.Substring(0, 2);
                    case 8:
                        return thisTick.Substring(8, 2) + "" + thisTick.Substring(3, 2) + "" + thisTick.Substring(6, 2) + "" + thisTick.Substring(0, 2);
                    default:
                        return thisTick.Substring(8, 2) + "" + thisTick.Substring(3, 2) + "" + thisTick.Substring(7, 1) + "" + thisTick.Substring(1, 1);
                }
            }
            catch (Exception ex)
            {
                ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
                return "";

            }
        }

        internal static string GetMaxExamCode()
        {
            Init();
            try
            {
                var sql1 = new StringBuilder();
                sql1.Append("Select coalesce(Max(\"ExamCode\"), '0') FROM  \"JTestEngine\".\"ExamSetting\";");

                var agentNo = _repository.RepositoryContext().Database.SqlQuery<string>(sql1.ToString()).ToList();
                if (!agentNo.Any() || agentNo.Count != 1)
                {
                    return "";
                }

                if (string.IsNullOrEmpty(agentNo[0]) || agentNo[0] == "0")
                {
                    return 1.ToString("D6");
                }

                return (int.Parse(agentNo[0]) + 1).ToString("D6"); 

            }
            catch (Exception ex)
            {
                ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
                return "";
            }
        }

        private static string GetMaxAgentNumber()
        {
            Init();
                try
                {
                    var sql1 = new StringBuilder();
                    sql1.Append("Select coalesce(Max(\"AgencyNumber\"), '0') FROM  \"LottoBiz\".\"Agent\";");

                    var agentNo = _repository.RepositoryContext().Database.SqlQuery<string>(sql1.ToString()).ToList();
                    return agentNo.IsNullOrEmpty() ? "0" : agentNo[0];

                }
                catch (Exception ex)
                {
                    ErrorManager.LogApplicationError(ex.StackTrace, ex.Source, ex.Message);
                    return "0";
                }
            }
        }
}
