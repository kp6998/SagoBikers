using Newtonsoft.Json;
using SagoBikers.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using static SagoBikers.Models.RQRS;

namespace SagoBikers.Controllers
{
    [RoutePrefix("")]
    public class HomeController : Controller
    {
        [AllowAnonymous]
        [Route("")]

        public ActionResult Login()
        {
            return View();
        }

        [Route("SignUp")]
        [AllowAnonymous]
        public ActionResult SignUp()
        {
            return View();
        }

        [Route("Feeds")]
        public ActionResult HomePage()
        {
            return View();
        }

        [Route("Post")]
        public ActionResult ImageUpload()
        {
            return View();
        }

        [Route("Chat")]
        public ActionResult ChatList()
        {
            return View();
        }
        [Route("Profile/{username}")]
        public ActionResult Profile(string username)
        {
            return View();
        }
        [Route("ProfileEdit")]
        public ActionResult ProfileEdit()
        {
            return View();
        }
        [Route("AddBikes")]
        public ActionResult AddBikes()
        {
            return View();
        }
        [Route("Clubs")]
        public ActionResult Clubs()
        {
            return View();
        }
        [Route("Notification")]
        public ActionResult Notification()
        {
            return View();
        }
        [Route("Shops")]
        public ActionResult Shops()
        {
            return View();
        }
        public string Logout()
        {
            Session.Clear();
            return "Logged out successfully";
        }
        [Route("FindUser")]
        public ActionResult Search()
        {
            return View();
        }
        public ActionResult GetUserId()
        {
            string getUserId = Convert.ToString(Session["userId"]);
            string proPicPath = ConfigValue.strDomainURL + "Files/ProfilePic/";
            return Json(new { GetUserId = getUserId, ProPicPath = proPicPath });
        }
        public ActionResult GetUsername()
        {
            string getUsername = Convert.ToString(Session["userName"]);
            return Json(new { GetUsername = getUsername });
        }

        public ActionResult RegistrationLogin(string strEmail, string strMobileNo, string strFullname, string strUsername, string strPassword, string strFlag, string strLgnTkn)
        {

            string strStatus = string.Empty;
            string strMessage = string.Empty;
            try
            {
                string struserID = string.Empty;
                string strLgnByPass = "N";
                #region Check Login Bypass
                if (!string.IsNullOrEmpty(strLgnTkn))
                {
                    string strBrowName = CommonFunctions.GetWebBrowserName();
                    string strDevID = CommonFunctions.GetDevId();
                    string[] LoginCredentials = CommonFunctions.DecryptString((strDevID + strBrowName), strLgnTkn).Split('|');
                    struserID = LoginCredentials[0];
                    strPassword = LoginCredentials[1];
                    strLgnByPass = "Y";
                }
                #endregion
                #region Request
                RQRS.Reg_and_Login login_cred = new RQRS.Reg_and_Login();
                login_cred.strEmail = strEmail;
                login_cred.strMobileNo = strMobileNo;
                login_cred.fullname = strFullname;
                login_cred.username = strUsername;
                login_cred.password = strPassword;
                login_cred.userId = strLgnByPass == "Y" ? struserID : "";
                login_cred.flag = strLgnByPass == "Y" ? "LB" : strFlag;
                string strRequest = JsonConvert.SerializeObject(login_cred);
                RQRS.ResponseData Response = Baseclass.InvokeServiceReq("API/Reg_and_Login_srvc", strRequest, "POST");
                #endregion
                #region Response Handling
                if (Response != null)
                {
                    strStatus = Response.strStatus;
                    strMessage = Response.strResponse;
                    if ((strFlag == "L" || strFlag == "LB") && Response.strStatus == "01")
                    {
                        DataTable dt = JsonConvert.DeserializeObject<DataTable>(Response.strResponse);
                        Session["userName"] = dt.Rows[0]["USERNAME"];
                        Session["userId"] = dt.Rows[0]["USERID"];
                        //strMessage = Convert.ToString(dt.Rows[0]["USERID"]) + " " + Convert.ToString(dt.Rows[0]["USERNAME"]) + " " + Convert.ToString(dt.Rows[0]["FULLNAME"]);
                        if (string.IsNullOrEmpty(strLgnTkn))
                        {
                            string strBrowName = CommonFunctions.GetWebBrowserName();
                            string strDevID = CommonFunctions.GetDevId();
                            strLgnTkn = CommonFunctions.EncryptString((strDevID + strBrowName), (dt.Rows[0]["USERID"].ToString() + "|" + strPassword));
                        }
                    }
                }
                else
                {
                    strStatus = "00";
                    strMessage = "Unable to process your request";
                }
                #endregion
            }
            catch (Exception ex)
            {
                strStatus = "00";
                strMessage = "Problem occured while processing your request.";
            }

            return Json(new { Status = strStatus, Message = strMessage, Token = strLgnTkn });
        }

