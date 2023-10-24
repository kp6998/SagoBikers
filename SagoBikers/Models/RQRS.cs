using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SagoBikers.Models
{
    public class RQRS
    {
        #region Request
        public class Reg_and_Login
        {
            public string strEmail { get; set; }
            public string strMobileNo { get; set; }
            public string fullname { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public string userId { get; set; }
            public string flag { get; set; }
        };
        public class PostRequest
        {
            public string  strUserId{ get; set; }
            public string strOtherUserId { get; set; }
            public string strPostContent { get; set; }
            public string strPostPath { get; set; }
            public string strPostID { get; set; }
            public string strPostWithRef { get; set; }
            public string strPostWithId { get; set; }
            public string strFlag { get; set; }
            public int startWith { get; set; }
            public int EndWith { get; set; }
        }
        public class BikesRequest
        {
            public string strUserId { get; set;}
            public string strBikeId { get; set; }
            public string strImagePath { get; set; }
            public string strBikeName { get; set; }
            public string strBikeBrand { get; set; }
            public string strBikeModel { get; set; }
            public string strAnniversary { get; set; }
            public string strRegNumber { get; set; }
            public string strFlag { get; set; }
        }
        public class ProfileRequest
        {
            public string strUserId { get; set; }
            public string strOtherUserId { get; set; }
            public string strProPicPath { get; set; }
            public string strFullName { get; set; }
            public string strUsername { get; set; }
            public string strMobile { get; set; }
            public string strMail { get; set; }
            public string strGender { get; set; }
            public string strDob { get; set; }
            public string strUserLocation { get; set; }
            public string strUserBio { get; set; }
            public string strFlag { get; set; }
        }
        public class SearchRequest
        {
            public string strUserId { get; set; }
            public string strName { get; set; }
        }
        public class NotificationsReq
        {
            public string strUserId { get; set;}
        }
        public class ClubsRequest
        {
            public string strUserId { get; set;}
            public string strClubId { get; set; }
            public string strClubName { get; set; }
            public string strClubLocation { get; set; }
            public string strClubType { get; set; }
            public string strClubDescription { get; set; }
            public string strClubImagePath { get; set; }
            public string strClubPostAccess { get; set; }
            public string strClubForBikes { get; set; }
            public string strClubForLocation { get; set; }
            public string strClubCurrency { get; set; }
            public string strClubFee { get; set; }
            public string strClubCreator { get; set; }
            public string strFlag { get; set; }
        }
        public class ClubsActReq
        {
            public string strUserId { get; set; }
            public string strClubId { get; set; }
            public string strMemberPos { get; set; }
            public string strMemberStatus { get; set; }
            public string strFlag { get; set; }
        }

            #endregion

            #region Resopnse
            public class ResponseData
        {
            public string strStatus { get; set; }
            public string strResponse { get; set; }
            public string strErrorMessage { get; set; }  
        };
        #endregion

    }
}