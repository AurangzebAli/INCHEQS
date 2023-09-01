using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using System.Xml;
using System.Data;

namespace INCHEQS.Areas.ICS.Service
{
    public class SVSSOAPRequest
    {

        public String InvokeService(String AccountNumber, String URLLink)
        {
            //DataTable dtSVS = new DataTable();
            String strXmlSVS = "";
            //Calling CreateSOAPWebRequest method  
            HttpWebRequest request = CreateSOAPWebRequest(URLLink);

            XmlDocument SOAPReqBody = new XmlDocument();
            //SOAP Body Request  

            String xmlRequestSOAP = @"<?xml version=""1.0"" encoding=""utf-8""?>
            < soap:Envelope xmlns:soap = ""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-   instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">  
             <soap:Body>
                 < Addition xmlns = ""http://tempuri.org/"">  
                  <AccountNo> " + AccountNumber + @" </AccountNo>
                </ Addition >
              </ soap:Body >
             </ soap:Envelope > ";

            SOAPReqBody.LoadXml(xmlRequestSOAP);

          using (Stream stream = request.GetRequestStream())
            {
                SOAPReqBody.Save(stream);
            }
            //Geting response from request  
            using (WebResponse Serviceres = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(Serviceres.GetResponseStream()))
                {
                    //reading stream  
                    var ServiceResult = rd.ReadToEnd();
                    //writting stream result on console  
                    //Console.WriteLine(ServiceResult);
                    //Console.ReadLine();
                    strXmlSVS = ServiceResult;
                    return strXmlSVS;
                }
            }
        }



        public HttpWebRequest CreateSOAPWebRequest(String URLWebservice)
        {
            //Making Web Request  
            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(URLWebservice);
            //SOAPAction  
            Req.Headers.Add(@"SOAPAction:http://tempuri.org/Addition");
            //Content_type  
            Req.ContentType = "text/xml;charset=\"utf-8\"";
            Req.Accept = "text/xml";
            //HTTP method  
            Req.Method = "POST";
            //return HttpWebRequest  
            return Req;
        }

    }
}