        public ActionResult CheckUserName(string strUserName)
        {
            bool Result = false;
            try
            {
                RQRS.Reg_and_Login login_cred = new RQRS.Reg_and_Login();
                login_cred.username = strUserName;
                login_cred.flag = "C";
                string strRequest = JsonConvert.SerializeObject(login_cred);
                RQRS.ResponseData Response = Baseclass.InvokeServiceReq("API/CheckUserName", strRequest, "POST");
                Result = Response.strStatus == "01";
            }
            catch (Exception)
            {
                Result = false;
            }
            return Json(new { result = Result});
        }
        public ActionResult GetPosts(string otherUserId, int exisitPostCount, string strFlag)
        {
            string strStatus = string.Empty;
            string strMessage = string.Empty;
            string strResponse = string.Empty;
            string getUserId = Convert.ToString(Session["userId"]);
            string postPath = ConfigValue.strDomainURL + "Files/Post/";
            string proPicPath = ConfigValue.strDomainURL + "Files/ProfilePic/";
            try
            {
                RQRS.PostRequest postRequest = new RQRS.PostRequest();
                postRequest.strUserId = getUserId;
                postRequest.strOtherUserId = otherUserId;
                postRequest.startWith = exisitPostCount + 1;
                postRequest.EndWith= exisitPostCount + ConfigValue.intPOSTLOADINGCOUNT;
                postRequest.strFlag = strFlag;
                string strRequest = JsonConvert.SerializeObject(postRequest);
                RQRS.ResponseData Response = Baseclass.InvokeServiceReq("API/Posts", strRequest, "POST");
                if (Response != null && Response.strStatus == "01")
                {
                    strStatus = Response.strStatus;
                    strMessage = Response.strResponse;
                }
                else
                {
                    strStatus = Response.strStatus;
                    strMessage = Response.strResponse;
                }
            }
            catch (Exception ex)
            {
                strStatus = "00";
                strMessage = "Unable to connect Service(#05)";
            }

            return Json(new { Status = strStatus, Message = strMessage, Path = postPath, ProPicPath = proPicPath });
        }
        
