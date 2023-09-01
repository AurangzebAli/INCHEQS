using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.DataAccessLayer;
using INCHEQS.Areas.ICS.Models.ICSAPIDebitPosting;
using Newtonsoft.Json;

namespace INCHEQS.Areas.ICS.Models.ICSAPIHostValidation
{
    public class APIHostValidation
    {
        private readonly ApplicationDbContext dbContext;
        public APIHostValidation(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;

        }



        #region API Parameters
        public string APIUsername { get; set; }
        public string APIPassword { get; set; }
        public string APIAccountInfoApiUrl { get; set; }
        public string APIAccountInfoAuth { get; set; }
        public string APITokenURL { get; set; }
        public string APITokenAuth { get; set; }
        public string APITokenGrantType { get; set; }
        public string APITokenScope { get; set; }

        public string APIFetchInstrumentDetail { get; set; }


        #endregion

        protected string GetAPIParam()
        {
            try
            {
                DataTable dtTokenURL = getAPIRequestInfo("TokenURLInfo");
                DataTable dtTokenAuth = getAPIRequestInfo("Authorization");
                DataTable dtTokenGrantType = getAPIRequestInfo("GrantType");
                DataTable dtTokenscope = getAPIRequestInfo("ScopeAccountInfo");
                DataTable dtAccountInfo = getAPIRequestInfo("AccountInfo");
                DataTable dtUserName = getAPIRequestInfo("username");
                DataTable dtPassword = getAPIRequestInfo("password");
                DataTable dtFetchInstrumentDetail = getAPIRequestInfo("FetchInstrumentDetail");

                if (dtTokenURL.Rows.Count == 0
                    || dtTokenAuth.Rows.Count == 0
                    || dtTokenGrantType.Rows.Count == 0
                    || dtTokenscope.Rows.Count == 0
                    || dtAccountInfo.Rows.Count == 0
                    || dtUserName.Rows.Count == 0
                    || dtPassword.Rows.Count == 0
                    || dtFetchInstrumentDetail.Rows.Count == 0
                    
                    )
                {
                    return "TokenURL/TokenAuth/TokenGrantType/TokenScope/AccountInfo/FetchInstrumentURL/Username/Password cannot be found.";
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
                    if (dtAccountInfo.Rows.Count > 0)
                    {
                        APIAccountInfoApiUrl = dtAccountInfo.Rows[0][0].ToString();

                    }
                    if (dtUserName.Rows.Count > 0)
                    {
                        APIUsername = dtUserName.Rows[0][0].ToString();

                    }
                    if (dtPassword.Rows.Count > 0)
                    {
                        APIPassword = dtPassword.Rows[0][0].ToString();

                    }
                    if (dtFetchInstrumentDetail.Rows.Count > 0)
                    {
                        APIFetchInstrumentDetail = dtFetchInstrumentDetail.Rows[0][0].ToString();

                    }
                    
                }
                return "";
            }
            catch (Exception ex)
            {

                return "GetAPIParam" + ex.Message;
            }
        }

        public Dictionary<string, string> GETAPIReponse(FormCollection collection, string accNo, string chequeno,string amount)
        {

            GetAPIParam();
            string vTokenApiEndPoint = APITokenURL;
            RestClient tokenapicall = new RestClient(vTokenApiEndPoint);
            //string Message = "";
            //tokenapicall.timeout = -1;
            RestRequest requesttokencall = new RestRequest("", Method.Post);
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
                    APIAccountInfoAuth = "Bearer " + BearerToken[0].ToString();

                    RestClient FTAPICall = new RestClient(APIAccountInfoApiUrl);
                    RestRequest requestFT = new RestRequest("", Method.Post);
                    List<SqlParameter> sqlParameterFT = new List<SqlParameter>();

                    Dictionary<string,string> message= GetAPIResult(APIAccountInfoApiUrl, collection,  BearerToken[0].ToString(), accNo, chequeno, amount);



                    return message;
                }
                else
                {
                    try
                    {
                        Dictionary<string, string> messaeDictionary = new Dictionary<string, string>();

                        string message = resulttokencall.Result.StatusCode.ToString() + " 1 "
                            + Convert.ToString( resulttokencall.Result.StatusDescription) + " 2 "
                            + resulttokencall.Result.Content +
                            " apitoken auth: " + APITokenAuth +
                             " apigranttype: " + APITokenGrantType +
                            " apitokenscope: " + APITokenScope +
                            " apitokenurl: " + APITokenURL;

                    

                    return messaeDictionary;
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                Dictionary<string, string> messaeException = new Dictionary<string, string>();
                messaeException.Add("Exception", "Token Service Execution" + ex.Message + ex.StackTrace);

                return messaeException;
            }


        }

