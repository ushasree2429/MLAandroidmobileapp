using MlaWebApi.EFModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Transactions;

namespace MlaWebApi.Controllers
{
    public class AddPostController : ApiController
    {
        // GET api/addpost
        public HttpResponseMessage Get(long userId)
        {
            HttpResponseMessage response;
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    using (var ctx = new ProjectEntities())
                    {
                        var innerQuery = (from frnd in ctx.Friends where frnd.FriendUserId == userId select frnd.GroupId).ToList();
                        var joinq = ctx.Group_key.Where(y => innerQuery.Contains(y.group_id) && y.user_id == userId).Select(s => new
                        {
                            groupkey = s.group_key1,
                            group_id = s.group_id
                        }).ToList();
                        var query = (from gr in joinq
                                     join post in ctx.Posts on gr.group_id equals post.groupid
                                     select new
                                     {
                                         post = post.post1,
                                         groupKey = gr.groupkey,
                                         sessionKey = post.session_key,
                                         username = post.owner,
                                         postType= post.postType
                                     }).ToList();

                        var query2 = ctx.Posts.Where(x => x.postType == "Public").Select(p => new
                        {
                            post = p.post1,
                            groupKey = "",
                            sessionKey = p.session_key,
                            username = p.owner,
                            postType = p.postType

                        }).ToList();

                        var defaultGrpId = (from grp in ctx.Group_table where grp.user_id == userId & grp.group_name.Contains("default") select grp.group_id).ToList();
                        long dId = defaultGrpId[0];
                        System.Diagnostics.Debug.WriteLine(" in post===>>>>" + dId);

                        var defaultGrpKey = (from grpKey in ctx.Group_key where grpKey.group_id == dId select grpKey.group_key1).ToList();
                        var dKey = defaultGrpKey[0];
                        var query3 = ctx.Posts.Where(x => x.postType == "Friends" & x.userId == userId).Select(s => new
                        {
                            post = s.post1,
                            groupKey = dKey,
                            sessionKey = s.session_key,
                            username = s.owner,
                            postType = s.postType
                        }).ToList();


                        var query4 = ctx.Posts.Where(x => x.postType == "Private" & x.userId == userId).Select(p => new {
                            post = p.post1,
                            groupKey = "",
                            sessionKey = p.session_key,
                            username = p.owner,
                            postType = p.postType
                        }).ToList();

                        scope.Complete();
                        //return query.Union(query2).AsQueryable();
                        response = Request.CreateResponse<IQueryable>(System.Net.HttpStatusCode.OK, query.Union(query2).Union(query3).Union(query4).AsQueryable());
                    }
                }
            }
            catch (Exception e)
            {
                response = Request.CreateResponse<Exception>(System.Net.HttpStatusCode.BadRequest, e);

            }
            return response;

        }

       

        // POST api/addpost
        public HttpResponseMessage Post(string post, long groupId, long userId ,String postType,String SessionKey,String userName, String digSig)
        {
            HttpResponseMessage response;
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {

                    using (var ctx = new ProjectEntities())
            {
                Post postobj = new Post();

                postobj.post1 = post;
                postobj.postType = postType;
                postobj.groupid = groupId;
                postobj.version_no = 1;
                postobj.digitalsignature = digSig;
                postobj.owner = userName;
                postobj.session_key = SessionKey;
                postobj.userId = userId;
              
                ctx.Posts.Add(postobj);
                ctx.SaveChanges();
                scope.Complete();
                  response = Request.CreateResponse<IQueryable>(System.Net.HttpStatusCode.OK, Enumerable.Empty<Post>().AsQueryable());


                        //return Enumerable.Empty<Post>().AsQueryable();


                    }
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

