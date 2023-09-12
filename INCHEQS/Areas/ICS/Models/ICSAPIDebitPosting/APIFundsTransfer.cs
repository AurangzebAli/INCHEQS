using INCHEQS.DataAccessLayer;
using INCHEQS.Security.SystemProfile;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Ajax.Utilities;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace INCHEQS.Areas.ICS.Models.ICSAPIDebitPosting
{
    public class clsAccountInfo
    {

        public string channelId { get; set; }
        public string accountNumber { get; set; }


    }
    public class clsInstrumentDetail
    {


        public string channelId { get; set; }
        public string accountNumber { get; set; }
        public string instrumentType { get; set; }
        public string instrumentNo { get; set; }

 
}
    public class APIFundsTransfer
    {
        private readonly ApplicationDbContext dbContext;
        public APIFundsTransfer(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;

        }

        public string fromAccountType { get; set; }
        public string  beneficiaryIBAN { get; set; }
        public string pinData { get; set; }
        public string relationshipId { get; set; }
        public string transmissionDate { get; set; }
        public string transmissionTime { get; set; }
        public string dateLocalTran { get; set; }
        public string timeLocalTran { get; set; }
        public string stan { get; set; }
        public string rrn { get; set; }
        public string acqInstCode { get; set; }
        public string fromBankIMD { get; set; }
        public string fromAccountNumber { get; set; }
        public string fromAccountCurrency { get; set; }
        public string fromAccountBranch { get; set; }
        public string toBankIMD { get; set; }
        public string toAccountNumber { get; set; }
        public string transactionAmount { get; set; }
        public string toAccountBranch { get; set; }
        public string transactionCurrency { get; set; }
        public string narrative { get; set; }
        public string purposeOfPayment { get; set; }
        public string cardAccepTermId { get; set; }



        #region API Parameters
        public string APIUsername { get; set; }
        public string APIPassword { get; set; }
        public string APIFundsTranferApiUrl { get; set; }
        public string APIFundsTransferAuth { get; set; }
        public string APITokenURL { get; set; }
        public string APITokenAuth { get; set; }
        public string APITokenGrantType { get; set; }
        public string APITokenScope { get; set; }

        public string APIFTAuth { get; set; }


        #endregion

        public  string GETAPIReponse1(FormCollection collection, string accNo,string chequeno)
        {

            GetAPIParam();
            string vTokenApiEndPoint = APITokenURL;
            RestClient tokenapicall = new RestClient(vTokenApiEndPoint);
            string Message = "";
            //tokenapicall.timeout = -1;
            RestRequest requesttokencall = new RestRequest("token"  , Method.Post);
            requesttokencall.AddHeader("authorization", APITokenAuth);
            requesttokencall.AddHeader("content-type", "application/x-www-form-urlencoded");
            requesttokencall.AddParameter("grant_type", APITokenGrantType);
            requesttokencall.AddParameter("scope", "AccountService");
            var resulttokencall = tokenapicall.ExecuteAsync(requesttokencall);
            try
            {
                //var resulttokencall = tokenapicall.ExecuteAsync(requesttokencall);
                var result = resulttokencall.Result;
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
               
                    JObject json = JObject.Parse(result.Content);
                    List<JToken> results = json.Children().ToList();
                    List<JToken> BearerToken = results[0].Children().ToList();
                    APIFTAuth = "Bearer " + BearerToken[0].ToString();

                    RestClient FTAPICall = new RestClient(APIFundsTranferApiUrl);
                    RestRequest requestFT = new RestRequest("", Method.Post);
                    List<SqlParameter> sqlParameterFT = new List<SqlParameter>();

                    DataTable dtfundstransfer = dbContext.GetRecordsAsDataTableSP("spcgFundsTransfer", sqlParameterFT.ToArray());
                    #region Comment Code
                    //APIFundsTransfer clsapift = new APIFundsTransfer(dbContext);
                    //clsapift.transmissionDate = DateTime.Now.ToString("yyyyMMdd");// collection["fldClearDate"].ToString("yyyyMMdd");
                    //clsapift.transmissionTime = DateTime.Now.ToString("hhmmss"); //"112834";//"hhmmss"
                    //clsapift.dateLocalTran = DateTime.Now.ToString("yyyyMMdd");// collection["fldClearDate"].ToString("yyyyMMdd");
                    //clsapift.timeLocalTran = DateTime.Now.ToString("hhmmss"); //"112834";// "hhmmss"
                    //clsapift.stan = dtfundstransfer.Rows[0]["fldFTcounter"].ToString();
                    ////"000002";
                    //clsapift.rrn = dtfundstransfer.Rows[0]["fldFTcounter"].ToString(); //"000002";
                    //clsapift.acqInstCode = "623977";
                    //clsapift.fromBankIMD = "623977";
                    //clsapift.fromAccountNumber = accNo;// "6580008552700087";
                    //clsapift.fromAccountCurrency = "586";
                    //clsapift.fromAccountBranch = collection["fldIssueBankCode"].Substring(collection["fldIssueBankCode"].Length - 3); //Account Branch
                    //clsapift.toBankIMD = "623977";
                    //if (collection["fldIssueBankCode"].Substring(3, 2) == "20")
                    //{
                    //    clsapift.toAccountNumber = "608080129";

                    //}
                    //else
                    //{
                    //    clsapift.toAccountNumber = "608080130";

                    //}
                    //clsapift.toAccountBranch = "954";
                    //clsapift.transactionAmount = (collection["fldAmount"].ToString() + "00").PadLeft(12, '0');//"000000002000";
                    //clsapift.transactionCurrency = "586";
                    //clsapift.narrative = "DebitPostingAccountNumber:" + accNo + " " + DateTime.Now;
                    //requestFT.AddHeader("authorization", APIFTAuth);
                    //requestFT.AddHeader("content-type", "application/json");
                    //requestFT.AddHeader("username", APIUsername);
                    //requestFT.AddHeader("password", "esbuser");



                    //requestFT.AddJsonBody(new { relationshipId ="''" });
                    //requestFT.AddJsonBody(new { transmissionDate = clsapift.transmissionDate });
                    //requestFT.AddJsonBody(new { transmissionTime= clsapift.transmissionTime });
                    //requestFT.AddJsonBody(new { dateLocalTran = clsapift.dateLocalTran });
                    //requestFT.AddJsonBody(new { timeLocalTran =  clsapift.timeLocalTran });
                    //requestFT.AddJsonBody(new { stan = clsapift.stan });
                    //requestFT.AddJsonBody(new { rrn  = clsapift.rrn });
                    //requestFT.AddJsonBody(new { acqInstCode = clsapift.acqInstCode });
                    //requestFT.AddJsonBody(new { pinData ="" });
                    //requestFT.AddJsonBody(new { fromBankIMD = clsapift.fromBankIMD });
                    //requestFT.AddJsonBody(new { fromAccountNumber =clsapift.fromAccountNumber });
                    //requestFT.AddJsonBody(new { fromAccountType = "''", });
                    //requestFT.AddJsonBody(new { fromAccountCurrency =clsapift.fromAccountCurrency });
                    //requestFT.AddJsonBody(new { fromAccountBranch = clsapift.fromAccountBranch });
                    //requestFT.AddJsonBody(new { tranIndecator = "''"  });
                    //requestFT.AddJsonBody(new { toBankIMD = clsapift.toBankIMD});
                    //requestFT.AddJsonBody(new { toAccountNumber = clsapift.toAccountNumber });
                    //requestFT.AddJsonBody(new { toAccountBranch =clsapift.toAccountBranch });
                    //requestFT.AddJsonBody(new { beneficiaryIBAN = "''" });
                    //requestFT.AddJsonBody(new { transactionAmount =clsapift.transactionAmount });
                    //requestFT.AddJsonBody(new { transactionCurrency =clsapift.transactionCurrency});
                    //requestFT.AddJsonBody(new { purposeOfPayment = "0122"  });
                    //requestFT.AddJsonBody(new { narrative = clsapift.narrative });
                    //requestFT.AddJsonBody(new { cardAccepTermId = "''" });
                    #endregion
                    Message =GetAPIResult1(APIFundsTranferApiUrl,  collection,dtfundstransfer,BearerToken[0].ToString(),accNo,chequeno);


                    return Message;
                }
                else
                {
                    return resulttokencall.Result.StatusCode.ToString() + " 1 " 
                        + resulttokencall.Result.StatusDescription.ToString() + " 2 " 
                        + resulttokencall.Result.Content  +
                        " apitoken auth: " +APITokenAuth + 
                         " apigranttype: " +APITokenGrantType + 
                        " apitokenscope: " +APITokenScope + 
                        " apitokenurl: " + APITokenURL;

                }
            }
            catch (Exception ex)
            {

                return "Token Service Execution"+ex.Message + ex.StackTrace;
            }

            
         }

        public string GetAPIResult1(string APIFundsTranferApiUrl, FormCollection collection, DataTable dtfundstransfer, string token, string accNo, string chequeNo)
        {
            var client = new RestClient("https://digitalbus-uat.bop.com.pk:8243/accountService/v1/getAcctInfo");
            RestRequest requestFT = new RestRequest("", Method.Post);
            requestFT.AddHeader("authorization", APIFTAuth);
            requestFT.AddHeader("content-type", "application/json");
            requestFT.AddHeader("username", APIUsername);
            requestFT.AddHeader("password", "esbuser");

            APIFundsTransfer clsapift = new APIFundsTransfer(dbContext);


            var varbody1 = new clsAccountInfo()
            {
                channelId= "ivr",
                accountNumber= "6510003179200019"

            };
            requestFT.AddParameter("application/json", JsonConvert.SerializeObject(varbody1), ParameterType.RequestBody);

            //requestFT.AddJsonBody(varbody1);

            try
            {
                startagain:
                var restResponse = client.ExecutePostAsync(requestFT);
                restResponse.Wait();
                var result1 = restResponse.Result;
                if (result1.StatusCode.ToString() == "Accepted")
                {
                    goto startagain;
                }
                if (result1.StatusCode == System.Net.HttpStatusCode.OK)
                {

                    JObject json = JObject.Parse(result1.Content);
                    List<JToken> results = json.Children().ToList();
                    List<JToken> endresult = results[1].Children().ToList();
                    return result1.Content.ToString();
                }
                else
                {
                    return result1.StatusCode + result1.StatusDescription + "GETAPIRESULT" + APIFTAuth + APIUsername + APIFundsTranferApiUrl;
                }

            }
            catch (Exception ex)
            {
                return ex.Message;
            }


        }


        public string GETAPIReponse(FormCollection collection, string accNo, string chequeno)
        {

            GetAPIParam();
            string vTokenApiEndPoint = APITokenURL;
            RestClient tokenapicall = new RestClient(vTokenApiEndPoint);
            string Message = "";
            //tokenapicall.timeout = -1;
            RestRequest requesttokencall = new RestRequest("token", Method.Post);
            requesttokencall.AddHeader("authorization", APITokenAuth);
            requesttokencall.AddHeader("content-type", "application/x-www-form-urlencoded");
            requesttokencall.AddParameter("grant_type", APITokenGrantType);
            requesttokencall.AddParameter("scope", APITokenScope);
            var resulttokencall = tokenapicall.ExecuteAsync(requesttokencall);
            try
            {
                //var resulttokencall = tokenapicall.ExecuteAsync(requesttokencall);
                var result = resulttokencall.Result;
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {

                    JObject json = JObject.Parse(result.Content);
                    List<JToken> results = json.Children().ToList();
                    List<JToken> BearerToken = results[0].Children().ToList();
                    APIFTAuth = "Bearer " + BearerToken[0].ToString();

                    RestClient FTAPICall = new RestClient(APIFundsTranferApiUrl);
                    RestRequest requestFT = new RestRequest("", Method.Post);
                    List<SqlParameter> sqlParameterFT = new List<SqlParameter>();

                    DataTable dtfundstransfer = dbContext.GetRecordsAsDataTableSP("spcgFundsTransfer", sqlParameterFT.ToArray());
                    #region Comment Code
                    //APIFundsTransfer clsapift = new APIFundsTransfer(dbContext);
                    //clsapift.transmissionDate = DateTime.Now.ToString("yyyyMMdd");// collection["fldClearDate"].ToString("yyyyMMdd");
                    //clsapift.transmissionTime = DateTime.Now.ToString("hhmmss"); //"112834";//"hhmmss"
                    //clsapift.dateLocalTran = DateTime.Now.ToString("yyyyMMdd");// collection["fldClearDate"].ToString("yyyyMMdd");
                    //clsapift.timeLocalTran = DateTime.Now.ToString("hhmmss"); //"112834";// "hhmmss"
                    //clsapift.stan = dtfundstransfer.Rows[0]["fldFTcounter"].ToString();
                    ////"000002";
                    //clsapift.rrn = dtfundstransfer.Rows[0]["fldFTcounter"].ToString(); //"000002";
                    //clsapift.acqInstCode = "623977";
                    //clsapift.fromBankIMD = "623977";
                    //clsapift.fromAccountNumber = accNo;// "6580008552700087";
                    //clsapift.fromAccountCurrency = "586";
                    //clsapift.fromAccountBranch = collection["fldIssueBankCode"].Substring(collection["fldIssueBankCode"].Length - 3); //Account Branch
                    //clsapift.toBankIMD = "623977";
                    //if (collection["fldIssueBankCode"].Substring(3, 2) == "20")
                    //{
                    //    clsapift.toAccountNumber = "608080129";

                    //}
                    //else
                    //{
                    //    clsapift.toAccountNumber = "608080130";

                    //}
                    //clsapift.toAccountBranch = "954";
                    //clsapift.transactionAmount = (collection["fldAmount"].ToString() + "00").PadLeft(12, '0');//"000000002000";
                    //clsapift.transactionCurrency = "586";
                    //clsapift.narrative = "DebitPostingAccountNumber:" + accNo + " " + DateTime.Now;
                    //requestFT.AddHeader("authorization", APIFTAuth);
                    //requestFT.AddHeader("content-type", "application/json");
                    //requestFT.AddHeader("username", APIUsername);
                    //requestFT.AddHeader("password", "esbuser");



                    //requestFT.AddJsonBody(new { relationshipId ="''" });
                    //requestFT.AddJsonBody(new { transmissionDate = clsapift.transmissionDate });
                    //requestFT.AddJsonBody(new { transmissionTime= clsapift.transmissionTime });
                    //requestFT.AddJsonBody(new { dateLocalTran = clsapift.dateLocalTran });
                    //requestFT.AddJsonBody(new { timeLocalTran =  clsapift.timeLocalTran });
                    //requestFT.AddJsonBody(new { stan = clsapift.stan });
                    //requestFT.AddJsonBody(new { rrn  = clsapift.rrn });
                    //requestFT.AddJsonBody(new { acqInstCode = clsapift.acqInstCode });
                    //requestFT.AddJsonBody(new { pinData ="" });
                    //requestFT.AddJsonBody(new { fromBankIMD = clsapift.fromBankIMD });
                    //requestFT.AddJsonBody(new { fromAccountNumber =clsapift.fromAccountNumber });
                    //requestFT.AddJsonBody(new { fromAccountType = "''", });
                    //requestFT.AddJsonBody(new { fromAccountCurrency =clsapift.fromAccountCurrency });
                    //requestFT.AddJsonBody(new { fromAccountBranch = clsapift.fromAccountBranch });
                    //requestFT.AddJsonBody(new { tranIndecator = "''"  });
                    //requestFT.AddJsonBody(new { toBankIMD = clsapift.toBankIMD});
                    //requestFT.AddJsonBody(new { toAccountNumber = clsapift.toAccountNumber });
                    //requestFT.AddJsonBody(new { toAccountBranch =clsapift.toAccountBranch });
                    //requestFT.AddJsonBody(new { beneficiaryIBAN = "''" });
                    //requestFT.AddJsonBody(new { transactionAmount =clsapift.transactionAmount });
                    //requestFT.AddJsonBody(new { transactionCurrency =clsapift.transactionCurrency});
                    //requestFT.AddJsonBody(new { purposeOfPayment = "0122"  });
                    //requestFT.AddJsonBody(new { narrative = clsapift.narrative });
                    //requestFT.AddJsonBody(new { cardAccepTermId = "''" });
                    #endregion
                    Message = GetAPIResult(APIFundsTranferApiUrl, collection, dtfundstransfer, BearerToken[0].ToString(), accNo, chequeno);


                    return Message;
                }
                else
                {
                    return resulttokencall.Result.StatusCode.ToString() + " 1 "
                        + resulttokencall.Result.StatusDescription.ToString() + " 2 "
                        + resulttokencall.Result.Content +
                        " apitoken auth: " + APITokenAuth +
                         " apigranttype: " + APITokenGrantType +
                        " apitokenscope: " + APITokenScope +
                        " apitokenurl: " + APITokenURL;

                }
            }
            catch (Exception ex)
            {

                return "Token Service Execution" + ex.Message + ex.StackTrace;
            }


        }


        public string GetAPIResult(string APIFundsTranferApiUrl, FormCollection collection, DataTable dtfundstransfer, string token, string accNo, string chequeNo)
        {
            var client = new RestClient(APIFundsTranferApiUrl);
            RestRequest requestFT = new RestRequest("", Method.Post);
            requestFT.AddHeader("authorization", APIFTAuth);
            requestFT.AddHeader("content-type", "application/json");
            requestFT.AddHeader("username", APIUsername);
            requestFT.AddHeader("password", "esbuser");

            APIFundsTransfer clsapift = new APIFundsTransfer(dbContext);
            clsapift.transmissionDate = DateTime.Now.ToString("yyyyMMdd");// collection["fldClearDate"].ToString("yyyyMMdd");
            clsapift.transmissionTime = DateTime.Now.ToString("hhmmss"); //"112834";//"hhmmss"
            clsapift.dateLocalTran = DateTime.Now.ToString("yyyyMMdd");// collection["fldClearDate"].ToString("yyyyMMdd");
            clsapift.timeLocalTran = DateTime.Now.ToString("hhmmss"); //"112834";// "hhmmss"
            clsapift.stan = dtfundstransfer.Rows[0]["fldFTcounter"].ToString();
            //"000002";
            clsapift.rrn = dtfundstransfer.Rows[0]["fldFTcounter"].ToString(); //"000002";
            clsapift.acqInstCode = "623977";
            clsapift.fromBankIMD = "623977";
            clsapift.fromAccountCurrency = "586";
            clsapift.fromAccountBranch = collection["fldIssueBranchCode"].Replace(",","").Trim();//.Substring(collection["fldIssueBankCode"].Length - 3); //Account Branch
            clsapift.toBankIMD = "623977";

            if (collection["fldTaskId"] == "306260")
            {
                if (collection["fldIssueStateCode"] == "20")
                {
                    clsapift.fromAccountNumber = "608080129";

                }
                else
                {
                    clsapift.fromAccountNumber = "608080130";

                }
                clsapift.toAccountNumber = accNo;// "6580008552700087";
                clsapift.fromAccountBranch = "954";
                clsapift.toAccountBranch = collection["fldIssueBranchCode"].Replace(",","").Trim();//.Substring(collection["fldIssueBankCode"].Length - 3);
            }
            else
            {
                clsapift.fromAccountNumber = accNo;// "6580008552700087";
                if (collection["fldIssueStateCode"] == "20")
                {
                    clsapift.toAccountNumber = "608080129";

                }
                else
                {
                    clsapift.toAccountNumber = "608080130";

                }

                clsapift.fromAccountBranch = collection["fldIssueBranchCode"].Replace(",","").Trim();//.Substring(collection["fldIssueBankCode"].Length - 3);
                clsapift.toAccountBranch = "954";

            }


            clsapift.transactionAmount = (collection["fldAmount"].ToString() + "00").PadLeft(12, '0');//"000000002000";
            clsapift.transactionCurrency = "586";
            clsapift.narrative = "DebitPostingAccountNumber:" + accNo + " " + DateTime.Now;
            clsapift.purposeOfPayment = "0122";
            clsapift.fromAccountCurrency = "586";

            var varbody1 = new
            {
                relationshipId = "",
                transmissionDate = clsapift.transmissionDate,
                transmissionTime = clsapift.transmissionTime,
                dateLocalTran = clsapift.dateLocalTran,
                timeLocalTran = clsapift.timeLocalTran,
                stan = clsapift.stan,
                rrn = clsapift.rrn,
                acqInstCode = clsapift.acqInstCode,
                pinData = "",
                fromBankIMD = clsapift.fromBankIMD,
                fromAccountNumber = clsapift.fromAccountNumber, 
                fromAccountType = "",
                fromAccountCurrency = clsapift.fromAccountCurrency,
                fromAccountBranch = clsapift.fromAccountBranch,
                tranIndecator = "",
                toBankIMD = clsapift.fromBankIMD,
                toAccountNumber = clsapift.toAccountNumber,
                toAccountBranch = clsapift.toAccountBranch,
                beneficiaryIBAN = "",
                transactionAmount = clsapift.transactionAmount,
                transactionCurrency = clsapift.transactionCurrency,
                purposeOfPayment = clsapift.purposeOfPayment,
                narrative = clsapift.narrative,
                cardAccepTermId = ""


            };

            requestFT.AddParameter("application/json", JsonConvert.SerializeObject(varbody1), ParameterType.RequestBody);

            //var varbody1 = new
            //{
            //    relationshipId = "",
            //    transmissionDate = "20230803",
            //    transmissionTime = "094156",
            //    dateLocalTran = "20230803",
            //    timeLocalTran = "094156",
            //    stan = "000399",
            //    rrn = "000399",
            //    acqInstCode = "623977",
            //    pinData = "",
            //    fromBankIMD = "623977",
            //    fromAccountNumber = "6580003315500019",
            //    fromAccountType = "",
            //    fromAccountCurrency = "586",
            //    fromAccountBranch = "002",
            //    tranIndecator = "",
            //    toBankIMD = "623977",
            //    toAccountNumber = "608080130",
            //    toAccountBranch = "954",
            //    beneficiaryIBAN = "",
            //    transactionAmount = "000001391600",
            //    transactionCurrency = "586",
            //    purposeOfPayment = "0122",
            //    narrative = "Testing",
            //    cardAccepTermId = ""


            //};

            //requestFT.AddJsonBody(varbody1);

            try
            {
                var restResponse = client.ExecuteAsync(requestFT);
                restResponse.Wait();
                var result1 = restResponse.Result;
                if (result1.StatusCode == System.Net.HttpStatusCode.OK)
                {

                    JObject json = JObject.Parse(result1.Content);
                    List<JToken> results = json.Children().ToList();
                    List<JToken> endresult = results[1].Children().ToList();
                    List<JToken> error011 = results[0].Children().ToList();
                    List<JToken> transactionLogId = results[3].Children().ToList();


                    List<SqlParameter> sqlParameterFTransfer = new List<SqlParameter>();
                    sqlParameterFTransfer.Add(new SqlParameter("@fldftcounter ", clsapift.stan));
                    sqlParameterFTransfer.Add(new SqlParameter("@fldAccountNumber", accNo));
                    sqlParameterFTransfer.Add(new SqlParameter("@fldChequeSerialNo", chequeNo));
                    sqlParameterFTransfer.Add(new SqlParameter("fldTransactionLogId", transactionLogId[3].ToString()));
                    DataTable dtROWS = dbContext.GetRecordsAsDataTableSP("spciFundsTransfer", sqlParameterFTransfer.ToArray());
                    if (error011[0].ToString() == "011")
                    {
                        return "Service Responded 011 Code";
                    }

                    return endresult[0].ToString();
                }
                else
                {
                    return result1.StatusCode + result1.StatusDescription + "GETAPIRESULT" + APIFTAuth + APIUsername + APIFundsTranferApiUrl;
                }

            }
            catch (Exception ex)
            {

                return "API AFTER TOKEN: " + ex.Message + ex.StackTrace + ex.InnerException + ex.Data +
                       "URL: " + APIFundsTranferApiUrl
                     + " relationshipId: " + varbody1.relationshipId
                     + " transmissionDate: " + varbody1.transmissionDate
                     + " transmissionTime: " + varbody1.transmissionTime
                     + " dateLocalTran: " + varbody1.dateLocalTran
                     + " timeLocalTran: " + varbody1.timeLocalTran
                     + " stan: " + varbody1.stan
                     + " rnn: " + varbody1.rrn
                     + "acqInstCode: " + varbody1.acqInstCode
                     + "pinData:" + varbody1.acqInstCode
                     + "fromBankIMD: " + varbody1.fromBankIMD
                     + "fromAccountNumber:" + varbody1.fromAccountNumber
                     + "fromAccountType: " + varbody1.fromAccountType
                     + "fromAccountCurrency: " + varbody1.fromAccountCurrency
                     + "fromAccountBranch: " + varbody1.fromAccountBranch
                     + "tranIndecator:" + varbody1.tranIndecator
                     + "toBankIMD: " + varbody1.fromBankIMD
                     + "toAccountNumber: " + varbody1.toAccountNumber
                     + "toAccountBranch: " + varbody1.toAccountBranch
                     + "beneficiaryIBAN: " + varbody1.beneficiaryIBAN
                     + "transactionAmount: " + varbody1.transactionAmount
                     + "transactionCurrency: " + varbody1.transactionCurrency
                     + "purposeOfPayment: " + varbody1.purposeOfPayment
                     + "narrative: " + varbody1.narrative
                     + "cardAccepTermId: " + varbody1.narrative
                     + "cardAccepTermId: " + APIFTAuth

                     ;



            }


        }


        protected string GetAPIParam()
        {
            try
            {
                DataTable dtTokenURL = getAPIRequestInfo("TokenURLFundsTransfer");
                DataTable dtTokenAuth = getAPIRequestInfo("Authorization");
                DataTable dtTokenGrantType = getAPIRequestInfo("GrantType");
                DataTable dtTokenscope = getAPIRequestInfo("ScopeFinancial");
                DataTable dtFundsTransfer = getAPIRequestInfo("FundsTransfer");
                DataTable dtUserName = getAPIRequestInfo("username");
                DataTable dtPassword = getAPIRequestInfo("password");
                if (dtTokenURL.Rows.Count == 0
                    || dtTokenAuth.Rows.Count == 0
                    || dtTokenGrantType.Rows.Count == 0
                    || dtTokenscope.Rows.Count == 0
                    || dtFundsTransfer.Rows.Count == 0
                    || dtUserName.Rows.Count == 0
                    || dtPassword.Rows.Count == 0
                    )
                {
                    return "TokenURL/TokenAuth/TokenGrantType/TokenScope/FundsTransferURL/Username/Password cannot be found.";
                }
                else
                {
                    if (dtTokenURL.Rows.Count > 0)
                    {

                        APITokenURL = dtTokenURL.Rows[0][0].ToString();
                    }
                    if (dtTokenAuth.Rows.Count > 0)
                    {

                        APITokenAuth = dtTokenAuth.Rows[0][0].ToString();
                    }
                    if (dtTokenGrantType.Rows.Count > 0)
                    {

                        APITokenGrantType = dtTokenGrantType.Rows[0][0].ToString();
                    }
                    if (dtTokenscope.Rows.Count > 0)
                    {
                        APITokenScope = dtTokenscope.Rows[0][0].ToString();
                    }
                    if (dtFundsTransfer.Rows.Count > 0)
                    {
                        APIFundsTranferApiUrl = dtFundsTransfer.Rows[0][0].ToString();

                    }
                    if (dtUserName.Rows.Count > 0)
                    {
                        APIUsername = dtUserName.Rows[0][0].ToString();

                    }
                    if (dtPassword.Rows.Count > 0)
                    {
                        APIPassword = dtPassword.Rows[0][0].ToString();

                    }

                }
                return "";
            }
            catch (Exception ex)
            {

                return "GetAPIParam"+ex.Message;
            }
        }

        public DataTable getAPIRequestInfo(string Code)
        {
            DataTable dtResult = null/* TODO Change to default(_) if this is not a reference type */;
            try
            {
                bool boolResult = true;
                string strquery;
                strquery = "select fldrequestPath from tblWebApiRequestType where fldRequestName = '" + Code.Trim() + "'";
                dtResult = dbContext.GetRecordsAsDataTable(strquery);// cls_query.fct_sqlGetDataTable(conn, strquery, boolResult);
                if (boolResult == false)
                    dtResult = null/* TODO Change to default(_) if this is not a reference type */;
            }
            catch (Exception ex)
            {
                string _strErrorMessage = "[getAPIRequestInfo]:" + ex.Message;
                dtResult = null/* TODO Change to default(_) if this is not a reference type */;
                throw ex;
            }
            return dtResult;
        }






    }
}