        public Dictionary<string, string> GetAPIResult(string APIAccountInfoApiUrl, FormCollection collection, string token, string accNo, string chequeNo,string amount)
        {
            var client = new RestClient(APIAccountInfoApiUrl);
            RestRequest requestAccountInfo = new RestRequest("", Method.Post);
            requestAccountInfo.AddHeader("authorization", APIAccountInfoAuth);
            requestAccountInfo.AddHeader("content-type", "application/json");
            requestAccountInfo.AddHeader("username", APIUsername);
            requestAccountInfo.AddHeader("password", "esbuser");
            Dictionary<string, string> APIError = new Dictionary<string, string>();

            APIFundsTransfer clsapift = new APIFundsTransfer(dbContext);


            var varbody1 = new clsAccountInfo()
            {
                channelId = "ivr",
                accountNumber = accNo

            };
            requestAccountInfo.AddParameter("application/json", JsonConvert.SerializeObject(varbody1), ParameterType.RequestBody);

            //requestFT.AddJsonBody(varbody1);

            try
            {
            startagain:
                var restResponse = client.ExecutePostAsync(requestAccountInfo);
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


                    JObject jsonB = JObject.Parse(result1.Content);
                    List<JToken> resultsB = jsonB.Children().ToList();
                    List<JToken> ResponseCode = resultsB[0].Children().ToList();
                    if ((ResponseCode[0].ToString() == "000000"))
                    {
                        List<JToken> Lstresults = resultsB[2].Children().ToList();
                        List<JToken> splitedval = Lstresults[0].ToList();
                        List<JToken> ACStatNoDR = splitedval[9].Children().ToList();
                        List<JToken> AvailableBalance = splitedval[10].Children().ToList();
                        List<JToken> Priority = splitedval[11].Children().ToList();
                        List<JToken> NSFStatus = splitedval[12].Children().ToList();
                        List<JToken> CreditBlock = splitedval[13].Children().ToList();
                        List<JToken> AccountClosed = splitedval[14].Children().ToList();
                        List<JToken> StopPayment = splitedval[15].Children().ToList();
                        List<JToken> Dormant = splitedval[16].Children().ToList();
                        List<JToken> OpenClose = splitedval[17].Children().ToList();
                        List<JToken> Frozen = splitedval[18].Children().ToList();
                        List<JToken> Deceased = splitedval[19].Children().ToList();
                        List<JToken> ResidentStatus = splitedval[20].Children().ToList();
                        var resultFetchinstrumentdetail =GetAPIInstrumentDetail(accNo, chequeNo);
                        if (Convert.ToInt32(resultFetchinstrumentdetail.StatusCode) == 200)
                        {
                            JObject jsonIns = JObject.Parse(resultFetchinstrumentdetail.Content);
                            List<JToken> resultsIns = jsonIns.Children().ToList();
                            List<JToken> ResponseCodeIns = resultsIns[0].Children().ToList();
                            if ((ResponseCodeIns[0].ToString() == "000000"))
                            {
                                List<JToken> LstInsresults = resultsIns[2].Children().ToList();
                                List<JToken> Inssplitedval = LstInsresults[0].ToList();
                                List<JToken> ChequeUsedStatus = Inssplitedval[2].Children().ToList();
                                double vChequeAmount = Convert.ToDouble(amount);
                                if (Convert.ToDouble(AvailableBalance[0]) < vChequeAmount
                                    || NSFStatus[0].ToString() == "Y" || CreditBlock[0].ToString() == "Y"
                                    || AccountClosed[0].ToString() == "Y" || StopPayment[0].ToString() == "Y" || Dormant[0].ToString() == "Y"
                                    || OpenClose[0].ToString() == "C"
                                    || Frozen[0].ToString() == "Y"
                                    || Deceased[0].ToString() == "Y" || ResidentStatus[0].ToString() == "NR" ||
                                    ResidentStatus[0].ToString() == "not available"
                                    || ChequeUsedStatus[0].ToString() == "U")
                                {
                                    Dictionary<string,string> APIBranchResponse = new Dictionary<string, string>();
                                    APIBranchResponse.Add("CCUandB", "B");
                                    APIBranchResponse.Add("Balance",AvailableBalance[0].ToString());
                                    APIBranchResponse.Add("NSFStatus", NSFStatus[0].ToString());
                                    APIBranchResponse.Add("CreditBlock", CreditBlock[0].ToString());
                                    APIBranchResponse.Add("AccountClosed", AccountClosed[0].ToString());
                                    APIBranchResponse.Add("StopPayment", StopPayment[0].ToString());
                                    APIBranchResponse.Add("Dormant", Dormant[0].ToString());
                                    APIBranchResponse.Add("OpenClose", OpenClose[0].ToString());
                                    APIBranchResponse.Add("Frozen", Frozen[0].ToString());
                                    APIBranchResponse.Add("Deceased", Deceased[0].ToString());
                                    APIBranchResponse.Add("ResidentStatus", ResidentStatus[0].ToString());
                                    APIBranchResponse.Add("ChequeUsedStatus", ChequeUsedStatus[0].ToString());
                                    APIBranchResponse.Add("fldHostValidationHttpCode", Convert.ToInt32(resultFetchinstrumentdetail.StatusCode).ToString());

                                    return APIBranchResponse;
                                }
                                else
                                {
                                    Dictionary<string, string> APICCUResponse = new Dictionary<string, string>();
                                    APICCUResponse.Add("CCUandB", "CCU");
                                    APICCUResponse.Add("Balance", AvailableBalance[0].ToString());
                                    APICCUResponse.Add("NSFStatus", NSFStatus[0].ToString());
                                    APICCUResponse.Add("CreditBlock", CreditBlock[0].ToString());
                                    APICCUResponse.Add("AccountClosed", AccountClosed[0].ToString());
                                    APICCUResponse.Add("Dormant", Dormant[0].ToString());
                                    APICCUResponse.Add("OpenClose", OpenClose[0].ToString());
                                    APICCUResponse.Add("Frozen", Frozen[0].ToString());
                                    APICCUResponse.Add("Deceased", Deceased[0].ToString());
                                    APICCUResponse.Add("ResidentStatus", ResidentStatus[0].ToString());
                                    APICCUResponse.Add("ChequeUsedStatus", ChequeUsedStatus[0].ToString());
                                    APICCUResponse.Add("fldHostValidationHttpCode", Convert.ToInt32(resultFetchinstrumentdetail.StatusCode).ToString());

                                    return APICCUResponse;
                                }




                            }
                            else
                            {

                                
                                Dictionary<string, string> APIRRUResponse = new Dictionary<string, string>();
                                APIRRUResponse .Add("CCUandB", "");
                                APIRRUResponse .Add("Balance", "");
                                APIRRUResponse .Add("NSFStatus", "");
                                APIRRUResponse .Add("CreditBlock", "");
                                APIRRUResponse .Add("AccountClosed", "");
                                APIRRUResponse .Add("Dormant", "");
                                APIRRUResponse .Add("OpenClose", "");
                                APIRRUResponse .Add("Frozen", "");
                                APIRRUResponse .Add("Deceased", "");
                                APIRRUResponse .Add("ResidentStatus", "");
                                APIRRUResponse .Add("ChequeUsedStatus", "");
                                APIRRUResponse.Add("fldHostValidationHttpCode", Convert.ToInt32(resultFetchinstrumentdetail.StatusCode).ToString());
                                APIRRUResponse.Add("ResponseCode", ResponseCodeIns[0].ToString() + resultsIns[1].Children().ToList().ToString());

                                return APIRRUResponse;

                                    
                            }
                        }


                    }
                    else
                    {
                        APIError.Add("CCUandB", "No Data Found in Core Banking While Calling Host Validation");
                        return APIError;
                    }
                }
                else
                {
                    APIError.Add("CCUandB", result1.StatusCode + result1.StatusDescription + "GETAPIRESULT" + APIAccountInfoAuth + APIUsername + APIAccountInfoApiUrl);

                    return APIError;
                }

            }
            catch (Exception ex)
            {
                APIError.Add("CCUandB", ex.Message);
                return APIError;
            }

            return APIError;
        }

        public RestResponse GetAPIInstrumentDetail(string accNo, string chequeNo)
        {
            var client = new RestClient(APIFetchInstrumentDetail);
            RestRequest requestInstrumentDetail = new RestRequest("", Method.Post);
            requestInstrumentDetail.AddHeader("authorization", APIAccountInfoAuth);
            requestInstrumentDetail.AddHeader("content-type", "application/json");
            requestInstrumentDetail.AddHeader("username", APIUsername);
            requestInstrumentDetail.AddHeader("password", "esbuser");

            var varbody1 = new clsInstrumentDetail()
            {
                channelId = "ivr",
                instrumentType = "000",
                instrumentNo = chequeNo,
                accountNumber = accNo
               
            };
            requestInstrumentDetail.AddParameter("application/json", JsonConvert.SerializeObject(varbody1), ParameterType.RequestBody);
            startagain:
            var restResponse = client.ExecutePostAsync(requestInstrumentDetail);
            restResponse.Wait();
            var result1 = restResponse.Result;
            if (result1.StatusCode.ToString() == "Accepted")
            {
                goto startagain;
            }
            
                return result1;

            
            
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