        public ActionResult UploadPost(string postContent)
        {
            string strStatus = string.Empty;
            string strMessage = string.Empty;
            string filePathRef = string.Empty;
            try
            {
                string strPostID = string.Empty;
                string strResponse = string.Empty;
                string strFileName = string.Empty;
                string filename = string.Empty;
                string getUserId = Convert.ToString(Session["userId"]);
                HttpFileCollectionBase files = Request.Files;
                HttpPostedFileBase file = files[0];
                string strExtention = file.FileName.Split('.')[file.FileName.Split('.').Length - 1];

                #region PostID Algorithm
                string path = ConfigurationManager.AppSettings["POSTPATH"] != null ? ConfigurationManager.AppSettings["POSTPATH"].ToString() : "";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                int count = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly).Length;
                if (count > 0)
                {
                    int num = count + 1;
                    string strPreceding = string.Empty;
                    for (int i = 0; i < (6 - Convert.ToString(num).Length); i++)
                    {
                        strPreceding += "0";
                    }
                    strPostID = getUserId + DateTime.Now.Year.ToString() + "_POST" + strPreceding + num;
                }
                else
                {
                    strPostID = getUserId + DateTime.Now.Year.ToString() + "_POST000001";
                }
                #endregion

                #region Save Post                
                strFileName = Path.Combine(path, strPostID);
                file.SaveAs(strFileName + "." + strExtention);
                filePathRef = strFileName + "." + strExtention;
                #endregion

                #region Insert Post to Database
                RQRS.PostRequest PostRequest = new RQRS.PostRequest();
                PostRequest.strUserId = getUserId;
                PostRequest.strPostContent = postContent;
                PostRequest.strPostPath = strPostID + "." + strExtention;
                PostRequest.strPostID = strPostID;
                PostRequest.strFlag = "IP";
                string strRequest = JsonConvert.SerializeObject(PostRequest);
                RQRS.ResponseData Response = Baseclass.InvokeServiceReq("API/ManagePosts", strRequest, "POST");
                if (Response != null && Response.strStatus == "01")
                {
                    strStatus = Response.strStatus;
                    strMessage = Response.strResponse;
                }
                else
                {
                    strStatus = "00";
                    strMessage = "Unable to upload post";
                    if (System.IO.File.Exists(filePathRef))
                    {
                        System.IO.File.Delete(filePathRef);
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                strStatus = "00";
                strMessage = ex.ToString();
                if (System.IO.File.Exists(filePathRef))
                {
                    System.IO.File.Delete(filePathRef);
                }
            }
            return Json(new { Status = strStatus, Message = strMessage });
        }
        
        public ActionResult PostActions(string strPostID,string strFlag,string strCmnt)
        {
            string strResponse = string.Empty;
            string strStatus = string.Empty;
            string strMessage = string.Empty;
            string proPicPath = ConfigValue.strDomainURL + "Files/ProfilePic/";

            #region 
            RQRS.PostRequest PostRequest = new RQRS.PostRequest();
            PostRequest.strUserId = Convert.ToString(Session["userId"]);
            PostRequest.strPostID = strPostID;
            PostRequest.strFlag = strFlag;
            PostRequest.strPostContent = strCmnt;
            string strAPIMethod = strFlag == "SC" || strFlag == "SL" ? "FetchLikesandComments" : "ManageLikesandComments";
            string strRequest = JsonConvert.SerializeObject(PostRequest);
            RQRS.ResponseData Response = Baseclass.InvokeServiceReq("API/" + strAPIMethod, strRequest, "POST");
            if (Response != null && Response.strStatus == "01")
            {
                strStatus = Response.strStatus;
                strResponse = Response.strResponse;
            }
            else
            {
                strStatus = "00";
                strMessage = "Unable to process you request";
            }
            #endregion

            return Json(new { Status = strStatus, Message = strMessage, Response = strResponse, ProPicPath = proPicPath, UserName = Convert.ToString(Session["userName"]) });

        }
        public ActionResult ManageProfile(string strOtherUserId, string strUsername, string strFullName, string strMobile, string strMail, string strProPicPath,
            string strGender, string strDob, string strUserLocation, string strUserBio, string strFlag)
        {
            string strStatus = string.Empty;
            string strMessage = string.Empty;
            string strResponse = string.Empty;
            string proPicPath = ConfigValue.strDomainURL + "Files/ProfilePic/";
            string bikesPath = ConfigValue.strDomainURL + "Files/Bikes/";
            string getUserId = Convert.ToString(Session["userId"]);
            try
            {
                RQRS.ProfileRequest profileRequest= new RQRS.ProfileRequest();
                profileRequest.strUserId = getUserId;
                profileRequest.strOtherUserId = strOtherUserId;
                profileRequest.strUsername = strUsername;
                profileRequest.strFullName = strFullName;
                profileRequest.strMobile = strMobile;
                profileRequest.strMail = strMail;
                profileRequest.strProPicPath= strProPicPath;
                profileRequest.strGender = strGender;
                profileRequest.strDob = strDob;
                profileRequest.strUserLocation = strUserLocation;
                profileRequest.strUserBio = strUserBio;
                profileRequest.strFlag = strFlag;
                string strRequest = JsonConvert.SerializeObject(profileRequest);
                RQRS.ResponseData Response = Baseclass.InvokeServiceReq("API/Profile", strRequest, "POST");
                if (Response != null && Response.strStatus == "01")
                {
                    strStatus = Response.strStatus;
                    strMessage = Response.strResponse;
                }
                else
                {
                    strStatus = Response.strStatus;
                    strMessage = Response.strResponse;
                }
            }
            catch(Exception ex) 
            {
                strStatus = "00";
                strMessage = "Unable to connect Service(#05)";
            }

            return Json(new { Status = strStatus, Message = strMessage, ProPicPath = proPicPath, BikesPath = bikesPath });
        }
        public ActionResult GetUsers(string strName)
        {
            string strResponse = string.Empty;
            string strStatus = string.Empty;
            string strMessage = string.Empty;

            #region 
            RQRS.PostRequest PostRequest = new RQRS.PostRequest();
            PostRequest.strUserId = Convert.ToString(Session["userId"]);
            string strAPIMethod = "FetchLikesandComments";
            string strRequest = JsonConvert.SerializeObject(PostRequest);
            RQRS.ResponseData Response = Baseclass.InvokeServiceReq("API/" + strAPIMethod, strRequest, "POST");
            if (Response != null && Response.strStatus == "01")
            {
                strStatus = Response.strStatus;
                strResponse = Response.strResponse;
            }
            else
            {
                strStatus = "00";
                strMessage = "Unable to process you request";
            }
            #endregion

            return Json(new { Status = strStatus, Message = strMessage, Response = strResponse, UserName = Convert.ToString(Session["userName"]) });
        }
        public ActionResult SearchUsers(string strName)
        {
            string strResponse = string.Empty;
            string strStatus = string.Empty;
            string strMessage = string.Empty;
            string getUserId = Convert.ToString(Session["userId"]);
            string proPicPath = ConfigValue.strDomainURL + "Files/ProfilePic/";

            #region 
            RQRS.SearchRequest SearchRequest = new RQRS.SearchRequest();
            SearchRequest.strUserId = getUserId;
            SearchRequest.strName = strName;
            string strRequest = JsonConvert.SerializeObject(SearchRequest);
            RQRS.ResponseData Response = Baseclass.InvokeServiceReq("API/SearchUsers", strRequest, "POST");
            if (Response != null)
            {
                strStatus = Response.strStatus;
                strResponse = Response.strResponse;
            }
            else
            {
                strStatus = "00";
                strMessage = "Unable to process you request";
            }
            #endregion

            return Json(new { Status = strStatus, Message = strResponse, ProPicPath = proPicPath });
        }
    }
}