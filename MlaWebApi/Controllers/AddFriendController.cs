using MlaWebApi.EFModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Transactions;
using System.Web.Http;

namespace MlaWebApi.Controllers
{
    public class AddFriendController : ApiController
    {
        // GET api/addfriend
        public HttpResponseMessage GetFriend(long userId)
        {
            HttpResponseMessage response;
            try
            {

                using (var db = new ProjectEntities())
                {

                    var innerQuery = (from frnd in db.Friends where frnd.userId == userId select frnd.FriendUserId);
                    System.Diagnostics.Debug.WriteLine("debug===>>>>" + innerQuery.ToList()[0]);

                    var res = db.registers.Where(x => !innerQuery.Contains(x.userId) && x.userId != userId && x.userId>0).Select(reg => new
                    {
                        userId = reg.userId,
                        userType = reg.userType,
                        userName = reg.userName
                    }).ToList();

                   // return res.AsQueryable();
                    response = Request.CreateResponse<IQueryable>(System.Net.HttpStatusCode.OK, res.AsQueryable());
                }
            }
            catch (Exception e)
            {
                response = Request.CreateResponse<Exception>(System.Net.HttpStatusCode.BadRequest, e);
            }
            return response;




        }



        // POST api/addfriend
        public HttpResponseMessage Post(long userId, long FriendUserId, String groupkey)
        {
            HttpResponseMessage response;
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    int groupId = 0;
                    using (var ctx = new ProjectEntities())
                    {
                        var user = ctx.Group_table.Where(x => x.user_id == userId).Select(grp => new
                        {
                            groupId = grp.group_id
                        }).ToList(); //add default name condition also
                        groupId = Convert.ToInt32(user[0].groupId);

                        Friend objfrnd = new Friend();
                        objfrnd.FriendUserId = FriendUserId;
                        objfrnd.userId = userId;
                        objfrnd.GroupId = groupId;
                        ctx.Friends.Add(objfrnd);
                        ctx.SaveChanges();



                        Group_key gk = new Group_key();
                        gk.group_id = groupId;
                        gk.group_key1 = groupkey;
                        gk.status = true;
                        gk.user_id = FriendUserId;
                        gk.version = 1;
                        ctx.Group_key.Add(gk);
                        ctx.SaveChanges();
                        scope.Complete();

                        

                        response = Request.CreateResponse<IQueryable>(System.Net.HttpStatusCode.OK, user.AsQueryable());

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
