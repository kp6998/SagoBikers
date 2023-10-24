using Newtonsoft.Json;
using SagoBikers.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using static SagoBikers.Models.RQRS;
using static System.Net.WebRequestMethods;

namespace SagoBikers.Controllers
{
    public class APIController : ApiController
    {
        public RQRS.ResponseData Reg_and_Login_srvc(RQRS.Reg_and_Login get_login_cred)
        {
            RQRS.ResponseData ResponseData = new RQRS.ResponseData();
            try
            {
                string strErrorMsg = string.Empty;
                Hashtable hs_Param = new Hashtable();
                hs_Param.Add("FULLNAME", get_login_cred.fullname ?? "");
                hs_Param.Add("USERNAME", get_login_cred.username ?? "");
                hs_Param.Add("PASSWORD", get_login_cred.password ?? "");
                hs_Param.Add("USERID", get_login_cred.userId ?? "");
                hs_Param.Add("FLAG", get_login_cred.flag);
                hs_Param.Add("MOBILE", get_login_cred.strMobileNo ?? "");
                hs_Param.Add("MAIL", get_login_cred.strEmail ?? "");
                DataSet dsOutput = DBHandler.ExecProcedureReturnsDataset(DBHandler.StoreProcedure.P_LOGIN_DETAILS, hs_Param, ref strErrorMsg);

                if (dsOutput != null && dsOutput.Tables.Count > 0 && dsOutput.Tables[0].Rows.Count > 0)
                {
                    ResponseData.strStatus = get_login_cred.flag == "R" ? Convert.ToString(dsOutput.Tables[0].Rows[0]["STATUS"]) : "01";
                    ResponseData.strResponse = get_login_cred.flag == "R" ? Convert.ToString(dsOutput.Tables[0].Rows[0]["MESSAGE"]) : JsonConvert.SerializeObject(dsOutput.Tables[0]);
                }
                else
                {
                    ResponseData.strStatus = "00";
                    ResponseData.strResponse = get_login_cred.flag == "R" ? "Unable to Register now, Please Try again!" : "Username or Password is Incorrect!";
                    ResponseData.strErrorMessage = strErrorMsg != "" ? strErrorMsg : "Problem occured while processing request";
                }
            }
            catch (Exception ex)
            {
                ResponseData.strStatus = "00";
                ResponseData.strResponse = "Problem occurred while processing(#01)";
                ResponseData.strErrorMessage = Convert.ToString(ex);
            }
            return ResponseData;
        }
        //[HttpPost]
        public RQRS.ResponseData MobileUpload()
        {
            string filePathRef = string.Empty;

            RQRS.ResponseData ResponseData = new RQRS.ResponseData();
            try
            {
                string strFileName = string.Empty;
                string strImgId = string.Empty;
                string strId = string.Empty;
                var request = HttpContext.Current.Request;
                var flag = request.Form["flag"];
                var userId = request.Form["userId"];
                var messageContent = request.Form["messageContent"];
                var photo = request.Files["photo"];
                string strExtention = photo.FileName.Split('.')[photo.FileName.Split('.').Length - 1];
                if (flag == "post")
                {
                    #region PostID Algorithm
                    string path = ConfigurationManager.AppSettings["POSTPATH"] != null ? ConfigurationManager.AppSettings["POSTPATH"].ToString() : "";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    strId = Convert.ToString(userId) + "POST";
                    strImgId = strId + DateTime.Now.ToString("yyyyMMddHHmmss");
                    #endregion

                    #region Save Post                
                    strFileName = Path.Combine(path, strImgId);
                    photo.SaveAs(strFileName + "." + strExtention);
                    filePathRef = strFileName + "." + strExtention;
                    #endregion

                    #region Upload Post
                    var result = JsonConvert.DeserializeObject<PostRequest>(messageContent);
                    RQRS.PostRequest postRequest = new RQRS.PostRequest();
                    postRequest.strUserId = Convert.ToString(userId);
                    postRequest.strPostContent = result.strPostContent;
                    postRequest.strPostPath = strImgId + "." + strExtention;
                    postRequest.strPostID = strId;
                    postRequest.strPostWithRef = result.strPostWithRef;
                    postRequest.strPostWithId = result.strPostWithId;
                    postRequest.strFlag = "IP";
                    ResponseData = ManagePosts(postRequest);
                    if (ResponseData.strStatus != "01")
                    {
                        if (System.IO.File.Exists(filePathRef))
                        {
                            System.IO.File.Delete(filePathRef);
                        }
                    }
                    #endregion
                }
                else if (flag == "bikes")
                {
                    var result = JsonConvert.DeserializeObject<BikesRequest>(messageContent);

                    #region BikeID Algorithm
                    string path = ConfigurationManager.AppSettings["BIKESPATH"] != null ? ConfigurationManager.AppSettings["BIKESPATH"].ToString() : "";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    if (result.strBikeId != "")
                    {
                        strId = result.strBikeId;
                    }
                    else
                    {
                        string userRef = Convert.ToString(userId) + "BIKE_";
                        int latestFileNum = 0;
                        //123BIKE_1_123
                        foreach (string filePath in Directory.EnumerateFiles(path, $"*{userRef}*"))
                        {
                            string latestFileName = Path.GetFileName(filePath);
                            string[] splitFileName = latestFileName.Split('_');
                            int fileNum = Convert.ToInt16(splitFileName[1]);
                            if (fileNum > latestFileNum)
                            {
                                latestFileNum = fileNum;
                            }
                        }
                        if (latestFileNum != 0)
                        {
                            strId = userRef + (latestFileNum + 1);
                        }
                        else
                        {
                            strId = userRef + "1";
                        }
                        strImgId = strId + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    }
                    #endregion

                    #region Save Bike
                    string partialFileName = strId + "*";
                    string[] filesToDelete = Directory.GetFiles(path, partialFileName);
                    foreach (string fileToDelete in filesToDelete)
                    {
                        System.IO.File.Delete(fileToDelete);
                    }
                    strFileName = Path.Combine(path, strImgId);
                    photo.SaveAs(strFileName + "." + strExtention);
                    filePathRef = strFileName + "." + strExtention;
                    #endregion

                    #region Upload Bike
                    RQRS.BikesRequest bikesRequest = new RQRS.BikesRequest();
                    bikesRequest.strUserId = Convert.ToString(userId);
                    bikesRequest.strImagePath = strImgId + "." + strExtention;
                    bikesRequest.strBikeId = strId;
                    bikesRequest.strBikeName = result.strBikeName;
                    bikesRequest.strBikeBrand = result.strBikeBrand;
                    bikesRequest.strBikeModel = result.strBikeModel;
                    bikesRequest.strAnniversary = result.strAnniversary;
                    bikesRequest.strRegNumber = result.strRegNumber;
                    bikesRequest.strFlag = result.strFlag;
                    ResponseData = BikeUpload(bikesRequest);
                    if (ResponseData.strStatus != "01")
                    {
                        if (System.IO.File.Exists(filePathRef))
                        {
                            System.IO.File.Delete(filePathRef);
                        }
                    }
                    #endregion
                }
                else if (flag == "proPic")
                {
                    #region Profile Pic Algorithm
                    string path = ConfigurationManager.AppSettings["PROPICPATH"] != null ? ConfigurationManager.AppSettings["PROPICPATH"].ToString() : "";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    strId = Convert.ToString(userId);
                    strImgId = strId + DateTime.Now.ToString("yyyyMMddHHmmss");
                    #endregion

                    #region Save Profile Pic        
                    string partialFileName = strId + "*";
                    string[] filesToDelete = Directory.GetFiles(path, partialFileName);
                    foreach (string fileToDelete in filesToDelete)
                    {
                        System.IO.File.Delete(fileToDelete);
                    }
                    strFileName = Path.Combine(path, strImgId);
                    photo.SaveAs(strFileName + "." + strExtention);
                    filePathRef = strFileName + "." + strExtention;
                    #endregion

                    #region Upload Profile Pic
                    var result = JsonConvert.DeserializeObject<ProfileRequest>(messageContent);
                    RQRS.ProfileRequest profileRequest = new RQRS.ProfileRequest();
                    profileRequest.strUserId = Convert.ToString(userId);
                    profileRequest.strOtherUserId = result.strOtherUserId;
                    profileRequest.strProPicPath = strImgId + "." + strExtention;
                    profileRequest.strFullName = result.strFullName;
                    profileRequest.strUsername = result.strUsername;
                    profileRequest.strMobile = result.strMobile;
                    profileRequest.strMail = result.strMail;
                    profileRequest.strGender = result.strGender;
                    profileRequest.strDob = result.strDob;
                    profileRequest.strUserLocation = result.strUserLocation;
                    profileRequest.strUserBio = result.strUserBio;
                    profileRequest.strFlag = result.strFlag;
                    ResponseData = Profile(profileRequest);
                    if (ResponseData.strStatus != "01")
                    {
                        if (System.IO.File.Exists(filePathRef))
                        {
                            System.IO.File.Delete(filePathRef);
                        }
                    }
                    #endregion
                }
                else if (flag == "clubPic")
                {
                    var result = JsonConvert.DeserializeObject<ClubsRequest>(messageContent);

                    #region ClubId Algorithm
                    string path = ConfigurationManager.AppSettings["CLUBPICPATH"] != null ? ConfigurationManager.AppSettings["CLUBPICPATH"].ToString() : "";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    if (result.strClubId != "")
                    {
                        strId = result.strClubId;
                    }
                    else
                    {
                        strId = "SBC" + DateTime.Now.ToString("yyyyMMddHHmmss") + DateTime.Now.Millisecond.ToString("D3");
                    }
                    strImgId = strId + DateTime.Now.ToString("yyyyMMddHHmmss");
                    #endregion

                    #region Save club pic
                    string partialFileName = strId + "*";
                    string[] filesToDelete = Directory.GetFiles(path, partialFileName);
                    foreach (string fileToDelete in filesToDelete)
                    {
                        System.IO.File.Delete(fileToDelete);
                    }
                    strFileName = Path.Combine(path, strImgId);
                    photo.SaveAs(strFileName + "." + strExtention);
                    filePathRef = strFileName + "." + strExtention;
                    #endregion

                    #region Upload Bike
                    RQRS.ClubsRequest clubsRequest = new RQRS.ClubsRequest();
                    clubsRequest = result;
                    clubsRequest.strUserId = Convert.ToString(userId);
                    clubsRequest.strClubId = strId;
                    clubsRequest.strClubImagePath = strImgId + "." + strExtention;
                    ResponseData = ManageClubs(clubsRequest);
                    if (ResponseData.strStatus != "01")
                    {
                        if (System.IO.File.Exists(filePathRef))
                        {
                            System.IO.File.Delete(filePathRef);
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                ResponseData.strStatus = "00";
                ResponseData.strResponse = "Unable to connect database(#05)";
                ResponseData.strErrorMessage = Convert.ToString(ex);
                if (System.IO.File.Exists(filePathRef))
                {
                    System.IO.File.Delete(filePathRef);
                }
            }
            return ResponseData;
        }
        public RQRS.ResponseData ManagePosts(RQRS.PostRequest PostRequest)
        {
            RQRS.ResponseData ResponseData = new RQRS.ResponseData();
            try
            {
                string strErrorMsg = string.Empty;
                Hashtable hs_Param = new Hashtable();
                hs_Param.Add("USERID", PostRequest.strUserId);
                hs_Param.Add("POSTCONTENT", PostRequest.strPostContent ?? "");
                hs_Param.Add("POSTWITHREF", PostRequest.strPostWithRef ?? "");
                hs_Param.Add("POSTWITHID", PostRequest.strPostWithId ?? "");
                hs_Param.Add("POSTPATH", PostRequest.strPostPath ?? "");
                hs_Param.Add("POSTID", PostRequest.strPostID ?? "");
                hs_Param.Add("FLAG", PostRequest.strFlag ?? "");
                int ResultCount = 0;
                bool result = DBHandler.InsertUpdateDelete(DBHandler.StoreProcedure.P_MANAGE_POSTS, hs_Param, ref strErrorMsg, ref ResultCount);
                if (ResultCount > 0)
                {
                    ResponseData.strStatus = "01";
                    if (PostRequest.strFlag == "IP")
                    {
                        ResponseData.strResponse = "Your Post Successfully Uploaded!";
                    }
                    else if(PostRequest.strFlag == "DP")
                    {
                        ResponseData.strResponse = "Your Post Successfully Deleted!";
                    }
                    else if (PostRequest.strFlag == "UP")
                    {
                        ResponseData.strResponse = "Your Post Successfully Updated!";
                    }
                }
                else
                {
                    if (strErrorMsg != "" && strErrorMsg != null)
                    {
                        ResponseData.strStatus = "00";
                        ResponseData.strErrorMessage = strErrorMsg;
                        if (PostRequest.strFlag == "IP")
                        {
                            ResponseData.strResponse = "Unable to upload post";
                        }
                        else if (PostRequest.strFlag == "DP")
                        {
                            ResponseData.strResponse = "Unable to delete post";
                        }
                        else if (PostRequest.strFlag == "UP")
                        {
                            ResponseData.strResponse = "Unable to update post";
                        }
                    }
                    else
                    {
                        ResponseData.strStatus = "00";
                        ResponseData.strResponse = "Unable to process now, Please try again!";
                        ResponseData.strErrorMessage = "Problem occured!";
                    }
                }
            }
            catch (Exception ex)
            {
                ResponseData.strStatus = "00";
                ResponseData.strResponse = "Problem occurred while processing(#01)";

            }
            return ResponseData;
        }
        public RQRS.ResponseData Posts(RQRS.PostRequest postRequest)
        {
            RQRS.ResponseData ResponseData = new RQRS.ResponseData();
            try
            {
                string strErrorMsg = string.Empty;
                DataSet dsOutput = new DataSet();
                Hashtable hs_Param = new Hashtable();
                hs_Param.Add("USERID", postRequest.strUserId);
                hs_Param.Add("OTHERUSERID", postRequest.strOtherUserId ?? "");
                hs_Param.Add("FLAG", postRequest.strFlag ?? "");
                hs_Param.Add("STARTFROM", postRequest.startWith);
                hs_Param.Add("ENDWITH", postRequest.EndWith);
                hs_Param.Add("POSTID", postRequest.strPostID ?? "");
                dsOutput = DBHandler.ExecProcedureReturnsDataset(DBHandler.StoreProcedure.P_POSTS, hs_Param, ref strErrorMsg);
                if (dsOutput != null && dsOutput.Tables.Count > 0 && dsOutput.Tables[0].Rows.Count > 0)
                {
                    ResponseData.strStatus = "01";
                    ResponseData.strResponse = JsonConvert.SerializeObject(dsOutput);
                }
                else
                {
                    ResponseData.strStatus = "00";
                    ResponseData.strResponse = "No Posts found";
                    ResponseData.strErrorMessage = strErrorMsg != "" ? strErrorMsg : "No Records found";
                }
            }
            catch (Exception ex)
            {
                ResponseData.strStatus = "00";
                ResponseData.strResponse = "Problem occurred while processing(#01)";
                ResponseData.strErrorMessage = Convert.ToString(ex);
            }

            return ResponseData;
        }
        public RQRS.ResponseData CheckUserName(RQRS.Reg_and_Login get_login_cred)
        {
            RQRS.ResponseData ResponseData = new RQRS.ResponseData();
            try
            {
                string strErrorMsg = string.Empty;
                Hashtable hs_Param = new Hashtable();
                hs_Param.Add("FULLNAME", get_login_cred.fullname ?? "");
                hs_Param.Add("USERNAME", get_login_cred.username ?? "");
                hs_Param.Add("PASSWORD", get_login_cred.password ?? "");
                hs_Param.Add("USERID", get_login_cred.userId ?? "");
                hs_Param.Add("FLAG", get_login_cred.flag);
                hs_Param.Add("MOBILE", "");
                hs_Param.Add("MAIL", "");
                DataSet dsOutput = DBHandler.ExecProcedureReturnsDataset(DBHandler.StoreProcedure.P_LOGIN_DETAILS, hs_Param, ref strErrorMsg);
                if (dsOutput != null && dsOutput.Tables.Count > 0 && dsOutput.Tables[0].Rows.Count > 0)
                {
                    ResponseData.strStatus = dsOutput.Tables[0].Rows[0]["STATUS"].ToString();
                    ResponseData.strResponse = ResponseData.strStatus =="00"  ? "Username already taken, please try something else" : "";
                }
                else
                {
                    ResponseData.strStatus = "00";
                }
            }
            catch (Exception ex)
            {
                ResponseData.strStatus = "00";
            }
            return ResponseData;
        }
        public RQRS.ResponseData BikeUpload(RQRS.BikesRequest bikesRequest)
        {
            ResponseData ResponseData = new ResponseData();
            try
            {
                string strErrorMsg = string.Empty;
                Hashtable hs_Param = new Hashtable();
                hs_Param.Add("USERID", bikesRequest.strUserId ?? "");
                hs_Param.Add("BIKEID", bikesRequest.strBikeId ?? "");
                hs_Param.Add("IMAGEPATH", bikesRequest.strImagePath ?? "");
                hs_Param.Add("BIKENAME", bikesRequest.strBikeName ?? "");
                hs_Param.Add("BIKEBRAND", bikesRequest.strBikeBrand ?? "");
                hs_Param.Add("BIKEMODEL", bikesRequest.strBikeModel ?? "");
                hs_Param.Add("ANNIVERSARY", bikesRequest.strAnniversary ?? "");
                hs_Param.Add("REGNUMBER", bikesRequest.strRegNumber ?? "");
                hs_Param.Add("FLAG", bikesRequest.strFlag ?? "");
                int ResultCount = 0;
                bool result = DBHandler.InsertUpdateDelete(DBHandler.StoreProcedure.P_MANAGE_BIKES, hs_Param, ref strErrorMsg, ref ResultCount);
                if (ResultCount > 0)
                {
                    ResponseData.strStatus = "01";
                    ResponseData.strResponse = "Your bike successfully updated!";
                }
                else
                {
                    if (strErrorMsg != "" && strErrorMsg != null)
                    {
                        ResponseData.strStatus = "00";
                        ResponseData.strResponse = "Unable update bike details";
                        ResponseData.strErrorMessage = strErrorMsg;
                    }
                    else
                    {
                        ResponseData.strStatus = "00";
                        ResponseData.strResponse = "Unable to update now, Please Try again!";
                        ResponseData.strErrorMessage = "Problem occured while Insert or Update bike!";
                    }
                }
            }
            catch(Exception ex)
            {
                ResponseData.strStatus = "00";
                ResponseData.strResponse = "Problem occured while upload data(#01)";
                ResponseData.strErrorMessage = Convert.ToString(ex);
            }
            return ResponseData;
        }
        public RQRS.ResponseData ManageLikesandComments(RQRS.PostRequest PostRequest)
        {
            RQRS.ResponseData ResponseData = new RQRS.ResponseData();
            try
            {
                string strErrorMsg = string.Empty;
                Hashtable hs_Param = new Hashtable();
                hs_Param.Add("POSTID", PostRequest.strPostID);
                hs_Param.Add("USERID", PostRequest.strUserId ?? "");
                hs_Param.Add("COMMENT", !string.IsNullOrEmpty(PostRequest.strPostContent) ? CommonFunctions.EncryptString(PostRequest.strPostID, PostRequest.strPostContent) : "");
                hs_Param.Add("FLAG", PostRequest.strFlag);
                int ResultCount = 0;
                bool result = DBHandler.InsertUpdateDelete(DBHandler.StoreProcedure.P_MANAGE_LIKES_COMMENTS, hs_Param, ref strErrorMsg, ref ResultCount);
                if (ResultCount > 0)
                {
                    ResponseData.strStatus = "01";
                    ResponseData.strResponse = PostRequest.strPostContent;// "Like updated";
                }
                else
                {
                    ResponseData.strStatus = "00";
                    ResponseData.strResponse = "Unable to process your request";
                    ResponseData.strErrorMessage = PostRequest.strFlag == "IL" ? "Unable to insert like" : (PostRequest.strFlag == "RL" ? "Unable to delete like" : (PostRequest.strFlag == "IC" ? "Unable to insert comment" : "Unable to process your request"));
                }
            }
            catch (Exception ex)
            {
                ResponseData.strStatus = "00";
                ResponseData.strResponse = "Unable to process your request";
                ResponseData.strErrorMessage = ex.ToString();
            }

            return ResponseData;
        }
        public RQRS.ResponseData FetchLikesandComments(RQRS.PostRequest PostRequest)
        {
            RQRS.ResponseData ResponseData = new RQRS.ResponseData();
            try
            {
                string strErrorMsg = string.Empty;
                Hashtable hs_Param = new Hashtable();
                hs_Param.Add("POSTID", PostRequest.strPostID);
                hs_Param.Add("USERID", PostRequest.strUserId ?? "");
                hs_Param.Add("COMMENT", "");
                hs_Param.Add("FLAG", PostRequest.strFlag);
                DataSet dsOutput = DBHandler.ExecProcedureReturnsDataset(DBHandler.StoreProcedure.P_MANAGE_LIKES_COMMENTS, hs_Param, ref strErrorMsg);

                if (dsOutput != null && dsOutput.Tables.Count > 0 && dsOutput.Tables[0].Rows.Count > 0)
                {
                    ResponseData.strStatus = "01";
                    if (PostRequest.strFlag == "SC")
                    {
                        var CommentDetails = from _row in dsOutput.Tables[0].AsEnumerable()
                                             select new
                                             {
                                                 USERID = _row["USERID"].ToString(),
                                                 PROPICPATH = _row["PROPICPATH"].ToString(),
                                                 USERNAME = _row["USERNAME"].ToString(),
                                                 USER_VERIFIED = _row["USER_VERIFIED"].ToString(),
                                                 POSTID = _row["POSTID"].ToString(),
                                                 COMMENTS = CommonFunctions.DecryptString(_row["POSTID"].ToString(), _row["COMMENTS"].ToString()),
                                                 COMMENTDATETIME = (DateTime)_row["COMMENTDATETIME"],
                                             };

                        DataTable CommentTable = CommonFunctions.ConvertToDataTable(CommentDetails);
                        CommentTable.TableName = "Temp";
                        DataSet mergeTables = new DataSet();
                        mergeTables.Tables.Add(CommentTable);
                        // mergeTables.Tables.Add(dsOutput.Tables[1].Copy());
                        ResponseData.strResponse = JsonConvert.SerializeObject(mergeTables);
                    }
                    else
                    {
                        ResponseData.strResponse = JsonConvert.SerializeObject(dsOutput.Tables[0]);
                    }
                }
                else
                {
                    ResponseData.strStatus = "00";
                    ResponseData.strResponse = PostRequest.strFlag == "SC" ? "No comments found" : "No likes found";
                    ResponseData.strErrorMessage = PostRequest.strFlag == "SC" ? "Unable to fetch comments" : "Unable to fetch likes";
                }
            }
            catch (Exception ex)
            {
                ResponseData.strStatus = "00";
                ResponseData.strResponse = "Problem occured while upload data(#01)";
            }
            return ResponseData;
        }
        public RQRS.ResponseData Profile(RQRS.ProfileRequest profileRequest)
        {
            RQRS.ResponseData ResponseData = new RQRS.ResponseData();
            string strErrorMsg = string.Empty;
            try
            {
                if (profileRequest.strFlag == "EU" && profileRequest.strProPicPath == "")
                {
                    string path = ConfigurationManager.AppSettings["PROPICPATH"] != null ? ConfigurationManager.AppSettings["PROPICPATH"].ToString() : "";
                    string partialFileName = Convert.ToString(profileRequest.strUserId) + "*";
                    string[] filesToDelete = Directory.GetFiles(path, partialFileName);
                    foreach (string fileToDelete in filesToDelete)
                    {
                        System.IO.File.Delete(fileToDelete);
                    }
                }
                Hashtable hs_Param = new Hashtable();
                hs_Param.Add("USERID", profileRequest.strUserId ?? "");
                hs_Param.Add("OTHERUSERID", profileRequest.strOtherUserId ?? "");
                hs_Param.Add("FULLNAME", profileRequest.strFullName ?? "");
                hs_Param.Add("USERNAME", profileRequest.strUsername ?? "");
                hs_Param.Add("PROPICPATH", profileRequest.strProPicPath ?? "");
                hs_Param.Add("MOBILE", profileRequest.strMobile ?? "");
                hs_Param.Add("MAIL", profileRequest.strMail ?? "");
                hs_Param.Add("GENDER", profileRequest.strGender ?? "");
                hs_Param.Add("DOB", profileRequest.strDob ?? "");
                hs_Param.Add("USERBIO", profileRequest.strUserBio ?? "");
                hs_Param.Add("USERLOCATION", profileRequest.strUserLocation ?? "");
                hs_Param.Add("FLAG", profileRequest.strFlag ?? "");
                if (profileRequest.strFlag == "EU" || profileRequest.strFlag == "IF" || profileRequest.strFlag == "RF")
                {
                    int ResultCount = 0;
                    bool result = DBHandler.InsertUpdateDelete(DBHandler.StoreProcedure.P_PROFILE, hs_Param, ref strErrorMsg, ref ResultCount);
                    if (ResultCount > 0)
                    {
                        ResponseData.strStatus = "01";
                        ResponseData.strResponse = profileRequest.strFlag == "EU" ? profileRequest.strProPicPath : "IF / RF";
                    }
                    else
                    {
                        ResponseData.strStatus = "00";
                        ResponseData.strResponse = profileRequest.strFlag == "EU" ? "Unable to update profile" : "Unable to process your request";
                        ResponseData.strErrorMessage = strErrorMsg == "" ? "Unable to process your request" : ResponseData.strErrorMessage;
                    }
                }
                else
                {
                    DataSet dsOutput = new DataSet();
                    dsOutput = DBHandler.ExecProcedureReturnsDataset(DBHandler.StoreProcedure.P_PROFILE, hs_Param, ref strErrorMsg);
                    if (dsOutput != null && dsOutput.Tables.Count > 0 && dsOutput.Tables[0].Rows.Count > 0)
                    {
                        string JSONString = string.Empty;
                        JSONString = JsonConvert.SerializeObject(dsOutput);
                        ResponseData.strStatus = "01";
                        ResponseData.strResponse = JSONString;
                    }
                    else
                    {
                        ResponseData.strStatus = "00";
                        ResponseData.strErrorMessage = strErrorMsg != "" ? strErrorMsg : "No Records found";
                        if(profileRequest.strFlag == "FFR") ResponseData.strResponse = "No followers found";
                        else if (profileRequest.strFlag == "FFG") ResponseData.strResponse = "No followings found";
                        else ResponseData.strResponse = "Profile not found";
                    }
                }
            }
            catch (Exception ex)
            {
                ResponseData.strStatus = "00";
                ResponseData.strResponse = "Unable to connect database(#05)";
                ResponseData.strErrorMessage = Convert.ToString(ex);
            }
            return ResponseData;
        }
        public RQRS.ResponseData Notifications(RQRS.NotificationsReq notificationsReq)
        {
            ResponseData ResponseData = new ResponseData();
            string strErrorMsg = string.Empty;
            DataSet dsOutput = new DataSet();
            try
            {
                Hashtable hs_Param = new Hashtable();
                hs_Param.Add("USERID", notificationsReq.strUserId);
                dsOutput = DBHandler.ExecProcedureReturnsDataset(DBHandler.StoreProcedure.P_NOTIFICATIONS, hs_Param, ref strErrorMsg);
                if (dsOutput != null && dsOutput.Tables.Count > 0 && dsOutput.Tables[0].Rows.Count > 0)
                {
                    ResponseData.strStatus = "01";
                    ResponseData.strResponse = JsonConvert.SerializeObject(dsOutput);
                }
                else
                {
                    ResponseData.strStatus = "00";
                    ResponseData.strResponse = "No notifications available";
                    ResponseData.strErrorMessage = strErrorMsg != "" ? strErrorMsg : "No records found";
                }
            }
            catch (Exception ex)
            {
                ResponseData.strStatus = "00";
                ResponseData.strResponse = "Problem occurred while processing(#01)";
                ResponseData.strErrorMessage = Convert.ToString(ex);
            }
            return ResponseData;
        }
        //public static int a = 0;
        public RQRS.ResponseData SearchUsers(RQRS.SearchRequest SearchRequest)
        {
            //Debug.WriteLine(a++ + " - " + SearchRequest.strName);
            RQRS.ResponseData ResponseData = new RQRS.ResponseData();
            try
            {
                string strErrorMsg = string.Empty;
                DataSet dsOutput = new DataSet();
                Hashtable hs_Param = new Hashtable();
                hs_Param.Add("USERID", SearchRequest.strUserId);
                hs_Param.Add("USERNAME", SearchRequest.strName);
                dsOutput = DBHandler.ExecProcedureReturnsDataset(DBHandler.StoreProcedure.P_SEARCH_USERS, hs_Param, ref strErrorMsg);
                if (dsOutput != null && dsOutput.Tables.Count > 0 && dsOutput.Tables[0].Rows.Count > 0)
                {
                    ResponseData.strStatus = "01";
                    ResponseData.strResponse = JsonConvert.SerializeObject(dsOutput);
                }
                else
                {
                    ResponseData.strStatus = "00";
                    ResponseData.strResponse = "No name available";
                    ResponseData.strErrorMessage = strErrorMsg != "" ? strErrorMsg : "No records found";
                }
            }
            catch (Exception ex)
            {
                ResponseData.strStatus = "00";
                ResponseData.strResponse = "Problem occurred while processing(#01)";
                ResponseData.strErrorMessage = Convert.ToString(ex);
            }
            //Debug.WriteLine(a + " - " + ResponseData.strStatus);
            return ResponseData;
        }
        public RQRS.ResponseData ManageClubs(RQRS.ClubsRequest clubsRequest)
        {
            RQRS.ResponseData ResponseData = new RQRS.ResponseData();
            string strErrorMsg = string.Empty;
            try
            {
                Hashtable hs_Param = new Hashtable();
                hs_Param.Add("USER_ID", clubsRequest.strUserId ?? "");
                hs_Param.Add("CLUB_ID", clubsRequest.strClubId ?? "");
                hs_Param.Add("CLUB_NAME", clubsRequest.strClubName ?? "");
                hs_Param.Add("CLUB_LOCATION", clubsRequest.strClubLocation ?? "");
                hs_Param.Add("CLUB_TYPE", clubsRequest.strClubType ?? "");
                hs_Param.Add("CLUB_DESCRIPTION", clubsRequest.strClubDescription ?? "");
                hs_Param.Add("CLUB_IMAGEPATH", clubsRequest.strClubImagePath ?? "");
                hs_Param.Add("CLUB_POSTACCESS", clubsRequest.strClubPostAccess ?? "");
                hs_Param.Add("CLUB_FOR_BIKES", clubsRequest.strClubForBikes ?? "");
                hs_Param.Add("CLUB_FOR_LOCATION", clubsRequest.strClubForLocation ?? "");
                hs_Param.Add("CLUB_CURRENCY", clubsRequest.strClubCurrency ?? "");
                hs_Param.Add("CLUB_FEE", clubsRequest.strClubFee ?? "");
                hs_Param.Add("CLUB_CREATOR", clubsRequest.strClubCreator ?? "");
                hs_Param.Add("FLAG", clubsRequest.strFlag ?? "");
                if (clubsRequest.strFlag == "IC" || clubsRequest.strFlag == "CU")
                {
                    int ResultCount = 0;
                    bool result = DBHandler.InsertUpdateDelete(DBHandler.StoreProcedure.P_MANAGE_CLUBS, hs_Param, ref strErrorMsg, ref ResultCount);
                    if (ResultCount > 0)
                    {
                        ResponseData.strStatus = "01";
                        ResponseData.strResponse = "Your club successfully " + (clubsRequest.strFlag == "IC" ? "created!" : "updated");
                    }
                    else
                    {
                        ResponseData.strStatus = "00";
                        ResponseData.strResponse = "Unable to " + (clubsRequest.strFlag == "IC" ? "create" : "update") + " now, Please try again!";
                        ResponseData.strErrorMessage = strErrorMsg ?? "Problem occured while insert club";
                    }
                }
                else
                {
                    DataSet dsOutput = new DataSet();
                    dsOutput = DBHandler.ExecProcedureReturnsDataset(DBHandler.StoreProcedure.P_MANAGE_CLUBS, hs_Param, ref strErrorMsg);
                    if (dsOutput != null && dsOutput.Tables.Count > 0 && dsOutput.Tables[0].Rows.Count > 0)
                    {
                        ResponseData.strStatus = "01";
                        ResponseData.strResponse = JsonConvert.SerializeObject(dsOutput);
                    }
                    else
                    {
                        ResponseData.strStatus = "00";
                        ResponseData.strResponse = "Clubs not found";
                        ResponseData.strErrorMessage = strErrorMsg != "" ? strErrorMsg : "No Records found";
                    }
                }
            }
            catch(Exception ex) 
            {
                ResponseData.strStatus = "00";
                ResponseData.strResponse = "Problem occured while processing(#01)";
                ResponseData.strErrorMessage = Convert.ToString(ex);
            }
            return ResponseData;
        }
        public RQRS.ResponseData ClubsActivities(RQRS.ClubsActReq clubsActReq)
        {
            ResponseData ResponseData = new ResponseData();
            string strErrorMsg = string.Empty;
            try
            {
                Hashtable hs_Param = new Hashtable();
                hs_Param.Add("USER_ID", clubsActReq.strUserId ?? "");
                hs_Param.Add("CLUB_ID", clubsActReq.strClubId ?? "");
                hs_Param.Add("MEMBER_POSITION", clubsActReq.strMemberPos ?? "");
                hs_Param.Add("MEMBER_STATUS", clubsActReq.strMemberStatus ?? "");
                hs_Param.Add("FLAG", clubsActReq.strFlag ?? "");
                if (clubsActReq.strFlag == "IM" || clubsActReq.strFlag == "RM")
                {
                    int ResultCount = 0;
                    bool result = DBHandler.InsertUpdateDelete(DBHandler.StoreProcedure.P_CLUBS_ACTIVITIES, hs_Param, ref strErrorMsg, ref ResultCount);
                    if (ResultCount > 0)
                    {
                        ResponseData.strStatus = "01";
                        ResponseData.strResponse = clubsActReq.strMemberStatus == "J" ? "Joined successfully" : "Requested successfully";
                    }
                    else
                    {
                        ResponseData.strStatus = "00";
                        ResponseData.strResponse = "Unable to join now, Please try again!";
                        ResponseData.strErrorMessage = strErrorMsg ?? "Problem occured while insert member";
                    }
                }
                else
                {

                }
            }
            catch(Exception ex)
            {
                ResponseData.strStatus = "00";
                ResponseData.strResponse = "Problem occured while processing(#01)";
                ResponseData.strErrorMessage = Convert.ToString(ex);
            }
            return ResponseData;
        }
    }
}
