using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Management;
using System.Data;
using System.Reflection;
using Newtonsoft.Json;

namespace SagoBikers.Models
{
    public class Baseclass
    {
        public static RQRS.ResponseData InvokeServiceReq(string MethodName, string ParamInput, string MethodType)
        {
            string strResponse = string.Empty;
            RQRS.ResponseData Response = new RQRS.ResponseData();
            try
            {
                string ServiceURL = ConfigurationManager.AppSettings["SERVICEURL"].ToString().Trim() + MethodName;
                var client = new RestClient(ServiceURL);
                client.Timeout = -1;
                var request = new RestRequest(MethodType == "POST" ? Method.POST : Method.GET);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", ParamInput, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                strResponse = response.Content;
                Response = JsonConvert.DeserializeObject<RQRS.ResponseData>(strResponse);
            }
            catch (Exception ex)
            {
                {
                    Response.strStatus = "00";
                    Response.strResponse = "Unable to connect remote service";
                }
            }
            return Response;
        }
    }
    public class CheckSessionFilter : ActionFilterAttribute
    {

        public override void OnActionExecuting(System.Web.Mvc.ActionExecutingContext filterContext)
        {
            string strUserType = HttpContext.Current.Session["userId"] != null && HttpContext.Current.Session["userId"].ToString() != "" ? HttpContext.Current.Session["userId"].ToString() : "";
            if (strUserType == "")
            {
                HttpContext.Current.Session.Clear();
                filterContext.HttpContext.Response.Redirect("/Home/Login?Exp=Y", true);
            }
        }

    }

    public class CommonFunctions
    {
        public static string GetWebBrowserName()
        {
            string WebBrowserName = string.Empty;
            try
            {
                WebBrowserName = HttpContext.Current.Request.Browser.Browser;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return WebBrowserName;
        }

        public static string GetDevId()
        {
            string strNo = "";
            ManagementObjectSearcher bios = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
            ManagementObjectCollection bios_Collection = bios.Get();
            foreach (ManagementObject obj in bios_Collection)
            {
                strNo = obj["SerialNumber"].ToString();
                break; //break just to get the first found object data only
            }

            return strNo;
        }
        public static string EncryptString(string _securityKey, string PlainText)

        {
            byte[] toEncryptedArray = UTF8Encoding.UTF8.GetBytes(PlainText);
            MD5CryptoServiceProvider objMD5CryptoService = new MD5CryptoServiceProvider();
            byte[] securityKeyArray = objMD5CryptoService.ComputeHash(UTF8Encoding.UTF8.GetBytes(_securityKey));
            objMD5CryptoService.Clear();
            var objTripleDESCryptoService = new TripleDESCryptoServiceProvider();
            objTripleDESCryptoService.Key = securityKeyArray;
            objTripleDESCryptoService.Mode = CipherMode.ECB;
            objTripleDESCryptoService.Padding = PaddingMode.PKCS7;
            var objCrytpoTransform = objTripleDESCryptoService.CreateEncryptor();
            byte[] resultArray = objCrytpoTransform.TransformFinalBlock(toEncryptedArray, 0, toEncryptedArray.Length);
            objTripleDESCryptoService.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string DecryptString(string _securityKey, string CipherText)
        {
            byte[] toEncryptArray = Convert.FromBase64String(CipherText);
            MD5CryptoServiceProvider objMD5CryptoService = new MD5CryptoServiceProvider();
            byte[] securityKeyArray = objMD5CryptoService.ComputeHash(UTF8Encoding.UTF8.GetBytes(_securityKey));
            objMD5CryptoService.Clear();
            var objTripleDESCryptoService = new TripleDESCryptoServiceProvider();
            objTripleDESCryptoService.Key = securityKeyArray;
            objTripleDESCryptoService.Mode = CipherMode.ECB;
            objTripleDESCryptoService.Padding = PaddingMode.PKCS7;
            var objCrytpoTransform = objTripleDESCryptoService.CreateDecryptor();
            byte[] resultArray = objCrytpoTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            objTripleDESCryptoService.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        public static DataTable ConvertToDataTable<T>(IEnumerable<T> varlist)
        {
            DataTable dtReturn = new DataTable();

            // column names   
            PropertyInfo[] oProps = null;

            if (varlist == null) return dtReturn;

            foreach (T rec in varlist)
            {
                // Use reflection to get property names, to create table, Only first time, others will follow   
                if (oProps == null)
                {
                    oProps = ((Type)rec.GetType()).GetProperties();
                    foreach (PropertyInfo pi in oProps)
                    {
                        Type colType = pi.PropertyType;

                        if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }

                        dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
                    }
                }

                DataRow dr = dtReturn.NewRow();

                foreach (PropertyInfo pi in oProps)
                {
                    dr[pi.Name] = pi.GetValue(rec, null) == null ? DBNull.Value : pi.GetValue
                    (rec, null);
                }

                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }
    }

    public class GlobalValues
    {
        public static string strUserID = Convert.ToString(HttpContext.Current.Session["userId"]);
    }

    public class ConfigValue
    { 
        public static string strDomainURL = ConfigurationManager.AppSettings["DomainURL"] != null ? ConfigurationManager.AppSettings["DomainURL"].ToString() : "";
        public static int intPOSTLOADINGCOUNT = ConfigurationManager.AppSettings["POSTLOADINGCOUNT"] != null ? Convert.ToInt32(ConfigurationManager.AppSettings["POSTLOADINGCOUNT"]) : 0;
        
    }

}