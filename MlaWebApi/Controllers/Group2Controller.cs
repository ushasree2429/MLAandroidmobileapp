using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MlaWebApi.EFModels;

namespace MlaWebApi.Controllers
{
    public class Group2Controller : ApiController
    {
        public HttpResponseMessage GroupList(long loggedid)
        {
            HttpResponseMessage response;
            try
            {

                using (var db = new ProjectEntities())
                {


                    String defaultGroup = "defaultgroup";

                    var nestedQuery = (from frnd in db.Friends where (frnd.userId == loggedid) || (frnd.FriendUserId == loggedid) select frnd.GroupId).ToList();
                    var query = db.Group_table.Where(x => nestedQuery.Contains(x.group_id) && !x.group_name.Contains(defaultGroup))
                    .Select(p => new
                    {
                        group_name = p.group_name,
                        group_id = p.group_id
                    }).ToList();

                   // return query.AsQueryable();
                    response = Request.CreateResponse<IQueryable>(System.Net.HttpStatusCode.OK, query.AsQueryable());

                }
            }
            catch (Exception e)
            {
                response = Request.CreateResponse<Exception>(System.Net.HttpStatusCode.BadRequest, e);
            }
            return response;


        }

        public HttpResponseMessage GetGroupKey(long groupId,long userId)
        {
            HttpResponseMessage response;
            try
            {

                using (var db = new ProjectEntities())
                {
                    var query = db.Group_key.Where(x => x.group_id == groupId & x.user_id == userId).Select(grp => new
                    {
                        groupkey = grp.group_key1
                    }).ToList();
                   // return query.AsQueryable();
                    response = Request.CreateResponse<IQueryable>(System.Net.HttpStatusCode.OK, query.AsQueryable());

                }
            }
            catch (Exception e)
            {
                response = Request.CreateResponse<Exception>(System.Net.HttpStatusCode.BadRequest, e);
            }
            return response;

        }

        public HttpResponseMessage GetSelfGid(long UserId)
        {
            HttpResponseMessage response;
            try
            {
                String name = "defaultgroup" + UserId;
                using (var db = new ProjectEntities())
                {
                    var query = db.Group_table.Where(x => x.user_id == UserId & x.group_name == name).Select(grp => new
                    {
                        group_id = grp.group_id
                    }).ToList();
                   // return query.AsQueryable();
                    response = Request.CreateResponse<IQueryable>(System.Net.HttpStatusCode.OK, query.AsQueryable());

                }
            }
            catch (Exception e)
            {
                response = Request.CreateResponse<Exception>(System.Net.HttpStatusCode.BadRequest, e);
            }
            return response;
        }
    }
} 
